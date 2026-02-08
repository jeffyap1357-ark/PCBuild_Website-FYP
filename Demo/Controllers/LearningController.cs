using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

public class LearningController : Controller
{
    private readonly DB db;

    public LearningController(DB db)
    {
        this.db = db;
    }

    private int? GetCurrentUserId()
    {
        var name = User.Identity?.Name;
        if (string.IsNullOrEmpty(name)) return null;
        return db.Users.Where(u => u.Username == name).Select(u => u.UserId).FirstOrDefault();
    }

    public IActionResult Index()
    {
        var userId = GetCurrentUserId();
        var materials = db.LearningMaterials.OrderBy(m => m.SortOrder).ToList();

        var interestIds = userId.HasValue
            ? db.UserInterests.Where(ui => ui.UserId == userId).Select(ui => ui.MaterialId).ToHashSet()
            : new HashSet<int>();

        var progressIds = userId.HasValue
            ? db.UserProgresses.Where(up => up.UserId == userId).Select(up => up.MaterialId).ToHashSet()
            : new HashSet<int>();

        var interested = materials.Where(m => interestIds.Contains(m.MaterialId)).ToList();
        var all = materials;

        ViewBag.InterestIds = interestIds;
        ViewBag.ProgressIds = progressIds;
        ViewBag.InterestedMaterials = interested;
        ViewBag.UserId = userId;

        return View(all);
    }

    public IActionResult Detail(int id)
    {
        var userId = GetCurrentUserId();
        var material = db.LearningMaterials.FirstOrDefault(m => m.MaterialId == id);
        if (material == null) return RedirectToAction(nameof(Index));

        // Record progress when user opens (if logged in)
        if (userId.HasValue)
        {
            var existing = db.UserProgresses.FirstOrDefault(up => up.UserId == userId && up.MaterialId == id);
            if (existing == null)
            {
                db.UserProgresses.Add(new UserProgress { UserId = userId.Value, MaterialId = id });
                db.SaveChanges();
            }
        }

        var isInterested = userId.HasValue && db.UserInterests.Any(ui => ui.UserId == userId && ui.MaterialId == id);
        ViewBag.IsInterested = isInterested;
        ViewBag.UserId = userId;

        return View(material);
    }

    [Authorize]
    [HttpPost]
    public IActionResult MarkInterest(int id, string? returnTo = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var material = db.LearningMaterials.FirstOrDefault(m => m.MaterialId == id);
        if (material == null) return RedirectToAction(nameof(Index));

        if (!db.UserInterests.Any(ui => ui.UserId == userId && ui.MaterialId == id))
        {
            db.UserInterests.Add(new UserInterest { UserId = userId.Value, MaterialId = id });
            db.SaveChanges();
            TempData["Info"] = "Added to your interests.";
        }

        return returnTo == "index" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize]
    [HttpPost]
    public IActionResult UnmarkInterest(int id, string? returnTo = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var ui = db.UserInterests.FirstOrDefault(x => x.UserId == userId && x.MaterialId == id);
        if (ui != null)
        {
            db.UserInterests.Remove(ui);
            db.SaveChanges();
            TempData["Info"] = "Removed from interests.";
        }

        return returnTo == "index" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}
