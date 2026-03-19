using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Demo.Controllers;

[Authorize]
public class LearningAdminController : Controller
{
    private readonly DB _db;
    private readonly Helper _helper;

    public LearningAdminController(DB db, Helper helper)
    {
        _db = db;
        _helper = helper;
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
    public IActionResult Save(
        int guideId,
        string guideName,
        string guideDesc,
        int sortOrder,
        string modulesJson,
        List<IFormFile> moduleImageFiles)
    {
        guideName = (guideName ?? "").Trim();
        if (string.IsNullOrEmpty(guideName))
        {
            ModelState.AddModelError("GuideName", "Guide name is required.");
            return View("Edit", new Guide { GuideId = guideId, GuideName = guideName, GuideDesc = guideDesc ?? "", ContentModules = modulesJson ?? "[]", SortOrder = sortOrder });
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        List<GuideModule>? modules = null;
        try
        {
            modules = string.IsNullOrWhiteSpace(modulesJson)
                ? new List<GuideModule>()
                : JsonSerializer.Deserialize<List<GuideModule>>(modulesJson, jsonOptions);
        }
        catch
        {
            ModelState.AddModelError("", "Invalid modules data.");
        }

        if (modules == null)
            modules = new List<GuideModule>();

        // Save uploaded guide images (unique filename rename to avoid duplicates).
        // The JS assigns `fileSlot` based on the visible order of image modules with selected files.
        var imageFiles = moduleImageFiles ?? [];
        for (var i = 0; i < modules.Count; i++)
        {
            var m = modules[i];
            if (!string.Equals(m.Type, "image", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!m.FileSlot.HasValue)
                continue;

            var slot = m.FileSlot.Value;
            if (slot < 0 || slot >= imageFiles.Count)
                continue;

            var file = imageFiles[slot];
            if (file == null || file.Length <= 0)
                continue;

            var savedName = _helper.SaveHardwareImage(file, "guide-images");
            m.Value = "/guide-images/" + savedName;
            m.FileSlot = null; // not needed after persisting
        }

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
                case "video":
                    var link = (value ?? "").Trim();
                    if (!string.IsNullOrWhiteSpace(link))
                    {
                        var captionHtml = string.IsNullOrWhiteSpace(caption)
                            ? ""
                            : "<div class=\"guide-video-caption\">" + System.Net.WebUtility.HtmlEncode(caption) + "</div>";

                        var youtubeId = ExtractYoutubeId(link);
                        if (!string.IsNullOrWhiteSpace(youtubeId))
                        {
                            parts.Add("<div class=\"guide-video\">" +
                                      "<iframe " +
                                      "src=\"https://www.youtube-nocookie.com/embed/" + System.Net.WebUtility.HtmlEncode(youtubeId) + "\" " +
                                      "title=\"YouTube video\" " +
                                      "frameborder=\"0\" " +
                                      "allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share\" " +
                                      "allowfullscreen></iframe>" +
                                      "</div>" +
                                      captionHtml);
                        }
                        else
                        {
                            // For non-YouTube: provide safe link fallback.
                            parts.Add("<div class=\"guide-video\">" +
                                      "<a href=\"" + System.Net.WebUtility.HtmlEncode(link) + "\" target=\"_blank\" rel=\"noopener\">Open video</a>" +
                                      "</div>" +
                                      captionHtml);
                        }
                    }
                    break;
            }
        }
        return string.Join("", parts);
    }

    private static string? ExtractYoutubeId(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var trimmed = url.Trim();

        // Common YouTube URL formats:
        // - watch?v=<id>
        // - youtu.be/<id>
        // - /embed/<id>
        // - /shorts/<id>
        // - /live/<id>
        // YouTube video id is 11 chars: [A-Za-z0-9_-]
        var m = System.Text.RegularExpressions.Regex.Match(
            trimmed,
            @"(?:v=|youtu\.be/|\/embed\/|\/shorts\/|\/live\/)([A-Za-z0-9_-]{11})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (m.Success)
            return m.Groups[1].Value;

        return null;
    }
}

public class GuideModule
{
    public string? Type { get; set; }
    public string? Value { get; set; }
    public string? Caption { get; set; }
    public int? FileSlot { get; set; }
}
