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

    public IActionResult Configure(int? draftId = null)
    {
        var vm = new ConfigureBuildVM();
        var categories = db.Categories.Include(c => c.Hardware).OrderBy(c => c.CategoryId).ToList();
        var order = new[] { "Motherboard", "CPU", "RAM", "GPU", "Storage", "Power supply unit", "CPU cooler", "PC case" };

        if (draftId.HasValue)
        {
            var userId = GetCurrentUserId();
            var draft = db.DraftBuilds
                .Include(d => d.Details)
                .ThenInclude(dd => dd.Hardware)
                .ThenInclude(h => h.Category)
                .FirstOrDefault(d => d.DraftBuildId == draftId.Value && d.UserId == userId);
            if (draft != null)
            {
                vm.BuildName = draft.BuildName;
                var ramDetails = draft.Details.Where(dd => dd.Hardware.Category?.Type == "RAM").OrderBy(dd => dd.DraftBuildDetailId).ToList();
                var ramSelections = ramDetails.Select(dd => (int?)dd.HardwareId).ToList();
                while (ramSelections.Count < 4) ramSelections.Add(null);

                vm.Categories = categories
                    .OrderBy(c => Array.IndexOf(order, c.Type) >= 0 ? Array.IndexOf(order, c.Type) : 99)
                    .ThenBy(c => c.CategoryId)
                    .Select(c => new CategoryOptionVM
                    {
                        CategoryId = c.CategoryId,
                        Type = c.Type,
                        Items = c.Type == "GPU" ? Helper.FilterDuplicateGpuFromDisplay(c.Hardware.OrderBy(h => h.Name).ToList(), "GPU") : c.Hardware.OrderBy(h => h.Name).ToList(),
                        SelectedHardwareId = c.Type == "RAM" ? null : draft.Details.Where(dd => dd.Hardware.CategoryId == c.CategoryId).Select(dd => (int?)dd.HardwareId).FirstOrDefault(),
                        RamSlotSelections = c.Type == "RAM" ? ramSelections : []
                    }).ToList();
                ViewBag.DraftId = draftId.Value;
                return View(vm);
            }
        }

        vm.Categories = categories
            .OrderBy(c => Array.IndexOf(order, c.Type) >= 0 ? Array.IndexOf(order, c.Type) : 99)
            .ThenBy(c => c.CategoryId)
            .Select(c => new CategoryOptionVM
            {
                CategoryId = c.CategoryId,
                Type = c.Type,
                Items = c.Type == "GPU" ? Helper.FilterDuplicateGpuFromDisplay(c.Hardware.OrderBy(h => h.Name).ToList(), "GPU") : c.Hardware.OrderBy(h => h.Name).ToList(),
                SelectedHardwareId = null,
                RamSlotSelections = c.Type == "RAM" ? new List<int?>(new int?[4]) : []
            }).ToList();
        return View(vm);
    }

    [HttpPost]
    public IActionResult Configure(ConfigureBuildVM vm, int? draftId = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var order = new[] { "Motherboard", "CPU", "RAM", "GPU", "Storage", "Power supply unit", "CPU cooler", "PC case" };
        var categories = db.Categories.Include(c => c.Hardware).OrderBy(c => c.CategoryId).ToList();
        var ramCat = vm.Categories?.FirstOrDefault(x => x.Type == "RAM");
        var mbCat = vm.Categories?.FirstOrDefault(x => x.Type == "Motherboard");
        var mbId = mbCat?.SelectedHardwareId;
        var ramSlots = mbId.HasValue ? (db.Hardware.Find(mbId.Value)?.RamSlots ?? 4) : 4;

        vm.Categories = categories
            .OrderBy(c => Array.IndexOf(order, c.Type) >= 0 ? Array.IndexOf(order, c.Type) : 99)
            .ThenBy(c => c.CategoryId)
            .Select(c => new CategoryOptionVM
            {
                CategoryId = c.CategoryId,
                Type = c.Type,
                Items = c.Hardware.OrderBy(h => h.Name).ToList(),
                SelectedHardwareId = c.Type == "RAM" ? null : vm.Categories?.FirstOrDefault(x => x.CategoryId == c.CategoryId)?.SelectedHardwareId,
                RamSlotSelections = c.Type == "RAM" ? (ramCat?.RamSlotSelections ?? new List<int?>(new int?[4])) : []
            }).ToList();

        if (string.IsNullOrWhiteSpace(vm.BuildName))
            ModelState.AddModelError("BuildName", "Build name is required.");

        if (ModelState.IsValid)
        {
            var selectedIds = new List<int>();
            foreach (var c in vm.Categories)
            {
                if (c.Type == "RAM")
                {
                    var slotsToUse = Math.Min(ramSlots, c.RamSlotSelections?.Count ?? 0);
                    for (var i = 0; i < slotsToUse && i < (c.RamSlotSelections?.Count ?? 0); i++)
                    {
                        var hid = c.RamSlotSelections?[i];
                        if (hid.HasValue && hid.Value > 0)
                            selectedIds.Add(hid.Value);
                    }
                }
                else if (c.SelectedHardwareId.HasValue && c.SelectedHardwareId.Value > 0)
                {
                    selectedIds.Add(c.SelectedHardwareId.Value);
                }
            }

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

            if (draftId.HasValue)
            {
                var draft = db.DraftBuilds.FirstOrDefault(d => d.DraftBuildId == draftId.Value && d.UserId == userId);
                if (draft != null)
                {
                    db.DraftBuilds.Remove(draft);
                    db.SaveChanges();
                }
            }

            TempData["Info"] = "Build saved.";
            return RedirectToAction(nameof(Detail), new { id = build.BuildId });
        }

        ViewBag.DraftId = draftId;
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
            .ThenInclude(h => h.Category)
            .FirstOrDefault(b => b.BuildId == id && b.UserId == userId);

        if (build == null) return RedirectToAction(nameof(Index));

        var order = new[] { "Motherboard", "CPU", "RAM", "GPU", "Storage", "Power supply unit", "CPU cooler", "PC case" };
        var categories = db.Categories.Include(c => c.Hardware).OrderBy(c => c.CategoryId).ToList();
        var ramDetails = build.BuildDetails.Where(bd => bd.Hardware.Category?.Type == "RAM").OrderBy(bd => bd.BuildDetailId).ToList();
        var ramSelections = ramDetails.Select(bd => (int?)bd.HardwareId).ToList();
        while (ramSelections.Count < 4) ramSelections.Add(null);

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
                    Items = c.Type == "GPU" ? Helper.FilterDuplicateGpuFromDisplay(c.Hardware.OrderBy(h => h.Name).ToList(), "GPU") : c.Hardware.OrderBy(h => h.Name).ToList(),
                    SelectedHardwareId = c.Type == "RAM" ? null : build.BuildDetails.Where(bd => bd.Hardware.CategoryId == c.CategoryId).Select(bd => (int?)bd.HardwareId).FirstOrDefault(),
                    RamSlotSelections = c.Type == "RAM" ? ramSelections : []
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

        var ramCat = vm.Categories?.FirstOrDefault(x => x.Type == "RAM");
        var mbCat = vm.Categories?.FirstOrDefault(x => x.Type == "Motherboard");
        var mbId = mbCat?.SelectedHardwareId;
        var ramSlots = mbId.HasValue ? (db.Hardware.Find(mbId.Value)?.RamSlots ?? 4) : 4;

        var order = new[] { "Motherboard", "CPU", "RAM", "GPU", "Storage", "Power supply unit", "CPU cooler", "PC case" };
        var categories = db.Categories.Include(c => c.Hardware).OrderBy(c => c.CategoryId).ToList();
        vm.Categories = categories
            .OrderBy(c => Array.IndexOf(order, c.Type) >= 0 ? Array.IndexOf(order, c.Type) : 99)
            .ThenBy(c => c.CategoryId)
            .Select(c => new CategoryOptionVM
            {
                CategoryId = c.CategoryId,
                Type = c.Type,
                Items = c.Type == "GPU" ? Helper.FilterDuplicateGpuFromDisplay(c.Hardware.OrderBy(h => h.Name).ToList(), "GPU") : c.Hardware.OrderBy(h => h.Name).ToList(),
                SelectedHardwareId = c.Type == "RAM" ? null : vm.Categories?.FirstOrDefault(x => x.CategoryId == c.CategoryId)?.SelectedHardwareId,
                RamSlotSelections = c.Type == "RAM" ? (ramCat?.RamSlotSelections ?? new List<int?>(new int?[4])) : []
            }).ToList();

        if (string.IsNullOrWhiteSpace(vm.BuildName))
            ModelState.AddModelError("BuildName", "Build name is required.");

        if (ModelState.IsValid)
        {
            var selectedIds = new List<int>();
            foreach (var c in vm.Categories)
            {
                if (c.Type == "RAM")
                {
                    var slotsToUse = Math.Min(ramSlots, c.RamSlotSelections?.Count ?? 0);
                    for (var i = 0; i < slotsToUse && i < (c.RamSlotSelections?.Count ?? 0); i++)
                    {
                        var hid = c.RamSlotSelections?[i];
                        if (hid.HasValue && hid.Value > 0)
                            selectedIds.Add(hid.Value);
                    }
                }
                else if (c.SelectedHardwareId.HasValue && c.SelectedHardwareId.Value > 0)
                {
                    selectedIds.Add(c.SelectedHardwareId.Value);
                }
            }

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
