using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BiddingService.Extensions;

public static class IdentityServiceExtentions
{
	public static void AddIdentityServices(this IServiceCollection services, IConfiguration config)
	{

		services.AddAuthentication(cfg =>
		{
			cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(cfg =>
		{
			cfg.Authority = config["IdentityServiceUrl"];
			cfg.RequireHttpsMetadata = false;
			cfg.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				NameClaimType = "username"
			};

		});

	}
}
