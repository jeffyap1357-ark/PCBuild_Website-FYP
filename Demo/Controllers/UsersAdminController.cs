using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

[Authorize(Roles = Roles.Admin)]
public class UsersAdminController : Controller
{
    private readonly DB _db;

    public UsersAdminController(DB db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();
        ViewBag.CurrentUsername = User.Identity?.Name;
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetBlocked(int id, bool blocked)
    {
        var target = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (target == null)
        {
            TempData["Info"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        if (target.IsRootAdmin)
        {
            TempData["Info"] = "The root admin account cannot be blocked.";
            return RedirectToAction(nameof(Index));
        }

        var currentName = User.Identity?.Name;
        if (string.Equals(target.Username, currentName, StringComparison.Ordinal))
        {
            TempData["Info"] = "You cannot block your own account.";
            return RedirectToAction(nameof(Index));
        }

        target.IsBlocked = blocked;
        await _db.SaveChangesAsync();
        TempData["Info"] = blocked
            ? $"User \"{target.Username}\" has been blocked."
            : $"User \"{target.Username}\" has been unblocked.";
        return RedirectToAction(nameof(Index));
    }
}
