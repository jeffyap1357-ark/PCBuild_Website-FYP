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

    [MaxLength(20)]
    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    [Display(Name = "Profile Picture")]
    public IFormFile? AvatarFile { get; set; }
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
/// <summary>Optimization strategy for recommendations.</summary>
public enum OptimizationType
{
    /// <summary>Prioritize CPU/GPU performance; higher weights on Special score.</summary>
    Performance,
    /// <summary>Prioritize value-for-money; higher weights on price efficiency.</summary>
    Budget,
    /// <summary>Balance performance and value.</summary>
    Balanced,
    /// <summary>Gaming-focused; uses GamingScore weighting.</summary>
    Gaming,
    /// <summary>Office/productivity-focused; uses OfficeScore weighting.</summary>
    Office,
    /// <summary>Content editing-focused; uses EditingScore weighting.</summary>
    Editing
}

public class RecommendBuildVM
{
    [Required, Range(1000, 50000), Display(Name = "Budget (RM)")]
    public decimal Budget { get; set; } = 5000;

    [Display(Name = "Optimization type")]
    public OptimizationType OptimizationType { get; set; } = OptimizationType.Balanced;
}

// Performance evaluation (user-selected build with budget and score)
public class PerformanceBuildVM
{
    public decimal Budget { get; set; } = 500000;

    public List<CategoryOptionVM> Categories { get; set; } = [];

    public decimal TotalPrice { get; set; }

    public int TotalScore { get; set; }

    // Whether result (price/score) is calculated
    public bool HasResult { get; set; }
}

// Quiz: user takes PC build knowledge test
public class QuizQuestionAnswerVM
{
    public int QuizQuestionId { get; set; }

    public string QuestionText { get; set; }

    public string OptionA { get; set; }
    public string OptionB { get; set; }
    public string OptionC { get; set; }
    public string OptionD { get; set; }

    // User selected answer: "A", "B", "C", or "D"
    public string? SelectedOption { get; set; }

    // Filled after grading
    public string CorrectOption { get; set; }
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
}

public class QuizTakeVM
{
    public string Level { get; set; } = "Beginner";
    public List<QuizQuestionAnswerVM> Questions { get; set; } = [];

    public int TotalQuestions => Questions.Count;

    public int CorrectCount { get; set; }

    public bool IsSubmitted { get; set; }
}

// Feedback: user rating and comment submission
public class FeedbackCreateVM
{
    [Required, Range(1, 5)]
    [Display(Name = "Rating (1-5)")]
    public int Rating { get; set; }

    [StringLength(1000)]
    [Display(Name = "More feedback (optional)")]
    public string? Comment { get; set; }

    public string? ModuleName { get; set; }
    public string? PageUrl { get; set; }

    public string? ReturnUrl { get; set; }
}

public class FeedbackDistributionItemVM
{
    public int Rating { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class FeedbackReportVM
{
    public double AverageRating { get; set; }
    public int TotalCount { get; set; }
    public List<FeedbackDistributionItemVM> Distribution { get; set; } = [];
    public List<Feedback> Feedbacks { get; set; } = [];
}

public class HardwareEditVM
{
    public int HardwareId { get; set; }
    [Required, StringLength(200), Display(Name = "Name")]
    public string? Name { get; set; }
    [Required, Display(Name = "Category")]
    public int CategoryId { get; set; }
    [Required, Range(0, double.MaxValue), Display(Name = "Price")]
    public decimal Price { get; set; }
    [StringLength(2000), Display(Name = "Detail")]
    public string? Detail { get; set; }
    [StringLength(100), Display(Name = "Brand")]
    public string? Brand { get; set; }
    [Display(Name = "Display image (required for new)")]
    public IFormFile? PictureFile { get; set; }
    [Display(Name = "Clean image for build diagram (required for new)")]
    public IFormFile? CleanPictureFile { get; set; }
    public string? CurrentPicture { get; set; }
    public string? CurrentCleanPicture { get; set; }
    [StringLength(50), Display(Name = "Socket (CPU/Motherboard)")]
    public string? Socket { get; set; }
    [StringLength(20), Display(Name = "DDR type (Motherboard/RAM)")]
    public string? DdrType { get; set; }
    [StringLength(50), Display(Name = "Form factor (Motherboard/Case)")]
    public string? FormFactor { get; set; }
    [Display(Name = "Special (1-10)")]
    public int? Special { get; set; }
    [Display(Name = "RAM slots (Motherboard)")]
    public int? RamSlots { get; set; }
    [Display(Name = "Release year")]
    public int? ReleaseYear { get; set; }
    [Display(Name = "Rating (1-5)")]
    public int? Rating { get; set; }
    [Display(Name = "Performance score (0-100)")]
    public int? PerformanceScore { get; set; }
    [Display(Name = "Power (W) (CPU/GPU/PSU)")]
    public int? PowerConsumption { get; set; }
    [Display(Name = "Gaming score (1-10)")]
    public int? GamingScore { get; set; }
    [Display(Name = "Office score (1-10)")]
    public int? OfficeScore { get; set; }
    [Display(Name = "Editing score (1-10)")]
    public int? EditingScore { get; set; }
    [StringLength(500), Display(Name = "Reference URL")]
    public string? Reference { get; set; }
}
