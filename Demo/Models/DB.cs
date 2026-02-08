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
        mb.Entity<UserInterest>().HasKey(x => new { x.UserId, x.MaterialId });
        mb.Entity<UserProgress>().HasKey(x => new { x.UserId, x.MaterialId });
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Build> Builds { get; set; }
    public DbSet<BuildDetail> BuildDetails { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Hardware> Hardware { get; set; }
    public DbSet<LearningMaterial> LearningMaterials { get; set; }
    public DbSet<UserInterest> UserInterests { get; set; }
    public DbSet<UserProgress> UserProgresses { get; set; }
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
    public List<UserInterest> UserInterests { get; set; } = [];
    public List<UserProgress> UserProgresses { get; set; } = [];
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

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public List<BuildDetail> BuildDetails { get; set; } = [];
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

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }
}

// Learning & Guide
public class LearningMaterial
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaterialId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public string Content { get; set; } = "";

    public int SortOrder { get; set; }
}

// User marked material as interested
public class UserInterest
{
    public int UserId { get; set; }
    public int MaterialId { get; set; }
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    [ForeignKey(nameof(MaterialId))]
    public LearningMaterial Material { get; set; }
}

// User viewed/opened material (for progress indicator)
public class UserProgress
{
    public int UserId { get; set; }
    public int MaterialId { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    [ForeignKey(nameof(MaterialId))]
    public LearningMaterial Material { get; set; }
}
