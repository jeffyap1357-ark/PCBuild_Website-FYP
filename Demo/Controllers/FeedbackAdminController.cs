using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

[Authorize(Roles = Roles.Admin)]
public class FeedbackAdminController : Controller
{
    private readonly DB db;

    public FeedbackAdminController(DB db)
    {
        this.db = db;
    }

    public IActionResult Index()
    {
        var all = db.Feedbacks.OrderByDescending(f => f.CreatedAt).ToList();
        var total = all.Count;
        double avg = total > 0 ? all.Average(f => f.Rating) : 0;

        var dist = new List<FeedbackDistributionItemVM>();
        for (int r = 1; r <= 5; r++)
        {
            var count = all.Count(f => f.Rating == r);
            dist.Add(new FeedbackDistributionItemVM
            {
                Rating = r,
                Count = count,
                Percentage = total > 0 ? (double)count * 100.0 / total : 0
            });
        }

        var vm = new FeedbackReportVM
        {
            AverageRating = avg,
            TotalCount = total,
            Distribution = dist.OrderByDescending(d => d.Rating).ToList(),
            Feedbacks = all
        };

        ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(dist.Select(d => new { rating = d.Rating, count = d.Count }));
        return View(vm);
    }
}

