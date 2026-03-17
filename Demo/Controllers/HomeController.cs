using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

public class HomeController : Controller
{
    private readonly DB db;

    public HomeController(DB db)
    {
        this.db = db;
    }

    // GET: Home/Index
    public IActionResult Index()
    {
        ViewBag.HideMainTitle = true;
        ViewBag.IsHomePage = true;
        // Higher rated hardware: 2 rows x 5 = 10 items for section two
        var topRated = db.Hardware
            .Include(h => h.Category)
            .OrderByDescending(h => h.Rating ?? 0)
            .ThenByDescending(h => h.PerformanceScore ?? 0)
            .Take(10)
            .ToList();
        ViewBag.TopRatedHardware = topRated;
        return View();
    }
}
