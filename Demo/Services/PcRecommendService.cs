using Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services;

public record RecommendResult(
    List<Hardware> Parts,
    decimal TotalPrice,
    bool Success,
    List<string> Warnings
);

/// <summary>Weights for performance, value-for-money, and power efficiency (0–1 each, sum = 1).</summary>
internal record ScoringWeights(double Performance, double Value, double Power);

public class PcRecommendService
{
    private readonly Models.DB _db;

    public PcRecommendService(Models.DB db) => _db = db;

    public async Task<RecommendResult> RecommendAsync(decimal budget, int targetTier, OptimizationType optimizationType = OptimizationType.Balanced)
    {
        var warnings = new List<string>();
        var cats = await _db.Categories.Include(c => c.Hardware).ToDictionaryAsync(c => c.Type, StringComparer.OrdinalIgnoreCase);

        if (!cats.TryGetValue("Motherboard", out var mbCat) || !mbCat.Hardware.Any())
            return new RecommendResult([], 0, false, ["No motherboard available in database."]);

        var mbList = mbCat.Hardware.Where(h => h.Socket != null).OrderByDescending(h => h.Special ?? 0).ToList();
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

            var gpu = PickBest(gpuCat, "GPU", null, weights, targetTier);
            var cpu = PickBest(cpus, "CPU", gpu, weights, targetTier);
            var ram = PickBest(rams, "RAM", null, weights, targetTier);
            var storage = PickBest(storageCat, "Storage", null, weights, targetTier);
            var psu = PickBest(psuCat, "Power supply unit", null, weights, targetTier);
            var pcCase = PickBest(cases, "PC case", null, weights, targetTier);

            if (cpu == null || ram == null || gpu == null || storage == null || psu == null || pcCase == null)
                continue;

            var parts = new List<Hardware> { mb, cpu, ram, gpu, storage, psu, pcCase };
            var total = parts.Sum(p => p.Price);
            if (total > budget)
            {
                var remaining = budget - mb.Price - cpu.Price - ram.Price;
                if (remaining < 500) continue;
                gpu = PickBest(gpuCat.Where(g => g.Price <= remaining).ToList(), "GPU", null, weights, targetTier);
                if (gpu == null) continue;
                remaining -= gpu.Price;
                storage = PickBest(storageCat.Where(s => s.Price <= remaining).ToList(), "Storage", null, weights, targetTier);
                if (storage == null) continue;
                remaining -= storage.Price;
                psu = PickBest(psuCat.Where(p => p.Price <= remaining).ToList(), "Power supply unit", null, weights, targetTier);
                if (psu == null) continue;
                remaining -= psu.Price;
                pcCase = PickBest(cases.Where(c => c.Price <= Math.Max(remaining, 0)).ToList(), "PC case", null, weights, targetTier);
                if (pcCase == null) continue;
                parts = [mb, cpu, ram, gpu, storage, psu, pcCase];
                total = parts.Sum(p => p.Price);
            }

            var score = ComputeBuildScore(parts, weights);
            var bestScore = best != null ? ComputeBuildScore(best.Parts, weights) : double.NegativeInfinity;
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
        var cpuW = (cpu.Special ?? 5) switch { >= 9 => 200, >= 8 => 150, >= 7 => 100, _ => 65 };
        var gpuW = (gpu.Special ?? 5) switch { >= 9 => 350, >= 8 => 200, >= 7 => 130, _ => 100 };
        return cpuW + gpuW + 80;
    }

    private static ScoringWeights GetWeights(OptimizationType type) => type switch
    {
        OptimizationType.Performance => new ScoringWeights(0.70, 0.15, 0.15),
        OptimizationType.Budget => new ScoringWeights(0.20, 0.70, 0.10),
        OptimizationType.Balanced => new ScoringWeights(0.50, 0.45, 0.05),
        _ => new ScoringWeights(0.50, 0.45, 0.05)
    };

    private static double ComponentScore(Hardware h, string categoryType, ScoringWeights w, Hardware? gpuForPower = null)
    {
        var perf = (h.Special ?? 0) / 10.0;
        var value = 50.0 * (h.Special ?? 0) / Math.Max((double)h.Price, 1);
        value = Math.Min(1, value);

        double power = 0.5;
        if (categoryType.Equals("CPU", StringComparison.OrdinalIgnoreCase) && gpuForPower != null)
        {
            var est = EstimatePower(h, gpuForPower);
            power = 1 - Math.Min(est / 600.0, 1);
        }
        else if (categoryType.Equals("GPU", StringComparison.OrdinalIgnoreCase))
        {
            var gpuW = (h.Special ?? 5) switch { >= 9 => 350, >= 8 => 200, >= 7 => 130, _ => 100 };
            power = 1 - Math.Min(gpuW / 400.0, 1);
        }

        var componentMultiplier = (categoryType.Equals("CPU", StringComparison.OrdinalIgnoreCase) ||
                                   categoryType.Equals("GPU", StringComparison.OrdinalIgnoreCase)) ? 1.3 : 1.0;

        return componentMultiplier * (w.Performance * perf + w.Value * value + w.Power * power);
    }

    private static Hardware? PickBest(List<Hardware> items, string categoryType, Hardware? gpuForPower, ScoringWeights weights, int targetTier)
    {
        if (items.Count == 0) return null;

        var meetsTier = items.Where(i => (i.Special ?? 0) >= targetTier - 1).ToList();
        var pool = meetsTier.Count > 0 ? meetsTier : items;
        return pool.OrderByDescending(h => ComponentScore(h, categoryType, weights, gpuForPower)).FirstOrDefault();
    }

    private static double ComputeBuildScore(List<Hardware> parts, ScoringWeights weights)
    {
        var cpu = parts.FirstOrDefault(p => p.Category?.Type?.Equals("CPU", StringComparison.OrdinalIgnoreCase) == true);
        var gpu = parts.FirstOrDefault(p => p.Category?.Type?.Equals("GPU", StringComparison.OrdinalIgnoreCase) == true);

        return parts.Sum(p =>
        {
            var cat = p.Category?.Type ?? "";
            return ComponentScore(p, cat, weights, gpu);
        });
    }
}
