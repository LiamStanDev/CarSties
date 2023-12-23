using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());


builder.Services.AddMassTransit(c =>
{
	c.UsingRabbitMq((context, cfg) =>
	{
		cfg.ConfigureEndpoints(context);
	});
});


var app = builder.Build();

// if (app.Environment.IsDevelopment()) {}

app.UseAuthorization();

app.MapControllers();


/// pooling 的關係程式會一直卡在 polling。原因是
/// app.Run() 在 await 之後，會等到 DbInitializer 完成
/// 才會執行。
/// 解決方法一：使用一個獨立的 Task.Run() 執行
/// 解決方法二：使用app.Lifetime.ApplicationStarted，
/// 因為被註冊的 call back 會在 app.Run 執行之後執行，
/// 因為 lifetime 為 Application`Started` 。
app.Lifetime.ApplicationStarted.Register(async () =>
{

	try
	{

		await DbInitializer.Initialize(app);
	}
	catch (Exception ex)
	{
		var logger = app.Services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "Problem Initializing MongoDB");
	}
});

app.Run();


// using Polly to make polling policy
static IAsyncPolicy<HttpResponseMessage> GetPolicy() =>
	HttpPolicyExtensions.HandleTransientHttpError()
	.OrResult(res => res.StatusCode == System.Net.HttpStatusCode.NotFound)
	.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3)); // 裡面的 Func 給參數的話，會變成 exponential pooling


