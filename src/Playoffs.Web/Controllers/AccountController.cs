using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Playoffs.Web.Controllers;

public sealed class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(Uri? returnUrl = null) =>
        Challenge(
            new AuthenticationProperties
            {
                // Local URLs only, so the login cannot be used as an open redirect.
                RedirectUri = Url.IsLocalUrl(returnUrl?.OriginalString)
                    ? returnUrl.OriginalString
                    : "/",
            },
            OpenIdConnectDefaults.AuthenticationScheme
        );

    [Authorize]
    [HttpPost]
    public IActionResult Logout() =>
        SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        );

    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}