using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
