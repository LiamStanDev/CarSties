using BiddingService.Consumers;
using BiddingService.Services;
using MassTransit;

namespace BiddingService.Extensions;

public static class ApplicationSerivceExtensions
{
	public static void AddApplicationServices(this IServiceCollection services, IConfiguration config)
	{

		services.AddControllers();

		services.AddMassTransit(c =>
		{
			c.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
			c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));
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
					cfg.ConfigureEndpoints(context);
				}
			);
		});

		services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
		services.AddHostedService<CheckAuctionFinished>();

		services.AddScoped<GrpcAuctionClient>();
	}
}
