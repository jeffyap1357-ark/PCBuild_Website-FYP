using Demo.Models;

namespace Demo.Services;

public sealed class BuildAssessment
{
    public string Rating { get; set; } = "";
    public string Comment { get; set; } = "";
    public string Strengths { get; set; } = "";
    public string Weaknesses { get; set; } = "";
}

public static class BuildAssessmentService
{
    public static int ComputeScore(IEnumerable<Hardware> parts)
    {
        if (parts == null) return 0;

        var values = parts
            .Select(p => p.PerformanceScore ?? p.Special ?? 0)
            .Where(v => v > 0)
            .ToList();

        if (values.Count == 0) return 0;
        return (int)Math.Round(values.Average(), MidpointRounding.AwayFromZero);
    }

    public static BuildAssessment GetAssessment(int score, IEnumerable<Hardware> parts)
    {
        var categoryNames = (parts ?? [])
            .Where(p => p?.Category?.Type != null)
            .Select(p => p.Category!.Type)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rating = score switch
        {
            >= 85 => "Excellent",
            >= 70 => "Good",
            >= 55 => "Fair",
            > 0 => "Entry-level",
            _ => "Insufficient data"
        };

        var strengths = score >= 70
            ? "Strong overall performance for most workloads."
            : "Balanced basic setup.";

        if (categoryNames.Contains("GPU", StringComparer.OrdinalIgnoreCase))
            strengths = "Includes a discrete GPU and supports heavier visual workloads.";

        var weaknesses = score switch
        {
            >= 85 => "Higher cost and power draw.",
            >= 70 => "May need upgrades for top-end workloads.",
            >= 55 => "Limited headroom for demanding games and editing tasks.",
            > 0 => "Best for light usage; future upgrades recommended.",
            _ => "Some component scores are missing."
        };

        var comment = $"This build is rated {rating.ToLowerInvariant()} with score {score}.";

        return new BuildAssessment
        {
            Rating = rating,
            Comment = comment,
            Strengths = strengths,
            Weaknesses = weaknesses
        };
    }
}
