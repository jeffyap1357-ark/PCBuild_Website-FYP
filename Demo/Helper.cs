using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Demo.Models;

namespace Demo;

public class Helper
{
    /// <summary>GPU names that are duplicates (same product, different name). Hidden from lists; canonical names like "RTX 4060 8GB" are shown.</summary>
    public static readonly HashSet<string> GpuDuplicateDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "RTX 4060", "RTX 4070", "RX 6600", "RX 7800 XT"
    };

    /// <summary>Remove duplicate GPU entries from display (same product, different name). Data stays in DB.</summary>
    /// <param name="categoryType">When "GPU", filter by duplicate names; optional when list items have Category loaded.</param>
    public static List<Hardware> FilterDuplicateGpuFromDisplay(List<Hardware> list, string? categoryType = null)
    {
        var isGpu = categoryType == "GPU" || list.Count > 0 && list[0].Category?.Type == "GPU";
        if (!isGpu) return list;
        return list.Where(h => !GpuDuplicateDisplayNames.Contains(h.Name?.Trim() ?? "")).ToList();
    }

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
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

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

    /// <summary>Save uploaded image with a unique filename to avoid duplicates. Returns the saved filename.</summary>
    public string SaveHardwareImage(IFormFile file, string folder)
    {
        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
            ext = ".jpg";
        var fileName = Guid.NewGuid().ToString("n") + ext;
        var path = Path.Combine(en.WebRootPath, folder, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var stream = file.OpenReadStream();
        using var outStream = System.IO.File.Create(path);
        stream.CopyTo(outStream);
        return fileName;
    }

    // ------------------------------------------------------------------------
    // Hardware Image Helper
    // ------------------------------------------------------------------------

    private static readonly Dictionary<string, string> HardwareImageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ASUS B760-PLUS WIFI"] = "b760.png", ["MSI PRO B760M-A WIFI"] = "b760m.png",
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

    // Build page prefers transparent "clean" images from /implant when available.
    public string GetBuildImagePath(string? picture, string name, string? cleanPicture = null)
    {
        var implantFolder = Path.Combine(en.WebRootPath, "implant");
        if (!string.IsNullOrWhiteSpace(cleanPicture) && Directory.Exists(implantFolder))
        {
            var cleanPath = Path.Combine(implantFolder, Path.GetFileName(cleanPicture));
            if (System.IO.File.Exists(cleanPath))
                return "implant/" + Path.GetFileName(cleanPicture);
        }

        var hardwareFile = GetHardwareImagePath(picture, name);
        var hardwarePath = string.IsNullOrWhiteSpace(hardwareFile) ? "" : $"hardware/{hardwareFile}";

        if (!Directory.Exists(implantFolder))
            return hardwarePath;

        var implantFiles = Directory.EnumerateFiles(implantFolder)
            .Select(Path.GetFileName)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Cast<string>()
            .ToList();

        if (implantFiles.Count == 0)
            return hardwarePath;

        static string Normalize(string s) => Regex.Replace(s.ToLowerInvariant(), "[^a-z0-9]+", "");

        // B550 motherboard: use new image from implant (same filename, replaced file)
        var nameLower = (name ?? "").ToLowerInvariant();
        var picLower = (picture ?? "").ToLowerInvariant();
        if (nameLower.Contains("b550") || picLower.Contains("b550"))
        {
            var b550Match = implantFiles.FirstOrDefault(f =>
            {
                var n = Normalize(f);
                return n.Contains("b550") && (n.Contains("removebg") || n.Contains("clean") || n.Contains("usethis"));
            });
            if (b550Match != null)
                return $"implant/{b550Match}";
        }

        var keys = new List<string>();
        if (!string.IsNullOrWhiteSpace(hardwareFile))
            keys.Add(Path.GetFileNameWithoutExtension(hardwareFile));
        if (!string.IsNullOrWhiteSpace(picture))
            keys.Add(Path.GetFileNameWithoutExtension(picture));
        if (!string.IsNullOrWhiteSpace(name))
        {
            keys.Add(name);
            foreach (var word in name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                keys.Add(word);
        }

        var normalizedKeys = keys
            .Select(Normalize)
            .Where(k => k.Length >= 3)
            .Distinct()
            .ToList();

        string? match = null;
        foreach (var key in normalizedKeys)
        {
            match = implantFiles.FirstOrDefault(f =>
            {
                var normalizedFile = Normalize(f);
                return normalizedFile.Contains(key) &&
                       (normalizedFile.Contains("removebg") || normalizedFile.Contains("clean") || normalizedFile.Contains("usethis"));
            });
            if (match != null) break;
        }

        return match != null ? $"implant/{match}" : hardwarePath;
    }

    /// <summary>Layout key for PC case (data-case). Used to position MB + PSU on the case.</summary>
    public static string GetCaseLayoutKey(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "default";
        var n = name.ToLowerInvariant();
        if (n.Contains("td300")) return "td300";
        if (n.Contains("h5 flow") || n.Contains("h5flow")) return "h5flow";
        if (n.Contains("nexus") && n.Contains("m")) return "nexusm";
        return "default";
    }

    /// <summary>RAM diagram: pic 1–3 by brand (Corsair, G.Skill, Kingston); pic 4 = other. Empty slot shows no image. Name used when brand missing (e.g. Kingston 32GB DDR5 6000MHz).</summary>
    private static readonly Dictionary<string, string> RamBrandToImage = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Corsair"] = "implant/ram_1.png",
        ["G.Skill"] = "implant/ram_2.png",
        ["Kingston"] = "implant/ram_3.png",
    };

    public static string GetRamDisplayImage(string? brand, string? name = null)
    {
        if (!string.IsNullOrWhiteSpace(brand) && RamBrandToImage.TryGetValue(brand.Trim(), out var path))
            return path;
        var n = (name ?? "").ToLowerInvariant();
        if (n.Contains("kingston")) return "implant/ram_3.png";
        if (n.Contains("corsair")) return "implant/ram_1.png";
        if (n.Contains("g.skill") || n.Contains("gskill")) return "implant/ram_2.png";
        if (!string.IsNullOrWhiteSpace(brand)) return "implant/ram_4.png";
        return "implant/ram_4.png";
    }

    /// <summary>Layout key for motherboard (data-mb-layout). Each board has different slot positions for CPU, RAM, GPU, etc.</summary>
    public static string GetMotherboardLayoutKey(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "default";
        var n = name.ToLowerInvariant();
        if (n.Contains("b450m")) return "b450m";
        if (n.Contains("b560m")) return "b560m";
        if (n.Contains("b650m")) return "b650m";
        if (n.Contains("b760m")) return "b760m";
        if (n.Contains("b760")) return "b760";
        if (n.Contains("b550")) return "b550";
        if (n.Contains("b450")) return "b450";
        return "default";
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
        string host = cf["Smtp:Host"] ?? "smtp.gmail.com";
        int port = cf.GetValue<int>("Smtp:Port");
        if (port <= 0) port = 587;

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(name, user));
        foreach (var to in mail.To)
            msg.To.Add(new MailboxAddress(to.DisplayName, to.Address));
        msg.Subject = mail.Subject;
        msg.Body = new TextPart(mail.IsBodyHtml ? "html" : "plain") { Text = mail.Body };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        client.Connect(host, port, SecureSocketOptions.StartTls);
        client.Authenticate(user, pass);
        client.Send(msg);
        client.Disconnect(true);
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
