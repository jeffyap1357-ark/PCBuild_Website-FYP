using Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services;

public record RecommendResult(
    List<Hardware> Parts,
    decimal TotalPrice,
    bool Success,
    List<string> Warnings
);

/// <summary>Weights for performance, value-for-money, power efficiency, and purpose-specific scores (0–1 each, sum = 1).</summary>
internal record ScoringWeights(double Performance, double Value, double Power, double PurposeScore);

public class PcRecommendService
{
    private readonly Models.DB _db;

    public PcRecommendService(Models.DB db) => _db = db;

    public async Task<RecommendResult> RecommendAsync(decimal budget, OptimizationType optimizationType = OptimizationType.Balanced)
    {
        var warnings = new List<string>();
        var cats = await _db.Categories.Include(c => c.Hardware).ToDictionaryAsync(c => c.Type, StringComparer.OrdinalIgnoreCase);

        if (!cats.TryGetValue("Motherboard", out var mbCat) || !mbCat.Hardware.Any())
            return new RecommendResult([], 0, false, ["No motherboard available in database."]);

        var mbList = mbCat.Hardware.Where(h => h.Socket != null).OrderByDescending(h => h.PerformanceScore ?? h.Special ?? 0).ToList();
        var cpuCat = cats.GetValueOrDefault("CPU")?.Hardware.ToList() ?? [];
        var ramCat = cats.GetValueOrDefault("RAM")?.Hardware.ToList() ?? [];
        var gpuCat = cats.GetValueOrDefault("GPU")?.Hardware.ToList() ?? [];
        var storageCat = cats.GetValueOrDefault("Storage")?.Hardware.ToList() ?? [];
        var psuCat = cats.GetValueOrDefault("Power supply unit")?.Hardware.ToList() ?? [];
        var caseCat = cats.GetValueOrDefault("PC case")?.Hardware.ToList() ?? [];

        var minBudget = 4000m;
        if (budget < minBudget)
            warnings.Add($"Budget (RM {budget:N0}) may be too low. Minimum recommended: RM {minBudget:N0} for a complete build.");

        var weights = GetWeights(optimizationType);

        RecommendResult? best = null;
        foreach (var mb in mbList)
        {
            var socket = mb.Socket ?? "";
            var ddrType = mb.DdrType ?? "";
            var mbForm = mb.FormFactor ?? "ATX";

            var cpus = cpuCat.Where(c => string.IsNullOrEmpty(socket) || c.Socket == socket).ToList();
            var rams = ramCat.Where(r => string.IsNullOrEmpty(ddrType) || r.DdrType == ddrType).ToList();
            var cases = caseCat.Where(c => mbForm.Contains("Micro") ? (c.FormFactor?.Contains("Micro") == true || c.FormFactor == null) : true).ToList();

            if (!cpus.Any() || !rams.Any())
                continue;

            var gpu = PickBest(gpuCat, "GPU", null, weights, optimizationType);
            var cpu = PickBest(cpus, "CPU", gpu, weights, optimizationType);
            var ram = PickBest(rams, "RAM", null, weights, optimizationType);
            var storage = PickBest(storageCat, "Storage", null, weights, optimizationType);
            var psu = PickBest(psuCat, "Power supply unit", null, weights, optimizationType);
            var pcCase = PickBest(cases, "PC case", null, weights, optimizationType);

            if (cpu == null || ram == null || gpu == null || storage == null || psu == null || pcCase == null)
                continue;

            var parts = new List<Hardware> { mb, cpu, ram, gpu, storage, psu, pcCase };
            var total = parts.Sum(p => p.Price);
            if (total > budget)
            {
                var remaining = budget - mb.Price - cpu.Price - ram.Price;
                if (remaining < 500) continue;
                gpu = PickBest(gpuCat.Where(g => g.Price <= remaining).ToList(), "GPU", null, weights, optimizationType);
                if (gpu == null) continue;
                remaining -= gpu.Price;
                storage = PickBest(storageCat.Where(s => s.Price <= remaining).ToList(), "Storage", null, weights, optimizationType);
                if (storage == null) continue;
                remaining -= storage.Price;
                psu = PickBest(psuCat.Where(p => p.Price <= remaining).ToList(), "Power supply unit", null, weights, optimizationType);
                if (psu == null) continue;
                remaining -= psu.Price;
                pcCase = PickBest(cases.Where(c => c.Price <= Math.Max(remaining, 0)).ToList(), "PC case", null, weights, optimizationType);
                if (pcCase == null) continue;
                parts = [mb, cpu, ram, gpu, storage, psu, pcCase];
                total = parts.Sum(p => p.Price);
            }

            var score = ComputeBuildScore(parts, weights, optimizationType);
            var bestScore = best != null ? ComputeBuildScore(best.Parts, weights, optimizationType) : double.NegativeInfinity;
            if (best == null || score > bestScore)
            {
                var resultWarnings = new List<string>(warnings);
                var psuWatt = ParsePsuWattage(psu.Detail ?? psu.Name);
                var estPower = EstimatePower(cpu, gpu);
                if (psuWatt > 0 && estPower > psuWatt)
                    resultWarnings.Add($"⚠️ Unstable: Estimated power draw (~{estPower}W) may exceed PSU capacity ({psuWatt}W). Consider a higher wattage PSU.");
                if (total > budget)
                    resultWarnings.Add($"⚠️ Over budget: Total RM {total:N0} exceeds your budget of RM {budget:N0}.");
                best = new RecommendResult(parts, total, true, resultWarnings);
            }
        }

        if (best == null)
            return new RecommendResult([], 0, false, ["Could not build a compatible PC within budget. Try increasing your budget or check if hardware data is loaded."]);

        return best;
    }

    private static int ParsePsuWattage(string text)
    {
        var m = System.Text.RegularExpressions.Regex.Match(text, @"(\d{3,4})\s*W", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return m.Success && int.TryParse(m.Groups[1].Value, out var w) ? w : 0;
    }

    private static int EstimatePower(Hardware cpu, Hardware gpu)
    {
        if (cpu.PowerConsumption.HasValue && gpu.PowerConsumption.HasValue)
            return cpu.PowerConsumption.Value + gpu.PowerConsumption.Value + 80;
        var cpuW = (cpu.Special ?? 5) switch { >= 9 => 200, >= 8 => 150, >= 7 => 100, _ => 65 };
        var gpuW = (gpu.Special ?? 5) switch { >= 9 => 350, >= 8 => 200, >= 7 => 130, _ => 100 };
        return cpuW + gpuW + 80;
    }

    private static ScoringWeights GetWeights(OptimizationType type) => type switch
    {
        OptimizationType.Performance => new ScoringWeights(0.70, 0.15, 0.15, 0),
        OptimizationType.Budget => new ScoringWeights(0.20, 0.70, 0.10, 0),
        OptimizationType.Balanced => new ScoringWeights(0.50, 0.45, 0.05, 0),
        OptimizationType.Gaming => new ScoringWeights(0.20, 0.20, 0.10, 0.50),
        OptimizationType.Office => new ScoringWeights(0.15, 0.35, 0.10, 0.40),
        OptimizationType.Editing => new ScoringWeights(0.25, 0.25, 0.10, 0.40),
        _ => new ScoringWeights(0.50, 0.45, 0.05, 0)
    };

    private static double GetPurposeScore(Hardware h, OptimizationType type)
    {
        return type switch
        {
            OptimizationType.Gaming => (h.GamingScore ?? h.Special ?? 0) / 10.0,
            OptimizationType.Office => (h.OfficeScore ?? h.Special ?? 0) / 10.0,
            OptimizationType.Editing => (h.EditingScore ?? h.Special ?? 0) / 10.0,
            _ => (h.Special ?? 0) / 10.0
        };
    }

    private static double GetEffectiveScore(Hardware h, OptimizationType? type)
    {
        if (type.HasValue && (type == OptimizationType.Gaming || type == OptimizationType.Office || type == OptimizationType.Editing))
            return GetPurposeScore(h, type.Value);
        return (h.PerformanceScore ?? (h.Special ?? 0) * 10) / 100.0;
    }

    private static double ComponentScore(Hardware h, string categoryType, ScoringWeights w, Hardware? gpuForPower, OptimizationType optType)
    {
        var perf = (h.PerformanceScore ?? (h.Special ?? 0) * 10) / 100.0;
        var value = 50.0 * (h.Special ?? h.PerformanceScore ?? 0) / Math.Max((double)h.Price, 1);
        value = Math.Min(1, value);

        double power = 0.5;
        if (categoryType.Equals("CPU", StringComparison.OrdinalIgnoreCase) && gpuForPower != null)
        {
            var est = EstimatePower(h, gpuForPower);
            power = 1 - Math.Min(est / 600.0, 1);
        }
        else if (categoryType.Equals("GPU", StringComparison.OrdinalIgnoreCase))
        {
            var gpuW = h.PowerConsumption ?? ((h.Special ?? 5) switch { >= 9 => 350, >= 8 => 200, >= 7 => 130, _ => 100 });
            power = 1 - Math.Min(gpuW / 400.0, 1);
        }

        var purposeScore = GetPurposeScore(h, optType);
        var componentMultiplier = (categoryType.Equals("CPU", StringComparison.OrdinalIgnoreCase) ||
                                   categoryType.Equals("GPU", StringComparison.OrdinalIgnoreCase)) ? 1.3 : 1.0;

        return componentMultiplier * (w.Performance * perf + w.Value * value + w.Power * power + w.PurposeScore * purposeScore);
    }

    private static Hardware? PickBest(List<Hardware> items, string categoryType, Hardware? gpuForPower, ScoringWeights weights, OptimizationType optType)
    {
        if (items.Count == 0) return null;
        return items.OrderByDescending(h => ComponentScore(h, categoryType, weights, gpuForPower, optType)).FirstOrDefault();
    }

    private static double ComputeBuildScore(List<Hardware> parts, ScoringWeights weights, OptimizationType optType)
    {
        var cpu = parts.FirstOrDefault(p => p.Category?.Type?.Equals("CPU", StringComparison.OrdinalIgnoreCase) == true);
        var gpu = parts.FirstOrDefault(p => p.Category?.Type?.Equals("GPU", StringComparison.OrdinalIgnoreCase) == true);

        return parts.Sum(p =>
        {
            var cat = p.Category?.Type ?? "";
            return ComponentScore(p, cat, weights, gpu, optType);
        });
    }
}
