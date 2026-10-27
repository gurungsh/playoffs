using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playoffs.Web.Auth;

namespace Playoffs.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = Policies.Admin)]
public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}