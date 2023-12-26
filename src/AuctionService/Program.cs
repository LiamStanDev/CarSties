using AuctionService.Consumer;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// builder.WebHost.UseUrls("http://localhost:7001");

builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
	opt.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]);

});

// 取得現在 App Domain (這個進程 process 下) 下所有的 asmbly
// AddAutoMapper 會找哪個 class 繼承 Profile
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddMassTransit(c =>
{
	c.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
	c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

	c.UsingRabbitMq((context, cfg) =>
	{
		cfg.ConfigureEndpoints(context);
	});

	// 添加 OutBox 用來確保 Consistency
	c.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
	{
		o.UsePostgres();
		o.UseBusOutbox();
		o.QueryDelay = TimeSpan.FromSeconds(10); // 向 RabbitMQ 重傳間隔
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment()) { }

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
	try
	{

		var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

		await DbInitializer.InitializeAsync(context);
	}
	catch (Exception ex)
	{
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "Problem Initialize Auction Database");
	}
}

app.Run();
