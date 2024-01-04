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
				AllowedGrantTypes = {GrantType.ResourceOwnerPassword}, // Password type
				ClientId = "postman",
				ClientName = "Postman",
				AllowedScopes = {"openid", "profile", "auctionApp"},
				RedirectUris = {"https://www.getpostman.com/oauth2/callback"}, // this won't be used by postman, use whatever you want
                ClientSecrets = new[] {new Secret("NotASecret".Sha256())},
			}
		};
}
