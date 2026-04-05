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
        ViewBag.Categories = db.Categories.OrderBy(c => c.Type).ToList();
        ViewBag.TopRatedHardware = GetTopRatedHardwareQuery(null).ToList();
        return View();
    }

    /// <summary>Partial HTML for home "Higher rated hardware" grid (AJAX refresh).</summary>
    [HttpGet]
    public IActionResult TopRatedGrid(int? categoryId)
    {
        var list = GetTopRatedHardwareQuery(categoryId).ToList();
        return PartialView("_HomeTopRatedGrid", list);
    }

    private IQueryable<Hardware> GetTopRatedHardwareQuery(int? categoryId)
    {
        var query = db.Hardware.Include(h => h.Category).AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(h => h.CategoryId == categoryId.Value);
        return query
            .OrderByDescending(h => h.Rating ?? 0)
            .ThenByDescending(h => h.PerformanceScore ?? 0)
            .Take(10);
    }
}
