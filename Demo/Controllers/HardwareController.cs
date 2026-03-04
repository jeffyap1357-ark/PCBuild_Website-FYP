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

    // GET: Hardware/Compare?id1=1 or Compare?id1=1&id2=2 - Compare two hardware items of the same category
    public IActionResult Compare(int id1, int id2 = 0)
    {
        var h1 = db.Hardware.Include(x => x.Category).FirstOrDefault(x => x.HardwareId == id1);
        if (h1 == null)
        {
            TempData["Info"] = "Hardware item not found.";
            return RedirectToAction(nameof(List));
        }

        var sameCategory = db.Hardware.Include(h => h.Category)
            .Where(h => h.CategoryId == h1.CategoryId)
            .OrderBy(h => h.Name)
            .ToList();

        if (id2 <= 0)
        {
            ViewBag.Hardware1 = h1;
            ViewBag.Hardware2 = (Hardware?)null;
            ViewBag.SameCategoryHardware = sameCategory;
            ViewBag.CategoryName = h1.Category?.Type ?? "Hardware";
            return View();
        }

        if (id1 == id2)
        {
            TempData["Info"] = "Please select two different items to compare.";
            return RedirectToAction(nameof(Compare), new { id1 });
        }

        var h2 = db.Hardware.Include(x => x.Category).FirstOrDefault(x => x.HardwareId == id2);
        if (h2 == null)
        {
            TempData["Info"] = "Second hardware item not found.";
            return RedirectToAction(nameof(Compare), new { id1 });
        }

        if (h1.CategoryId != h2.CategoryId)
        {
            TempData["Info"] = "Can only compare hardware of the same category (e.g. RAM vs RAM).";
            return RedirectToAction(nameof(Compare), new { id1 });
        }

        ViewBag.Hardware1 = h1;
        ViewBag.Hardware2 = h2;
        ViewBag.SameCategoryHardware = sameCategory;
        ViewBag.CategoryName = h1.Category?.Type ?? "Hardware";
        return View();
    }
}
