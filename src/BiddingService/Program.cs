using BiddingService.Extensions;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

await Policy.Handle<TimeoutException>()
	.WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(10))
	.ExecuteAndCaptureAsync(async () =>
		await DB.InitAsync("BidDB",
		MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("BidDbConnection")))
	);

app.Run();
