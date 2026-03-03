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
    public IActionResult List(int? categoryId, string brand)
    {
        var query = db.Hardware.Include(h => h.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(h => h.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(h => h.Brand != null && h.Brand.Trim().ToLower() == brand.Trim().ToLower());

        var hardware = query.OrderBy(h => h.Category.Type).ThenBy(h => h.Name).ToList();
        ViewBag.Categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.Brands = db.Hardware.Where(h => h.Brand != null && h.Brand.Trim() != "").Select(h => h.Brand!.Trim()).Distinct().OrderBy(b => b).ToList();
        ViewBag.SelectedBrand = brand;

        return View(hardware);
    }

    // GET: Hardware/ListPartial (AJAX - returns partial HTML)
    public IActionResult ListPartial(int? categoryId, string brand, string search)
    {
        var query = db.Hardware.Include(h => h.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(h => h.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(h => h.Brand != null && h.Brand.Trim().ToLower() == brand.Trim().ToLower());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(h =>
                h.Name.ToLower().Contains(q) ||
                (h.Category != null && h.Category.Type.ToLower().Contains(q)) ||
                (h.Brand != null && h.Brand.ToLower().Contains(q)));
        }

        var hardware = query.OrderBy(h => h.Category.Type).ThenBy(h => h.Name).ToList();
        return PartialView("_HardwareCardGrid", hardware);
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
