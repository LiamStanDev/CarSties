using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(c =>
{
    c.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    // KebabCase is list 'this-is-a-template'
    // 將默認的 exchange 與 queue 的名稱 AuctionCreated 添加前綴為
    // search-AuctionCreated 以免多個微服務命名衝突。
    c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
    c.UsingRabbitMq(
        (context, cfg) =>
        {
            // 添加 Retry
            cfg.ReceiveEndpoint(
                "search-auction-created",
                e =>
                {
                    e.UseMessageRetry(r => r.Interval(5, 5));

                    e.ConfigureConsumer<AuctionCreatedConsumer>(context);
                }
            );

            // need below all cfg
            cfg.ConfigureEndpoints(context);
        }
    );
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
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(res => res.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3)); // 若使用參數則會變成 exponential polling
