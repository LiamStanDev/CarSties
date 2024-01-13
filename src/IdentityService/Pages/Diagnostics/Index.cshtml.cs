using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityService.Pages.Diagnostics;

[SecurityHeaders]
[Authorize]
public class Index : PageModel
{
	public ViewModel View { get; set; }

	public async Task<IActionResult> OnGet()
	{

		// we use docker, so this ip address is not localhost
		// it's generate by docker
		var localAddresses = new string[] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };
		if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
		{
			return NotFound();
		}

		View = new ViewModel(await HttpContext.AuthenticateAsync());

		return Page();
	}
}
