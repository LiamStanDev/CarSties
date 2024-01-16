using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddAuthentication(cfg =>
{
	cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(cfg =>
{
	cfg.Authority = builder.Configuration["IdentityServiceUrl"];

	cfg.RequireHttpsMetadata = false; // because our identity server run on http
	cfg.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateAudience = false,
		NameClaimType = "username",
	};
});

builder.Services.AddCors(c =>
{
	c.AddPolicy("policyForSignalIR", p =>
	{
		p.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials()
			.WithOrigins(builder.Configuration["ClientApp"]);
	});
});

var app = builder.Build();

app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
