using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playoffs.Web.Auth;

namespace Playoffs.Web.Areas.Organizer.Controllers;

[Area("Organizer")]
[Authorize(Policy = Policies.Organizer)]
public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}