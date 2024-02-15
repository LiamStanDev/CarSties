using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;

public class CustomProfileService : IProfileService
{
	private readonly UserManager<ApplicationUser> _userMgr;

	public CustomProfileService(UserManager<ApplicationUser> userMgr)
	{
		_userMgr = userMgr;
	}

	public async Task GetProfileDataAsync(ProfileDataRequestContext context)
	{
		var user = await _userMgr.GetUserAsync(context.Subject);
		var existingClaims = await _userMgr.GetClaimsAsync(user);

		var claim = new Claim("username", user.UserName);

		context.IssuedClaims.Add(claim);

		// For HttpContext.User.Identity.Name
		context.IssuedClaims.Add(existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name));

	}

	public Task IsActiveAsync(IsActiveContext context)
	{
		return Task.CompletedTask;
	}
}

