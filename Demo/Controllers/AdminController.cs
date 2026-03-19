using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly DB _db;

    public AdminController(DB db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var hardwareByCat = _db.Hardware
            .Include(h => h.Category)
            .GroupBy(h => h.Category != null ? h.Category.Type : "Other")
            .Select(g => new { label = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .ToList();
        var feedbackByRating = _db.Feedbacks
            .GroupBy(f => f.Rating)
            .Select(g => new { rating = g.Key, count = g.Count() })
            .OrderBy(x => x.rating)
            .ToList();
        var usersByRole = _db.Users
            .GroupBy(u => string.IsNullOrEmpty(u.Role) ? "User" : u.Role)
            .Select(g => new { label = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .ToList();

        var vm = new AdminDashboardVM
        {
            UserCount = _db.Users.Count(),
            BuildCount = _db.Builds.Count(),
            HardwareCount = _db.Hardware.Count(),
            QuizAttemptCount = _db.QuizAttempts.Count(),
            PerformanceReportCount = _db.PerformanceReports.Count(),
            FeedbackCount = _db.Feedbacks.Count(),
            CommunityCount = _db.Communities.Count(),
            HardwareByCategory = hardwareByCat.Select(x => new KeyValuePair<string, int>(x.label, x.count)).ToList(),
            FeedbackByRating = feedbackByRating.Select(x => new KeyValuePair<int, int>(x.rating, x.count)).ToList()
        };
        ViewBag.HardwareChartData = JsonSerializer.Serialize(hardwareByCat);
        ViewBag.FeedbackChartData = JsonSerializer.Serialize(feedbackByRating);
        ViewBag.UsersByRoleData = JsonSerializer.Serialize(usersByRole);
        return View(vm);
    }
}
