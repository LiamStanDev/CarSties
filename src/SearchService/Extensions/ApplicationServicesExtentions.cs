using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Service;

namespace SearchService.Extensions;

public static class ApplicationServicesExtentions
{

	public static void AddApplicationSerivces(this IServiceCollection services, IConfiguration config)
	{

		services.AddControllers();
		services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
		services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

		services.AddMassTransit(c =>
		{
			c.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
			// KebabCase is list 'this-is-a-template'
			// 將默認的 exchange 與 queue 的名稱 AuctionCreated 添加前綴為
			// search-AuctionCreated 以免多個微服務命名衝突。
			c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
			c.UsingRabbitMq(
				(context, cfg) =>
				{
					cfg.UseMessageRetry(r =>
					{
						r.Handle<RabbitMqConnectionException>();
						r.Interval(5, TimeSpan.FromSeconds(10));
					});
					cfg.Host(config["RabbitMQ:Host"], "/", host =>
							{
								host.Username(config.GetValue("RabbitMQ:Username", "guest"));
								host.Password(config.GetValue("RabbitMQ:Password", "guest"));
							});
					// 單獨配置某一個 Receive Endpoint
					// 我們配置的是 search-auction-created 這個 queue
					cfg.ReceiveEndpoint(
						"search-auction-created",
						e =>
						{
							// 這邊配置的是當 AuctionCreatedConsumer 產生任何異常
							// 就會進行 Retry 也就是再次執行一次 Consume
							e.ConfigureConsumer<AuctionCreatedConsumer>(context);
							e.UseMessageRetry(r => r.Interval(5, 5));
						}
					);

					// need below all cfg
					cfg.ConfigureEndpoints(context);
				}
			);
		});
	}


	// using Polly to make polling policy
	private static IAsyncPolicy<HttpResponseMessage> GetPolicy() =>
		HttpPolicyExtensions
			.HandleTransientHttpError()
			.OrResult(res => res.StatusCode == System.Net.HttpStatusCode.NotFound)
			.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3)); // 若使用參數則會變成 exponential polling

}
