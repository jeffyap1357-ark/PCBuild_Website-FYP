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
    [Required, Range(1000, 50000), Display(Name = "Budget (RM)")]
    public decimal Budget { get; set; } = 5000;

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
}

public class QuizTakeVM
{
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
