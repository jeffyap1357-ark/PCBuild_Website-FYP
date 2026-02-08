using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

public class HardwareController : Controller
{
    private readonly DB db;

    public HardwareController(DB db)
    {
        this.db = db;
    }

    // GET: Hardware/List
    public IActionResult List(int? categoryId)
    {
        var query = db.Hardware.Include(h => h.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(h => h.CategoryId == categoryId.Value);

        var hardware = query.OrderBy(h => h.Category.Type).ThenBy(h => h.Name).ToList();
        ViewBag.Categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
        ViewBag.SelectedCategoryId = categoryId;

        return View(hardware);
    }

    // GET: Hardware/Detail/5
    public IActionResult Detail(int id)
    {
        var h = db.Hardware.Include(x => x.Category).FirstOrDefault(x => x.HardwareId == id);
        if (h == null)
            return RedirectToAction(nameof(List));

        return View(h);
    }
}
