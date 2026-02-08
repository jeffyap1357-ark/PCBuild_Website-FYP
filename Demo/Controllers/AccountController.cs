using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace Demo.Controllers;

public class AccountController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;

    public AccountController(DB db, IWebHostEnvironment en, Helper hp)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
    }

    // GET: Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    public IActionResult Login(LoginVM vm, string? returnURL)
    {
        var u = db.Users.FirstOrDefault(x => x.Email == vm.Email);

        if (u == null || !hp.VerifyPassword(u.Password, vm.Password))
        {
            ModelState.AddModelError("", "Login credentials not matched.");
        }

        if (ModelState.IsValid)
        {
            TempData["Info"] = "Login successfully.";
            hp.SignIn(u!.Username, "User", vm.RememberMe);

            if (string.IsNullOrEmpty(returnURL))
                return RedirectToAction("Index", "Home");
            return Redirect(returnURL);
        }

        return View(vm);
    }

    // GET: Account/Logout
    public IActionResult Logout(string? returnURL)
    {
        TempData["Info"] = "Logout successfully.";
        hp.SignOut();
        return RedirectToAction("Index", "Home");
    }

    // GET: Account/AccessDenied
    public IActionResult AccessDenied(string? returnURL)
    {
        return View();
    }

    // GET: Account/CheckEmail
    public bool CheckEmail(string email)
    {
        return !db.Users.Any(u => u.Email == email);
    }

    // GET: Account/CheckUsername
    public bool CheckUsername(string username)
    {
        return !db.Users.Any(u => u.Username == username);
    }

    // GET: Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    public IActionResult Register(RegisterVM vm)
    {
        if (ModelState.IsValid("Email") && db.Users.Any(u => u.Email == vm.Email))
            ModelState.AddModelError("Email", "Duplicated Email.");
        if (ModelState.IsValid("Username") && db.Users.Any(u => u.Username == vm.Username))
            ModelState.AddModelError("Username", "Duplicated Username.");

        if (ModelState.IsValid)
        {
            db.Users.Add(new User
            {
                Username = vm.Username,
                Email = vm.Email,
                Password = hp.HashPassword(vm.Password),
            });
            db.SaveChanges();
            TempData["Info"] = "Register successfully. Please login.";
            return RedirectToAction("Login");
        }

        return View(vm);
    }

    // GET: Account/UpdatePassword
    [Authorize]
    public IActionResult UpdatePassword()
    {
        return View();
    }

    // POST: Account/UpdatePassword
    [Authorize]
    [HttpPost]
    public IActionResult UpdatePassword(UpdatePasswordVM vm)
    {
        var u = db.Users.FirstOrDefault(x => x.Username == User.Identity!.Name);
        if (u == null) return RedirectToAction("Index", "Home");

        if (!hp.VerifyPassword(u.Password, vm.Current))
            ModelState.AddModelError("Current", "Current Password not matched.");

        if (ModelState.IsValid)
        {
            u.Password = hp.HashPassword(vm.New);
            db.SaveChanges();
            TempData["Info"] = "Password updated.";
            return RedirectToAction();
        }

        return View();
    }

    // GET: Account/UpdateProfile
    [Authorize]
    public IActionResult UpdateProfile()
    {
        var u = db.Users.FirstOrDefault(x => x.Username == User.Identity!.Name);
        if (u == null) return RedirectToAction("Index", "Home");

        var vm = new UpdateProfileVM
        {
            Username = u.Username,
            Email = u.Email,
        };
        return View(vm);
    }

    // POST: Account/UpdateProfile
    [Authorize]
    [HttpPost]
    public IActionResult UpdateProfile(UpdateProfileVM vm)
    {
        var u = db.Users.FirstOrDefault(x => x.Username == User.Identity!.Name);
        if (u == null) return RedirectToAction("Index", "Home");

        if (ModelState.IsValid("Email") && vm.Email != u.Email && db.Users.Any(x => x.Email == vm.Email))
            ModelState.AddModelError("Email", "Duplicated Email.");
        if (ModelState.IsValid("Username") && vm.Username != u.Username && db.Users.Any(x => x.Username == vm.Username))
            ModelState.AddModelError("Username", "Duplicated Username.");

        if (ModelState.IsValid)
        {
            u.Username = vm.Username;
            u.Email = vm.Email;
            db.SaveChanges();
            TempData["Info"] = "Profile updated.";
            return RedirectToAction("Index", "Home");
        }

        return View(vm);
    }

    // GET: Account/ResetPassword
    public IActionResult ResetPassword()
    {
        return View();
    }

    // POST: Account/ResetPassword
    [HttpPost]
    public IActionResult ResetPassword(ResetPasswordVM vm)
    {
        var u = db.Users.FirstOrDefault(x => x.Email == vm.Email);
        if (u == null)
            ModelState.AddModelError("Email", "Email not found.");

        if (ModelState.IsValid)
        {
            string password = hp.RandomPassword();
            u!.Password = hp.HashPassword(password);
            db.SaveChanges();
            SendResetPasswordEmail(u, password);
            TempData["Info"] = "Password reset. Check your email.";
            return RedirectToAction("Login");
        }

        return View(vm);
    }

    private void SendResetPasswordEmail(User u, string password)
    {
        var mail = new MailMessage();
        mail.To.Add(new MailAddress(u.Email, u.Username));
        mail.Subject = "PC Build - Reset Password";
        mail.IsBodyHtml = true;
        var url = Url.Action("Login", "Account", null, Request.Scheme);
        mail.Body = $@"
            <p>Dear {u.Username},</p>
            <p>Your password has been reset to:</p>
            <h2 style='color: red'>{password}</h2>
            <p>Please <a href='{url}'>login</a> with your new password.</p>
            <p>From, PC Build</p>
        ";
        hp.SendEmail(mail);
    }
}
