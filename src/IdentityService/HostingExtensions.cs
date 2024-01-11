using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

internal static class HostingExtensions
{
	public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddRazorPages();

		builder.Services.AddDbContext<ApplicationDbContext>(
			options =>
				options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
		);

		builder
			.Services.AddIdentity<ApplicationUser, IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

		builder
			.Services.AddIdentityServer(options =>
			{
				options.Events.RaiseErrorEvents = true;
				options.Events.RaiseInformationEvents = true;
				options.Events.RaiseFailureEvents = true;
				options.Events.RaiseSuccessEvents = true;

				// 這邊若使用 Development 的話，IssuerUri 默認爲 localhost:5000
				// 這個值是因爲在 Properties/launchSettings.json 設定的
				// 但在 docker 環境中這樣不行。需要手動設置
				if (builder.Environment.IsEnvironment("Docker"))
				{
					options.IssuerUri = "identity-svc";
				}

				// see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
				// options.EmitStaticAudienceClaim = true;
			})
			.AddInMemoryIdentityResources(Config.IdentityResources)
			.AddInMemoryApiScopes(Config.ApiScopes)
			.AddInMemoryClients(Config.Clients)
			.AddAspNetIdentity<ApplicationUser>()
			.AddProfileService<CustomProfileService>();

		builder.Services.ConfigureApplicationCookie(opt =>
		{
			// the client should send "same-site" request
			opt.Cookie.SameSite = SameSiteMode.Lax;
		});

		builder.Services.AddAuthentication();

		return builder.Build();
	}

	public static WebApplication ConfigurePipeline(this WebApplication app)
	{
		app.UseSerilogRequestLogging();

		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseStaticFiles();
		app.UseRouting();
		app.UseIdentityServer();
		app.UseAuthorization();

		app.MapRazorPages().RequireAuthorization();

		return app;
	}
}

