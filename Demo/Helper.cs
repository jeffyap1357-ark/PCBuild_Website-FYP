using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Demo;

public class Helper
{
    private readonly IWebHostEnvironment en;
    private readonly IHttpContextAccessor ct;
    private readonly IConfiguration cf;

    public Helper(IWebHostEnvironment en,
                  IHttpContextAccessor ct,
                  IConfiguration cf)
    {
        this.en = en;
        this.ct = ct;
        this.cf = cf;
    }

    // ------------------------------------------------------------------------
    // Photo Upload Helper Functions
    // ------------------------------------------------------------------------

    public string ValidatePhoto(IFormFile f)
    {
        var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
        var reName = new Regex(@"^.+\.(jpeg|jpg|png)$", RegexOptions.IgnoreCase);

        if (!reType.IsMatch(f.ContentType) || !reName.IsMatch(f.FileName))
        {
            return "Only JPG and PNG photo is allowed.";
        }
        else if (f.Length > 1 * 1024 * 1024)
        {
            return "Photo size cannot more than 1MB.";
        }

        return "";
    }

    public string SavePhoto(IFormFile f, string folder)
    {
        var file = Guid.NewGuid().ToString("n") + ".jpg";
        var path = Path.Combine(en.WebRootPath, folder, file);

        var options = new ResizeOptions
        {
            Size = new(200, 200),
            Mode = ResizeMode.Crop,
        };

        using var stream = f.OpenReadStream();
        using var img = Image.Load(stream);
        img.Mutate(x => x.Resize(options));
        img.Save(path);

        return file;
    }

    public void DeletePhoto(string file, string folder)
    {
        file = Path.GetFileName(file);
        var path = Path.Combine(en.WebRootPath, folder, file);
        File.Delete(path);
    }

    // ------------------------------------------------------------------------
    // Hardware Image Helper
    // ------------------------------------------------------------------------

    private static readonly Dictionary<string, string> HardwareImageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ASUS B760-PLUS WIFI"] = "b760.png", ["MSI PRO B760M-A WIFI"] = "b760m.png", ["MSI B550-A PRO"] = "b550a.png",
        ["ASRock B650M Pro RS"] = "b650mpro.png", ["Intel Core i5-13400F"] = "intel5134.png", ["Intel Core i7-13700K"] = "intel5134.png",
        ["Ryzen 5 5600"] = "r55600.png", ["Ryzen 7 7700X"] = "r77700x.png", ["RTX 4060"] = "rtx4060.png", ["RTX 4070"] = "rtx4070.png",
        ["RX 6600"] = "rx660.png", ["RX 7800 XT"] = "rx780.png", ["Corsair 16GB DDR4 3200MHz"] = "consair16.png",
        ["Kingston 16GB DDR5 5200MHz"] = "kingston16.png", ["Corsair 32GB DDR4 3600MHz"] = "consair32.png",
        ["Kingston 32GB DDR5 6000MHz"] = "kingston32.png", ["Samsung 970 EVO 1TB"] = "samsung970.png",
        ["WD SN770 1TB"] = "wdsn850x.png", ["Samsung 980 PRO 2TB"] = "980pro.png",
        ["Corsair 650W 80+ Bronze"] = "650consair.png", ["Cooler Master 750W 80+ Gold"] = "cm750.png",
        ["Seasonic 850W 80+ Gold"] = "sea850.png", ["NZXT H5 Flow"] = "h5flow.png", ["Cooler Master TD300"] = "cttd300.png"
    };

    public string GetHardwareImagePath(string? picture, string name)
    {
        var folder = Path.Combine(en.WebRootPath, "hardware");
        if (!string.IsNullOrEmpty(picture) && File.Exists(Path.Combine(folder, picture)))
            return picture;
        if (HardwareImageMap.TryGetValue(name.Trim(), out var mapped))
            return mapped;
        foreach (var kv in HardwareImageMap)
            if (name.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        return picture ?? "";
    }

    // ------------------------------------------------------------------------
    // Security Helper Functions
    // ------------------------------------------------------------------------

    private readonly PasswordHasher<object> ph = new();

    public string HashPassword(string password)
    {
        return ph.HashPassword(0, password);
    }

    public bool VerifyPassword(string hash, string password)
    {
        return ph.VerifyHashedPassword(0, hash, password)
               == PasswordVerificationResult.Success;
    }

    public void SignIn(string email, string role, bool rememberMe)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Role, role),
        ];

        ClaimsIdentity identity = new(claims, "Cookies");

        ClaimsPrincipal principal = new(identity);

        AuthenticationProperties properties = new()
        {
            IsPersistent = rememberMe,
        };

        ct.HttpContext!.SignInAsync(principal, properties);
    }

    public void SignOut()
    {
        ct.HttpContext!.SignOutAsync();
    }

    public string RandomPassword()
    {
        string s = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string password = "";
        
        Random r = new();

        for (int i = 1; i <= 10; i++)
        {
            password += s[r.Next(s.Length)];
        }
       
        return password;
    }



    // ------------------------------------------------------------------------
    // Email Helper Functions
    // ------------------------------------------------------------------------

    public void SendEmail(MailMessage mail)
    {
        string user = cf["Smtp:User"] ?? "";
        string pass = cf["Smtp:Pass"] ?? "";
        string name = cf["Smtp:Name"] ?? "";
        string host = cf["Smtp:Host"] ?? "";
        int port = cf.GetValue<int>("Smtp:Port");

        mail.From = new MailAddress(user, name);

        using var smtp = new SmtpClient
        {
            Host = host,
            Port = port,
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass),
        };

        smtp.Send(mail);
    }



    // ------------------------------------------------------------------------
    // DateTime Helper Functions
    // ------------------------------------------------------------------------

    // Return January (1) to December (12)
    public SelectList GetMonthList()
    {
        var list = new List<object>();

        for (int n = 1; n <= 12; n++)
        {
            list.Add(new
            {
                Id = n,
                Name = new DateTime(1, n, 1).ToString("MMMM"),
            });
        }

        return new SelectList(list, "Id", "Name");
    }

    // Return min to max years
    public SelectList GetYearList(int min, int max, bool reverse = false)
    {
        var list = new List<int>();

        for (int n = min; n <= max; n++)
        {
            list.Add(n);
        }

        if (reverse) list.Reverse();

        return new SelectList(list);
    }



    // ------------------------------------------------------------------------
    // Shopping Cart Helper Functions
    // ------------------------------------------------------------------------

    public Dictionary<string, int> GetCart()
    {
        return ct.HttpContext!.Session.Get<Dictionary<string, int>>("Cart") ?? [];
    }

    public void SetCart(Dictionary<string, int>? dict = null)
    {
        if (dict == null)
        {
            ct.HttpContext!.Session.Remove("Cart");
        }
        else
        {
            ct.HttpContext!.Session.Set("Cart", dict);
        }
    }
}
