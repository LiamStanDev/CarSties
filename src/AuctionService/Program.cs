using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// builder.WebHost.UseUrls("http://localhost:7001");

builder.Services.AddDbContext<AuctionDbContext>(opt => {
    opt.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]);

});

// 取得現在 App Domain (這個進程 process 下) 下所有的 asmbly
// AddAutoMapper 會找哪個 class 繼承 Profile
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment()) { }

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope()) {
    try {

        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await DbInitializer.InitializeAsync(context);
    } catch (Exception ex) {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Problem Initialize Auction Database");
    }
}

app.Run();
