using BiddingService.Consumers;
using BiddingService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthentication(cfg =>
{
	cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(cfg =>
{
	cfg.Authority = builder.Configuration["IdentityServiceUrl"];
	cfg.RequireHttpsMetadata = false;
	cfg.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateAudience = false,
		NameClaimType = "username"
	};

});

builder.Services.AddMassTransit(c =>
{
	c.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
	c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));
	c.UsingRabbitMq(
		(context, cfg) =>
		{
			cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", host =>
				{
					host.Username(builder.Configuration.GetValue("RabbitMQ:Username", "guest"));
					host.Password(builder.Configuration.GetValue("RabbitMQ:Password", "guest"));
				});
			cfg.ConfigureEndpoints(context);
		}
	);
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHostedService<CheckAuctionFinished>();
builder.Services.AddScoped<GrpcAuctionClient>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

await DB.InitAsync("BidDB", MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("BidDbConnection")));

app.Run();
