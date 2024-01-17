using System.Security.Claims;

namespace AuctionService.UnitTests.Utils;

public class HttpContextHelper
{
	public static ClaimsPrincipal GetClaimsPrincipal()
	{
		// 因爲我們是獨立測試 Controller 所以沒有辦法透過框架進行權限驗證，
		// 所以我們需要手動給定 Claim，但我們在權限驗證 AddIdentity 指定
		// NameClaim 爲 "username", 但因爲沒有透過框架配置，所以默認NameClaim
		// 爲 ClaimTypes.Name
		var claims = new List<Claim> { new Claim(ClaimTypes.Name, "test") };

		var claimIdentiy = new ClaimsIdentity(claims, "testing");

		return new ClaimsPrincipal(claimIdentiy);
	}
}

