using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Demo.Models;

#nullable disable warnings

// View Models ----------------------------------------------------------------

public class LoginVM
{
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}

public class RegisterVM
{
    [StringLength(100)]
    [Remote("CheckUsername", "Account", ErrorMessage = "Duplicated {0}.")]
    [Display(Name = "Username")]
    public string Username { get; set; }

    [StringLength(256)]
    [EmailAddress]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    public string Email { get; set; }

    [StringLength(256, MinimumLength = 5)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [StringLength(256, MinimumLength = 5)]
    [Compare("Password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }
}

public class UpdatePasswordVM
{
    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string Current { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string New { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [Compare("New")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }
}

public class UpdateProfileVM
{
    [StringLength(100)]
    public string Username { get; set; }

    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; }
}

public class ResetPasswordVM
{
    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; }
}

// PC Build configuration
public class ConfigureBuildVM
{
    [Required, StringLength(200), Display(Name = "Build Name")]
    public string BuildName { get; set; }

    public List<CategoryOptionVM> Categories { get; set; } = [];
}

public class CategoryOptionVM
{
    public int CategoryId { get; set; }
    public string Type { get; set; }
    public List<Hardware> Items { get; set; } = [];
    public int? SelectedHardwareId { get; set; }
    /// <summary>For RAM: one selection per slot (based on motherboard RamSlots).</summary>
    public List<int?> RamSlotSelections { get; set; } = [];
}

// Community
public class ShareBuildVM
{
    public int BuildId { get; set; }
    [StringLength(2000), Display(Name = "Comment (optional)")]
    public string AuthorComment { get; set; }
    [Display(Name = "Build image (optional)")]
    public IFormFile BuildImage { get; set; }
}

// PC Recommend
/// <summary>Optimization strategy for recommendations: performance, budget, or balanced.</summary>
public enum OptimizationType
{
    /// <summary>Prioritize CPU/GPU performance; higher weights on Special score.</summary>
    Performance,
    /// <summary>Prioritize value-for-money; higher weights on price efficiency (Special/Price).</summary>
    Budget,
    /// <summary>Balance performance and value; moderate weights on both.</summary>
    Balanced
}

public class RecommendBuildVM
{
    [Required, Range(1000, 50000), Display(Name = "Budget (RM)")]
    public decimal Budget { get; set; } = 5000;

    [Display(Name = "Target tier (1-10, higher = better performance)")]
    [Range(1, 10)]
    public int TargetTier { get; set; } = 7;

    [Display(Name = "Optimization type")]
    public OptimizationType OptimizationType { get; set; } = OptimizationType.Balanced;
}
