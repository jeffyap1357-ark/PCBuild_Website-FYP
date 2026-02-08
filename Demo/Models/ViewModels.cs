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
}
