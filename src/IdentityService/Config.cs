using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
	public static IEnumerable<IdentityResource> IdentityResources =>
		new IdentityResource[]
		{
			new IdentityResources.OpenId(), // id token
			new IdentityResources.Profile(), // user profile
		};

	public static IEnumerable<ApiScope> ApiScopes =>
		new ApiScope[]
		{
			new ApiScope("auctionApp", "Auction app full access"),
		};

	public static IEnumerable<Client> Clients =>
		new Client[]
		{
			new Client
			{
				ClientId = "postman",
				ClientName = "Postman",
				AllowedScopes = {"openid", "profile", "auctionApp"},
				RedirectUris = {"https://www.getpostman.com/oauth2/callback"}, // this won't be used by postman, use whatever you want
                ClientSecrets = new[] {new Secret("NotASecret".Sha256())},
				AllowedGrantTypes = {GrantType.ResourceOwnerPassword}, // Password type
			},

			new Client
			{
				ClientId = "nextApp",
				ClientName = "nextApp",
				ClientSecrets = {new Secret("secret".Sha256())},
				AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
				RequirePkce = false,
				RedirectUris = {"http://localhost:3000/aip/auth/callback/id-server"},
				AllowOfflineAccess = true,
				AllowedScopes = {"openid", "profile", "auctionApp"},
				AccessTokenLifetime = 3600 * 24 * 30 // one month
			}
		};
}
