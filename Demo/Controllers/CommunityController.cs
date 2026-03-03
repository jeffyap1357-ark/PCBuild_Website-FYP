using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

public class CommunityController : Controller
{
    private readonly DB db;
    private readonly Helper helper;

    public CommunityController(DB db, Helper helper)
    {
        this.db = db;
        this.helper = helper;
    }

    private int? GetCurrentUserId()
    {
        var name = User.Identity?.Name;
        if (string.IsNullOrEmpty(name)) return null;
        return db.Users.Where(u => u.Username == name).Select(u => u.UserId).FirstOrDefault();
    }

    public IActionResult Index(string search, string sortBy)
    {
        var query = db.Communities.Include(c => c.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(c =>
                c.BuildName.ToLower().Contains(q) ||
                (c.User.Username != null && c.User.Username.ToLower().Contains(q)));
        }

        query = sortBy switch
        {
            "rating-high" => query.OrderByDescending(c => c.AverageRating ?? 0).ThenByDescending(c => c.SharedAt),
            "rating-low" => query.OrderBy(c => c.AverageRating ?? 0).ThenByDescending(c => c.SharedAt),
            _ => query.OrderByDescending(c => c.SharedAt)
        };

        var list = query.ToList();
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy ?? "";
        return View(list);
    }

    public IActionResult Detail(int id)
    {
        var community = db.Communities
            .Include(c => c.User)
            .Include(c => c.Build)
            .ThenInclude(b => b.BuildDetails)
            .ThenInclude(bd => bd.Hardware)
            .ThenInclude(h => h.Category)
            .Include(c => c.CommunityComments.OrderBy(cc => cc.CreatedAt))
            .ThenInclude(cc => cc.User)
            .FirstOrDefault(c => c.CommunityId == id);

        if (community == null) return RedirectToAction(nameof(Index));

        var userId = GetCurrentUserId();
        ViewBag.UserId = userId;
        ViewBag.UserRating = userId.HasValue
            ? db.CommunityRatings.FirstOrDefault(r => r.CommunityId == id && r.UserId == userId)?.Rating
            : null;

        return View(community);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Share(int buildId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var build = db.Builds
            .Include(b => b.BuildDetails)
            .ThenInclude(bd => bd.Hardware)
            .FirstOrDefault(b => b.BuildId == buildId && b.UserId == userId);
        if (build == null) return RedirectToAction(nameof(Index), "Build");
        if (build.IsShared) return RedirectToAction(nameof(Detail), new { id = db.Communities.First(c => c.BuildId == buildId).CommunityId });

        return View(new ShareBuildVM { BuildId = buildId });
    }

    [Authorize]
    [HttpPost]
    public IActionResult Share(ShareBuildVM vm)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var build = db.Builds.FirstOrDefault(b => b.BuildId == vm.BuildId && b.UserId == userId);
        if (build == null) return RedirectToAction(nameof(Index), "Build");
        if (build.IsShared) return RedirectToAction(nameof(Index));

        string imageFile = null;
        if (vm.BuildImage != null && vm.BuildImage.Length > 0)
        {
            var err = helper.ValidatePhoto(vm.BuildImage);
            if (string.IsNullOrEmpty(err))
                imageFile = helper.SavePhoto(vm.BuildImage, "images/community");
        }

        var community = new Community
        {
            BuildId = build.BuildId,
            UserId = userId.Value,
            BuildName = build.BuildName,
            TotalPrice = build.TotalPrice,
            SharedAt = DateTime.UtcNow,
            RatingCount = 0,
            AuthorComment = vm.AuthorComment?.Trim(),
            Image = imageFile
        };
        db.Communities.Add(community);
        build.IsShared = true;
        build.SharedAt = DateTime.UtcNow;
        db.SaveChanges();

        TempData["Info"] = "Build shared to Community.";
        return RedirectToAction(nameof(Detail), new { id = community.CommunityId });
    }

    [Authorize]
    [HttpPost]
    public IActionResult Comment(int communityId, string commentText)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");
        if (string.IsNullOrWhiteSpace(commentText) || commentText.Length > 1000) return RedirectToAction(nameof(Detail), new { id = communityId });

        var community = db.Communities.FirstOrDefault(c => c.CommunityId == communityId);
        if (community == null) return RedirectToAction(nameof(Index));

        db.CommunityComments.Add(new CommunityComment
        {
            CommunityId = communityId,
            UserId = userId.Value,
            CommentText = commentText.Trim(),
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
        TempData["Info"] = "Comment added.";
        return RedirectToAction(nameof(Detail), new { id = communityId });
    }

    [Authorize]
    [HttpPost]
    public IActionResult Rate(int communityId, int rating)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");
        if (rating < 1 || rating > 5) return RedirectToAction(nameof(Detail), new { id = communityId });

        var community = db.Communities.FirstOrDefault(c => c.CommunityId == communityId);
        if (community == null) return RedirectToAction(nameof(Index));

        var existing = db.CommunityRatings.FirstOrDefault(r => r.CommunityId == communityId && r.UserId == userId);
        if (existing != null)
        {
            existing.Rating = rating;
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            db.CommunityRatings.Add(new CommunityRating
            {
                CommunityId = communityId,
                UserId = userId.Value,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            });
        }
        db.SaveChanges();

        var ratings = db.CommunityRatings.Where(r => r.CommunityId == communityId).ToList();
        community.AverageRating = ratings.Count > 0 ? (decimal)ratings.Average(r => r.Rating) : null;
        community.RatingCount = ratings.Count;
        db.SaveChanges();

        TempData["Info"] = "Rating saved.";
        return RedirectToAction(nameof(Detail), new { id = communityId });
    }
}
