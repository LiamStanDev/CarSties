using AuctionService.Data;
using AuctionService.Extensions;
using AuctionServices.Services;
using Npgsql;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcAuctionService>();

// when the capture method is async function, use WaitAndRetryAsync and
// ExecuteAndCaptureAsync.
await Policy
	.HandleInner<NpgsqlException>()
	.WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(10))
	.ExecuteAndCaptureAsync(async () => await DbInitializer.InitializeAsync(app));


app.Run();
