using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AuctionService.Extensions;

public static class IdentityServiceExtensions
{
	public static void AddIdentityServices(this IServiceCollection services, IConfiguration config)
	{

		services.AddAuthentication(cfg =>
		{
			cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(cfg =>
		{
			// Authority means when Application receive JWT, it will
			// call the Authority to validate this token.
			cfg.Authority = config["IdentityServiceUrl"];

			// wheter use HTTPS to communicate with authority, if true
			// this application will only accept Authority Meta with HTTPS
			cfg.RequireHttpsMetadata = false; // because our identity server run on http
			cfg.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				NameClaimType = "username",
			};
		});

	}
}
