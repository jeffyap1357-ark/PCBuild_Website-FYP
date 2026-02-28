using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

[Authorize]
public class BuildController : Controller
{
    private readonly DB db;

    public BuildController(DB db)
    {
        this.db = db;
    }

    private int? GetCurrentUserId()
    {
        var name = User.Identity?.Name;
        if (string.IsNullOrEmpty(name)) return null;
        return db.Users.Where(u => u.Username == name).Select(u => u.UserId).FirstOrDefault();
    }

    public IActionResult Configure()
    {
        var vm = new ConfigureBuildVM();
        var categories = db.Categories.Include(c => c.Hardware).OrderBy(c => c.CategoryId).ToList();
        var order = new[] { "Motherboard", "CPU", "RAM", "GPU", "Storage", "Power supply unit", "CPU cooler", "PC case" };
        vm.Categories = categories
            .OrderBy(c => Array.IndexOf(order, c.Type) >= 0 ? Array.IndexOf(order, c.Type) : 99)
            .ThenBy(c => c.CategoryId)
            .Select(c => new CategoryOptionVM
            {
                CategoryId = c.CategoryId,
                Type = c.Type,
                Items = c.Hardware.OrderBy(h => h.Name).ToList(),
                SelectedHardwareId = null
            }).ToList();
        return View(vm);
    }

    [HttpPost]
    public IActionResult Configure(ConfigureBuildVM vm)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var order = new[] { "Motherboard", "CPU", "RAM", "GPU", "Storage", "Power supply unit", "CPU cooler", "PC case" };
        var categories = db.Categories.Include(c => c.Hardware).OrderBy(c => c.CategoryId).ToList();
        vm.Categories = categories
            .OrderBy(c => Array.IndexOf(order, c.Type) >= 0 ? Array.IndexOf(order, c.Type) : 99)
            .ThenBy(c => c.CategoryId)
            .Select(c => new CategoryOptionVM
            {
                CategoryId = c.CategoryId,
                Type = c.Type,
                Items = c.Hardware.OrderBy(h => h.Name).ToList(),
                SelectedHardwareId = vm.Categories?.FirstOrDefault(x => x.CategoryId == c.CategoryId)?.SelectedHardwareId
            }).ToList();

        if (string.IsNullOrWhiteSpace(vm.BuildName))
            ModelState.AddModelError("BuildName", "Build name is required.");

        if (ModelState.IsValid)
        {
            var selectedIds = vm.Categories
                .Where(c => c.SelectedHardwareId.HasValue && c.SelectedHardwareId.Value > 0)
                .Select(c => c.SelectedHardwareId!.Value)
                .ToList();

            var total = db.Hardware.Where(h => selectedIds.Contains(h.HardwareId)).Sum(h => h.Price);

            var build = new Build
            {
                UserId = userId.Value,
                BuildName = vm.BuildName.Trim(),
                TotalPrice = total
            };
            db.Builds.Add(build);
            db.SaveChanges();

            foreach (var hid in selectedIds)
                db.BuildDetails.Add(new BuildDetail { BuildId = build.BuildId, HardwareId = hid, Quantity = 1 });
            db.SaveChanges();

            TempData["Info"] = "Build saved.";
            return RedirectToAction(nameof(Detail), new { id = build.BuildId });
        }

        return View(vm);
    }

    public IActionResult Index()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var list = db.Builds
            .Include(b => b.Community)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BuildId)
            .ToList();

        return View(list);
    }

    public IActionResult Detail(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var build = db.Builds
            .Include(b => b.BuildDetails)
            .ThenInclude(bd => bd.Hardware)
            .ThenInclude(h => h.Category)
            .Include(b => b.Community)
            .FirstOrDefault(b => b.BuildId == id && b.UserId == userId);

        if (build == null) return RedirectToAction(nameof(Index));

        return View(build);
    }

    public IActionResult Edit(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var build = db.Builds
            .Include(b => b.BuildDetails)
            .ThenInclude(bd => bd.Hardware)
            .FirstOrDefault(b => b.BuildId == id && b.UserId == userId);

        if (build == null) return RedirectToAction(nameof(Index));

        var order = new[] { "Motherboard", "CPU", "RAM", "GPU", "Storage", "Power supply unit", "CPU cooler", "PC case" };
        var categories = db.Categories.Include(c => c.Hardware).OrderBy(c => c.CategoryId).ToList();
        var vm = new ConfigureBuildVM
        {
            BuildName = build.BuildName,
            Categories = categories
                .OrderBy(c => Array.IndexOf(order, c.Type) >= 0 ? Array.IndexOf(order, c.Type) : 99)
                .ThenBy(c => c.CategoryId)
                .Select(c => new CategoryOptionVM
                {
                    CategoryId = c.CategoryId,
                    Type = c.Type,
                    Items = c.Hardware.OrderBy(h => h.Name).ToList(),
                    SelectedHardwareId = build.BuildDetails.Where(bd => bd.Hardware.CategoryId == c.CategoryId).Select(bd => (int?)bd.HardwareId).FirstOrDefault()
                }).ToList()
        };
        ViewBag.BuildId = id;
        return View(vm);
    }

    [HttpPost]
    public IActionResult Edit(int id, ConfigureBuildVM vm)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var build = db.Builds.Include(b => b.BuildDetails).FirstOrDefault(b => b.BuildId == id && b.UserId == userId);
        if (build == null) return RedirectToAction(nameof(Index));

        var order = new[] { "Motherboard", "CPU", "RAM", "GPU", "Storage", "Power supply unit", "CPU cooler", "PC case" };
        var categories = db.Categories.Include(c => c.Hardware).OrderBy(c => c.CategoryId).ToList();
        vm.Categories = categories
            .OrderBy(c => Array.IndexOf(order, c.Type) >= 0 ? Array.IndexOf(order, c.Type) : 99)
            .ThenBy(c => c.CategoryId)
            .Select(c => new CategoryOptionVM
            {
                CategoryId = c.CategoryId,
                Type = c.Type,
                Items = c.Hardware.OrderBy(h => h.Name).ToList(),
                SelectedHardwareId = vm.Categories?.FirstOrDefault(x => x.CategoryId == c.CategoryId)?.SelectedHardwareId
            }).ToList();

        if (string.IsNullOrWhiteSpace(vm.BuildName))
            ModelState.AddModelError("BuildName", "Build name is required.");

        if (ModelState.IsValid)
        {
            var selectedIds = vm.Categories
                .Where(c => c.SelectedHardwareId.HasValue && c.SelectedHardwareId.Value > 0)
                .Select(c => c.SelectedHardwareId!.Value)
                .ToList();

            var total = db.Hardware.Where(h => selectedIds.Contains(h.HardwareId)).Sum(h => h.Price);

            build.BuildName = vm.BuildName.Trim();
            build.TotalPrice = total;
            db.BuildDetails.RemoveRange(build.BuildDetails);
            foreach (var hid in selectedIds)
                db.BuildDetails.Add(new BuildDetail { BuildId = build.BuildId, HardwareId = hid, Quantity = 1 });
            db.SaveChanges();

            TempData["Info"] = "Build updated.";
            return RedirectToAction(nameof(Detail), new { id = build.BuildId });
        }

        ViewBag.BuildId = id;
        return View(vm);
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var build = db.Builds.FirstOrDefault(b => b.BuildId == id && b.UserId == userId);
        if (build != null)
        {
            // Remove from Community first (comments, ratings, then community) so it disappears from Community page
            var community = db.Communities.FirstOrDefault(c => c.BuildId == id);
            if (community != null)
            {
                db.CommunityComments.RemoveRange(db.CommunityComments.Where(cc => cc.CommunityId == community.CommunityId));
                db.CommunityRatings.RemoveRange(db.CommunityRatings.Where(r => r.CommunityId == community.CommunityId));
                db.Communities.Remove(community);
            }
            db.Builds.Remove(build);
            db.SaveChanges();
            TempData["Info"] = "Build deleted" + (community != null ? " and removed from Community." : ".");
        }

        return RedirectToAction(nameof(Index));
    }
}
