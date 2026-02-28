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

    /// <summary>Diagnostic: lists tables in the database to verify Guide exists.</summary>
    public async Task<IActionResult> Diagnose(CancellationToken ct = default)
    {
        List<string> tables;
        try
        {
            tables = await db.Database
                .SqlQueryRaw<string>("SELECT TABLE_SCHEMA + '.' + TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME")
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            return Content($"<h2>Database error</h2><pre>{ex.Message}</pre>", "text/html");
        }

        var hasGuide = tables.Any(t => t.EndsWith(".Guide", StringComparison.OrdinalIgnoreCase));
        var html = $@"
<h2>Database diagnostic</h2>
<p><b>Guide table exists:</b> {(hasGuide ? "Yes" : "No")}</p>
<h3>Tables ({tables.Count})</h3>
<ul>{string.Join("", tables.Select(t => $"<li>{t}</li>"))}</ul>
<p><a href='/Learning'>Back to Learning</a></p>";
        return Content(html, "text/html");
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
        var guides = db.Guides.OrderBy(g => g.SortOrder).ToList();

        var readIds = userId.HasValue
            ? db.UserGuideProgresses.Where(up => up.UserId == userId).Select(up => up.GuideId).ToHashSet()
            : new HashSet<int>();

        ViewBag.ReadIds = readIds;
        ViewBag.UserId = userId;

        return View(guides);
    }

    public IActionResult Detail(int id)
    {
        var guide = db.Guides.FirstOrDefault(g => g.GuideId == id);
        if (guide == null) return RedirectToAction(nameof(Index));

        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            if (!db.UserGuideProgresses.Any(up => up.UserId == userId && up.GuideId == id))
            {
                db.UserGuideProgresses.Add(new UserGuideProgress { UserId = userId.Value, GuideId = id });
                db.SaveChanges();
            }
        }

        return View(guide);
    }
}
