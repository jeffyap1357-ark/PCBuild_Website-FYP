using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Models;

#nullable disable warnings

public class DB : DbContext
{
    public DB(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<UserGuideProgress>().HasKey(x => new { x.UserId, x.GuideId });
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Build> Builds { get; set; }
    public DbSet<BuildDetail> BuildDetails { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Hardware> Hardware { get; set; }
    public DbSet<Guide> Guides { get; set; }
    public DbSet<UserGuideProgress> UserGuideProgresses { get; set; }
    public DbSet<Community> Communities { get; set; }
    public DbSet<CommunityComment> CommunityComments { get; set; }
    public DbSet<CommunityRating> CommunityRatings { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<QuizAttemptDetail> QuizAttemptDetails { get; set; }
}

// User: PC Build website users
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; }

    [Required, MaxLength(256)]
    public string Password { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; }

    public List<Build> Builds { get; set; } = [];
    public List<UserGuideProgress> GuideProgresses { get; set; } = [];
}

// Build: PC builds created by users
public class Build
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BuildId { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(200)]
    public string BuildName { get; set; }

    [Precision(10, 2)]
    public decimal TotalPrice { get; set; }

    public bool IsShared { get; set; }
    public DateTime? SharedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public List<BuildDetail> BuildDetails { get; set; } = [];
    public Community Community { get; set; }
}

// BuildDetail: one hardware item in a build
public class BuildDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BuildDetailId { get; set; }

    public int BuildId { get; set; }
    public int HardwareId { get; set; }
    public int Quantity { get; set; } = 1;

    [ForeignKey(nameof(BuildId))]
    public Build Build { get; set; }

    [ForeignKey(nameof(HardwareId))]
    public Hardware Hardware { get; set; }
}

// Category: hardware types (CPU, Motherboard, GPU, etc.)
public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CategoryId { get; set; }

    [Required, MaxLength(100)]
    public string Type { get; set; }

    public List<Hardware> Hardware { get; set; } = [];
}

// Hardware: individual components
public class Hardware
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int HardwareId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; }

    public int CategoryId { get; set; }

    [Precision(10, 2)]
    public decimal Price { get; set; }

    [MaxLength(2000)]
    public string? Detail { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(200)]
    public string? Picture { get; set; }

    [MaxLength(50)]
    public string? Socket { get; set; }

    [MaxLength(20)]
    public string? DdrType { get; set; }

    [MaxLength(50)]
    public string? FormFactor { get; set; }

    public int? Special { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }
}

// Guide: learning materials
[Table("Guide", Schema = "dbo")]
public class Guide
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GuideId { get; set; }

    [Required, MaxLength(200)]
    public string GuideName { get; set; }

    [MaxLength(500)]
    public string? GuideDesc { get; set; }

    public string Content { get; set; } = "";

    public int SortOrder { get; set; }
}

// UserGuideProgress: user has read this guide (for color change)
public class UserGuideProgress
{
    public int UserId { get; set; }
    public int GuideId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    [ForeignKey(nameof(GuideId))]
    public Guide Guide { get; set; }
}

// Community: shared build (one per build when user shares)
[Table("Community", Schema = "dbo")]
public class Community
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CommunityId { get; set; }

    public int BuildId { get; set; }
    public int UserId { get; set; }

    [Required, MaxLength(200)]
    public string BuildName { get; set; }

    [Precision(10, 2)]
    public decimal TotalPrice { get; set; }

    public DateTime SharedAt { get; set; }

    [Precision(3, 2)]
    public decimal? AverageRating { get; set; }

    public int RatingCount { get; set; }

    [MaxLength(2000)]
    public string? AuthorComment { get; set; }

    [ForeignKey(nameof(BuildId))]
    public Build Build { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public List<CommunityComment> CommunityComments { get; set; } = [];
    public List<CommunityRating> CommunityRatings { get; set; } = [];
}

// CommunityComment: user comment on a shared build
[Table("CommunityComments", Schema = "dbo")]
public class CommunityComment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CommunityCommentId { get; set; }

    public int CommunityId { get; set; }
    public int UserId { get; set; }

    [Required, MaxLength(1000)]
    public string CommentText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CommunityId))]
    public Community Community { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}

// CommunityRating: user rating (1–5) on a shared build
[Table("CommunityRatings", Schema = "dbo")]
public class CommunityRating
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CommunityRatingId { get; set; }

    public int CommunityId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CommunityId))]
    public Community Community { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}

// Quiz: PC build knowledge test questions (MCQ with options A–D)
public class QuizQuestion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int QuizQuestionId { get; set; }

    [Required, MaxLength(500)]
    public string QuestionText { get; set; }

    [Required, MaxLength(200)]
    public string OptionA { get; set; }

    [Required, MaxLength(200)]
    public string OptionB { get; set; }

    [Required, MaxLength(200)]
    public string OptionC { get; set; }

    [Required, MaxLength(200)]
    public string OptionD { get; set; }

    /// <summary>A, B, C, or D.</summary>
    [Required, MaxLength(1)]
    public string CorrectOption { get; set; }

    public bool IsActive { get; set; } = true;
}

// QuizAttempt: one completed test by a user
public class QuizAttempt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int QuizAttemptId { get; set; }

    public int UserId { get; set; }

    public DateTime TakenAt { get; set; } = DateTime.UtcNow;

    public int TotalQuestions { get; set; }

    public int CorrectCount { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public List<QuizAttemptDetail> Details { get; set; } = [];
}

// QuizAttemptDetail: per-question answer for an attempt
public class QuizAttemptDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int QuizAttemptDetailId { get; set; }

    public int QuizAttemptId { get; set; }

    public int QuizQuestionId { get; set; }

    [MaxLength(1)]
    public string? SelectedOption { get; set; }

    public bool IsCorrect { get; set; }

    [ForeignKey(nameof(QuizAttemptId))]
    public QuizAttempt Attempt { get; set; }

    [ForeignKey(nameof(QuizQuestionId))]
    public QuizQuestion Question { get; set; }
}
