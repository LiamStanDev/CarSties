using AuctionService.Consumer;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
	opt.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]);
});

builder.Services.AddAuthentication(cfg =>
{
	cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(cfg =>
{
	// Authority means when Application receive JWT, it will 
	// call the Authority to validate this token.
	cfg.Authority = builder.Configuration["IdentityServiceUrl"];

	// wheter use HTTPS to communicate with authority, if true
	// this application will only accept Authority Meta with HTTPS 
	cfg.RequireHttpsMetadata = false; // because our identity server run on http
	cfg.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateAudience = false,
		NameClaimType = "username",
	};
});

// Get all assemblies in this App Domain only inside this process 
// AddAutoMapper will find class which inherited Profile
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddMassTransit(c =>
{
	c.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
	c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

	c.UsingRabbitMq(
		(context, cfg) =>
		{
			cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", host =>
					{
						// GetValue can set default value
						host.Username(builder.Configuration.GetValue("RabbitMQ:Username", "guest"));
						host.Password(builder.Configuration.GetValue("RabbitMQ:Password", "guest"));
					});
			cfg.ConfigureEndpoints(context);
		}
	);

	// 添加 OutBox 用來確保 Consistency
	// 要使用 Masstransit.EntityframworeCore package
	c.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
	{
		o.UsePostgres();
		o.UseBusOutbox();
		o.QueryDelay = TimeSpan.FromSeconds(10); // 向 RabbitMQ 重傳間隔
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
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
