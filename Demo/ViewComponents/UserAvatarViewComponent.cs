using Microsoft.AspNetCore.Mvc;

namespace Demo.ViewComponents;

public class UserAvatarViewComponent : ViewComponent
{
    private readonly DB _db;

    public UserAvatarViewComponent(DB db)
    {
        _db = db;
    }

    public IViewComponentResult Invoke()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return View("Default", (string?)null);

        var u = _db.Users.FirstOrDefault(x => x.Username == User.Identity!.Name);
        var avatarUrl = !string.IsNullOrEmpty(u?.Avatar)
            ? $"/avatars/{u.Avatar}"
            : null;
        return View("Default", avatarUrl);
    }
}
