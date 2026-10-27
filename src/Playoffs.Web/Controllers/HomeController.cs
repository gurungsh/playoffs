using Microsoft.AspNetCore.Mvc;

namespace Playoffs.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();

    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Error(int? code) => View(code);
}