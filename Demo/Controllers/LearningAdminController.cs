using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Demo.Controllers;

[Authorize]
public class LearningAdminController : Controller
{
    private readonly DB _db;

    public LearningAdminController(DB db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var guides = _db.Guides.OrderBy(g => g.SortOrder).ToList();
        return View(guides);
    }

    public IActionResult Create()
    {
        return View("Edit", new Guide
        {
            GuideId = 0,
            GuideName = "",
            GuideDesc = "",
            Content = "",
            ContentModules = "[]",
            SortOrder = (_db.Guides.Any() ? _db.Guides.Max(g => g.SortOrder) : 0) + 1
        });
    }

    public IActionResult Edit(int id)
    {
        var guide = _db.Guides.FirstOrDefault(g => g.GuideId == id);
        if (guide == null)
            return RedirectToAction(nameof(Index));
        if (string.IsNullOrWhiteSpace(guide.ContentModules))
            guide.ContentModules = "[]";
        return View(guide);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Save(int guideId, string guideName, string guideDesc, int sortOrder, string modulesJson)
    {
        guideName = (guideName ?? "").Trim();
        if (string.IsNullOrEmpty(guideName))
        {
            ModelState.AddModelError("GuideName", "Guide name is required.");
            return View("Edit", new Guide { GuideId = guideId, GuideName = guideName, GuideDesc = guideDesc ?? "", ContentModules = modulesJson ?? "[]", SortOrder = sortOrder });
        }

        List<GuideModule>? modules = null;
        try
        {
            modules = string.IsNullOrWhiteSpace(modulesJson) ? new List<GuideModule>() : JsonSerializer.Deserialize<List<GuideModule>>(modulesJson);
        }
        catch
        {
            ModelState.AddModelError("", "Invalid modules data.");
        }

        if (modules == null)
            modules = new List<GuideModule>();

        var html = BuildContentHtml(modules);

        if (guideId == 0)
        {
            var guide = new Guide
            {
                GuideName = guideName,
                GuideDesc = (guideDesc ?? "").Trim(),
                SortOrder = sortOrder,
                Content = html,
                ContentModules = JsonSerializer.Serialize(modules)
            };
            _db.Guides.Add(guide);
            _db.SaveChanges();
            TempData["Info"] = "Learning guide created.";
        }
        else
        {
            var guide = _db.Guides.FirstOrDefault(g => g.GuideId == guideId);
            if (guide == null)
                return RedirectToAction(nameof(Index));
            guide.GuideName = guideName;
            guide.GuideDesc = (guideDesc ?? "").Trim();
            guide.SortOrder = sortOrder;
            guide.Content = html;
            guide.ContentModules = JsonSerializer.Serialize(modules);
            _db.SaveChanges();
            TempData["Info"] = "Learning guide updated.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var guide = _db.Guides.FirstOrDefault(g => g.GuideId == id);
        if (guide != null)
        {
            _db.Guides.Remove(guide);
            _db.SaveChanges();
            TempData["Info"] = "Guide deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private static string BuildContentHtml(List<GuideModule> modules)
    {
        var parts = new List<string>();
        foreach (var m in modules)
        {
            var value = m.Value ?? "";
            var caption = m.Caption ?? "";
            switch ((m.Type ?? "").ToLowerInvariant())
            {
                case "title":
                    parts.Add("<h3>" + System.Net.WebUtility.HtmlEncode(value) + "</h3>");
                    break;
                case "content":
                    parts.Add("<p>" + System.Net.WebUtility.HtmlEncode(value).Replace("\r\n", "</p><p>").Replace("\n", "</p><p>") + "</p>");
                    break;
                case "image":
                    var alt = string.IsNullOrWhiteSpace(caption) ? System.Net.WebUtility.HtmlEncode(value) : System.Net.WebUtility.HtmlEncode(caption);
                    parts.Add(string.IsNullOrWhiteSpace(value)
                        ? ""
                        : "<figure class=\"guide-fig\"><img src=\"" + System.Net.WebUtility.HtmlEncode(value.Trim()) + "\" alt=\"" + alt + "\"><figcaption>" + System.Net.WebUtility.HtmlEncode(caption) + "</figcaption></figure>");
                    break;
            }
        }
        return string.Join("", parts);
    }
}

public class GuideModule
{
    public string? Type { get; set; }
    public string? Value { get; set; }
    public string? Caption { get; set; }
}
