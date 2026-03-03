using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

[Authorize]
public class FeedbackController : Controller
{
    private readonly DB db;

    public FeedbackController(DB db)
    {
        this.db = db;
    }

    private int? GetCurrentUserId()
    {
        var name = User.Identity?.Name;
        if (string.IsNullOrEmpty(name)) return null;
        return db.Users.Where(u => u.Username == name).Select(u => u.UserId).FirstOrDefault();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(FeedbackCreateVM vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Info"] = "Feedback not saved. Please select a rating between 1 and 5.";
            return Redirect(vm.ReturnUrl ?? "/");
        }

        var userId = GetCurrentUserId();

        var fb = new Feedback
        {
            UserId = userId,
            Rating = vm.Rating,
            Comment = string.IsNullOrWhiteSpace(vm.Comment) ? null : vm.Comment.Trim(),
            ModuleName = vm.ModuleName,
            PageUrl = vm.PageUrl,
            CreatedAt = DateTime.UtcNow
        };

        db.Feedbacks.Add(fb);
        db.SaveChanges();

        TempData["Info"] = "Thank you for your feedback!";
        return Redirect(vm.ReturnUrl ?? "/");
    }
}

