using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminAccountController : Controller
{
    private readonly DB db;
    private readonly Helper hp;

    public AdminAccountController(DB db, Helper hp)
    {
        this.db = db;
        this.hp = hp;
    }

    /// <summary>Root admin (admin@gmail.com) cannot be edited or deleted.</summary>
    private static bool IsRootAdmin(User u) => u.IsRootAdmin;

    public IActionResult Index()
    {
        var admins = db.Users.Where(u => u.Role == Roles.Admin).OrderBy(u => u.Username).ToList();
        return View(admins);
    }

    public IActionResult Create()
    {
        return View(new AdminCreateVM());
    }

    [HttpPost]
    public IActionResult Create(AdminCreateVM vm)
    {
        if (ModelState.IsValid("Email") && db.Users.Any(u => u.Email == vm.Email))
            ModelState.AddModelError("Email", "Email already exists.");
        if (ModelState.IsValid("Username") && db.Users.Any(u => u.Username == vm.Username))
            ModelState.AddModelError("Username", "Username already exists.");

        if (ModelState.IsValid)
        {
            db.Users.Add(new User
            {
                Username = vm.Username.Trim(),
                Email = vm.Email.Trim(),
                Password = hp.HashPassword(vm.Password),
                Role = Roles.Admin,
                IsRootAdmin = false
            });
            db.SaveChanges();
            TempData["Info"] = "Admin account created.";
            return RedirectToAction(nameof(Index));
        }

        return View(vm);
    }

    public IActionResult Edit(int id)
    {
        var u = db.Users.Find(id);
        if (u == null || u.Role != Roles.Admin) return RedirectToAction(nameof(Index));
        if (IsRootAdmin(u))
        {
            TempData["Info"] = "Root admin account cannot be edited.";
            return RedirectToAction(nameof(Index));
        }

        return View(new AdminEditVM
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email
        });
    }

    [HttpPost]
    public IActionResult Edit(int id, AdminEditVM vm)
    {
        var u = db.Users.Find(id);
        if (u == null || u.Role != Roles.Admin) return RedirectToAction(nameof(Index));
        if (IsRootAdmin(u))
        {
            TempData["Info"] = "Root admin account cannot be edited.";
            return RedirectToAction(nameof(Index));
        }

        vm.UserId = id;
        if (ModelState.IsValid("Email") && vm.Email != u.Email && db.Users.Any(x => x.Email == vm.Email))
            ModelState.AddModelError("Email", "Email already exists.");
        if (ModelState.IsValid("Username") && vm.Username != u.Username && db.Users.Any(x => x.Username == vm.Username))
            ModelState.AddModelError("Username", "Username already exists.");

        if (ModelState.IsValid)
        {
            u.Username = vm.Username.Trim();
            u.Email = vm.Email.Trim();
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
                u.Password = hp.HashPassword(vm.NewPassword);
            db.SaveChanges();
            TempData["Info"] = "Admin account updated.";
            return RedirectToAction(nameof(Index));
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var u = db.Users.Find(id);
        if (u == null || u.Role != Roles.Admin) return RedirectToAction(nameof(Index));
        if (IsRootAdmin(u))
        {
            TempData["Info"] = "Root admin account cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        db.Users.Remove(u);
        db.SaveChanges();
        TempData["Info"] = "Admin account deleted.";
        return RedirectToAction(nameof(Index));
    }
}
