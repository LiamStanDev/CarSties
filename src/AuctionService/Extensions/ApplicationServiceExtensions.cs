using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Extensions;
public static class ApplicationServiceExtenstions
{
	public static void AddApplicationServices(this IServiceCollection services, IConfiguration config)
	{

		services.AddControllers();


		services.AddDbContext<AuctionDbContext>(opt =>
		{
			opt.UseNpgsql(config["ConnectionStrings:DefaultConnection"]);
		});

		services.AddScoped<IAuctionRepository, AuctionRepository>();


		// Get all assemblies in this App Domain only inside this process
		// AddAutoMapper will find class which inherited Profile
		services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


		services.AddMassTransit(c =>
		{
			c.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
			c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

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
								// GetValue can set default value
								host.Username(config.GetValue("RabbitMQ:Username", "guest"));
								host.Password(config.GetValue("RabbitMQ:Password", "guest"));
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

		services.AddGrpc();
	}
}
