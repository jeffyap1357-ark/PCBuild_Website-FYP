using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Models;
using Demo.Services;

namespace Demo.Controllers;

public class RecommendController : Controller
{
    private readonly PcRecommendService _recommend;
    private readonly DB _db;

    public RecommendController(PcRecommendService recommend, DB db)
    {
        _recommend = recommend;
        _db = db;
    }

    public IActionResult Index()
    {
        return View(new RecommendBuildVM { Budget = 5000, TargetTier = 7 });
    }

    [HttpPost]
    public async Task<IActionResult> Index(RecommendBuildVM vm)
    {
        if (vm.Budget < 1000)
        {
            ModelState.AddModelError("Budget", "Minimum budget is RM 1,000.");
            return View(vm);
        }

        var result = await _recommend.RecommendAsync(vm.Budget, vm.TargetTier, vm.OptimizationType);
        ViewBag.Result = result;
        ViewBag.Budget = vm.Budget;
        return View(vm);
    }

    /// <summary>Save recommended build to user's build store and redirect to My Builds.</summary>
    [HttpPost, Authorize]
    public IActionResult SaveBuild([FromForm] List<int> hardwareIds, [FromForm] string buildName)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        if (hardwareIds == null || hardwareIds.Count == 0)
        {
            TempData["Error"] = "No hardware selected.";
            return RedirectToAction(nameof(Index));
        }

        var total = _db.Hardware.Where(h => hardwareIds.Contains(h.HardwareId)).Sum(h => h.Price);
        var build = new Build
        {
            UserId = userId.Value,
            BuildName = string.IsNullOrWhiteSpace(buildName) ? "Recommended Build" : buildName.Trim(),
            TotalPrice = total
        };
        _db.Builds.Add(build);
        _db.SaveChanges();

        foreach (var hid in hardwareIds.Distinct())
            _db.BuildDetails.Add(new BuildDetail { BuildId = build.BuildId, HardwareId = hid, Quantity = 1 });
        _db.SaveChanges();

        TempData["Info"] = "Build saved to My Builds.";
        return RedirectToAction(nameof(Index), "Build");
    }

    /// <summary>Create draft from recommendation and redirect to Configure page for editing.</summary>
    [HttpPost, Authorize]
    public IActionResult ConfigureBuild([FromForm] List<int> hardwareIds, [FromForm] string buildName)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        if (hardwareIds == null || hardwareIds.Count == 0)
        {
            TempData["Error"] = "No hardware selected.";
            return RedirectToAction(nameof(Index));
        }

        var total = _db.Hardware.Where(h => hardwareIds.Contains(h.HardwareId)).Sum(h => h.Price);
        var draft = new DraftBuild
        {
            UserId = userId.Value,
            BuildName = string.IsNullOrWhiteSpace(buildName) ? "Recommended Build" : buildName.Trim(),
            TotalPrice = total
        };
        _db.DraftBuilds.Add(draft);
        _db.SaveChanges();

        foreach (var hid in hardwareIds.Distinct())
            _db.DraftBuildDetails.Add(new DraftBuildDetail { DraftBuildId = draft.DraftBuildId, HardwareId = hid, Quantity = 1 });
        _db.SaveChanges();

        return RedirectToAction("Configure", "Build", new { draftId = draft.DraftBuildId });
    }

    private int? GetCurrentUserId()
    {
        var name = User.Identity?.Name;
        if (string.IsNullOrEmpty(name)) return null;
        return _db.Users.Where(u => u.Username == name).Select(u => u.UserId).FirstOrDefault();
    }
}
