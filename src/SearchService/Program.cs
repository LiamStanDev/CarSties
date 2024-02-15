using Polly;
using SearchService.Data;
using SearchService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationSerivces(builder.Configuration);


var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
	await Policy.Handle<TimeoutException>()
		.WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(10))
		.ExecuteAndCaptureAsync(async () => await DbInitializer.Initialize(app));
});

app.Run();

