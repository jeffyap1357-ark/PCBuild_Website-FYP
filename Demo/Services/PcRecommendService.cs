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

    /// <summary>Recommend a build. Optionally keep some components locked and vary the rest. Optionally exclude a build so a different combo is returned.</summary>
    /// <param name="lockedByCategory">Category type (e.g. "CPU", "GPU") to HardwareId for components to keep fixed.</param>
    /// <param name="excludePartIds">If set, the best build that equals this set of hardware IDs is skipped so a different combo is returned.</param>
    /// <param name="excludeUnlockedByCategory">If set, candidate builds that keep all unlocked categories identical are skipped.</param>
    public async Task<RecommendResult> RecommendAsync(
        decimal budget,
        OptimizationType optimizationType = OptimizationType.Balanced,
        IReadOnlyDictionary<string, int>? lockedByCategory = null,
        IReadOnlySet<int>? excludePartIds = null,
        IReadOnlyDictionary<string, int>? excludeUnlockedByCategory = null)
    {
        var warnings = new List<string>();
        var cats = await _db.Categories.Include(c => c.Hardware).ToDictionaryAsync(c => c.Type, StringComparer.OrdinalIgnoreCase);

        if (!cats.TryGetValue("Motherboard", out var mbCat) || !mbCat.Hardware.Any())
            return new RecommendResult([], 0, false, ["No motherboard available in database."]);

        Dictionary<string, Hardware>? lockedParts = null;
        if (lockedByCategory != null && lockedByCategory.Count > 0)
        {
            var ids = lockedByCategory.Values.Distinct().ToList();
            var lockedList = await _db.Hardware.Include(h => h.Category)
                .Where(h => ids.Contains(h.HardwareId))
                .ToListAsync();
            lockedParts = lockedList
                .Where(h => h.Category != null && lockedByCategory.TryGetValue(h.Category.Type, out var id) && id == h.HardwareId)
                .ToDictionary(h => h.Category!.Type, h => h, StringComparer.OrdinalIgnoreCase);
        }

        var mbList = mbCat.Hardware.Where(h => h.Socket != null).OrderByDescending(h => h.PerformanceScore ?? h.Special ?? 0).ToList();

        if (lockedParts != null)
        {
            if (lockedParts.TryGetValue("CPU", out var lockedCpu))
                mbList = mbList.Where(m => string.IsNullOrEmpty(lockedCpu.Socket) || m.Socket == lockedCpu.Socket).ToList();
            if (lockedParts.TryGetValue("RAM", out var lockedRam))
                mbList = mbList.Where(m => string.IsNullOrEmpty(lockedRam.DdrType) || m.DdrType == lockedRam.DdrType).ToList();
            if (lockedParts.TryGetValue("Motherboard", out var lockedMb))
                mbList = mbList.Where(m => m.HardwareId == lockedMb.HardwareId).ToList();
        }

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

            Hardware? cpu = lockedParts?.TryGetValue("CPU", out var lc) == true ? lc : PickBest(cpus, "CPU", null, weights, optimizationType);
            if (cpu != null && cpus.All(c => c.HardwareId != cpu.HardwareId))
                cpu = null;
            Hardware? ram = lockedParts?.TryGetValue("RAM", out var lr) == true ? lr : PickBest(rams, "RAM", null, weights, optimizationType);
            if (ram != null && rams.All(r => r.HardwareId != ram.HardwareId))
                ram = null;
            if (!cpus.Any() && cpu == null || !rams.Any() && ram == null)
                continue;
            if (cpu == null && cpus.Any() == false) continue;
            if (ram == null && rams.Any() == false) continue;
            if (cpu == null) cpu = PickBest(cpus, "CPU", null, weights, optimizationType);
            if (ram == null) ram = PickBest(rams, "RAM", null, weights, optimizationType);
            if (cpu == null || ram == null) continue;

            var gpu = lockedParts?.TryGetValue("GPU", out var lg) == true ? lg : PickBest(gpuCat, "GPU", null, weights, optimizationType);
            var storage = lockedParts?.TryGetValue("Storage", out var ls) == true ? ls : PickBest(storageCat, "Storage", null, weights, optimizationType);
            var psu = lockedParts?.TryGetValue("Power supply unit", out var lp) == true ? lp : PickBest(psuCat, "Power supply unit", null, weights, optimizationType);
            var pcCase = lockedParts?.TryGetValue("PC case", out var lcase) == true ? lcase : PickBest(cases, "PC case", null, weights, optimizationType);
            if (pcCase != null && cases.Any() && cases.All(c => c.HardwareId != pcCase.HardwareId))
                pcCase = PickBest(cases, "PC case", null, weights, optimizationType);

            if (gpu == null || storage == null || psu == null || pcCase == null)
                continue;

            var parts = new List<Hardware> { mb, cpu, ram, gpu, storage, psu, pcCase };
            var total = parts.Sum(p => p.Price);
            if (total > budget)
            {
                var remaining = budget - mb.Price - cpu.Price - ram.Price;
                if (remaining < 500) continue;
                if (lockedParts?.ContainsKey("GPU") != true)
                    gpu = PickBest(gpuCat.Where(g => g.Price <= remaining).ToList(), "GPU", null, weights, optimizationType);
                else if (gpu.Price > remaining) continue;
                if (gpu == null) continue;
                remaining -= gpu.Price;
                if (lockedParts?.ContainsKey("Storage") != true)
                    storage = PickBest(storageCat.Where(s => s.Price <= remaining).ToList(), "Storage", null, weights, optimizationType);
                else if (storage.Price > remaining) continue;
                if (storage == null) continue;
                remaining -= storage.Price;
                if (lockedParts?.ContainsKey("Power supply unit") != true)
                    psu = PickBest(psuCat.Where(p => p.Price <= remaining).ToList(), "Power supply unit", null, weights, optimizationType);
                else if (psu.Price > remaining) continue;
                if (psu == null) continue;
                remaining -= psu.Price;
                if (lockedParts?.ContainsKey("PC case") != true)
                    pcCase = PickBest(cases.Where(c => c.Price <= Math.Max(remaining, 0)).ToList(), "PC case", null, weights, optimizationType);
                if (pcCase == null) continue;
                parts = [mb, cpu, ram, gpu, storage, psu, pcCase];
                total = parts.Sum(p => p.Price);
            }

            var score = ComputeBuildScore(parts, weights, optimizationType);
            var bestScore = best != null ? ComputeBuildScore(best.Parts, weights, optimizationType) : double.NegativeInfinity;
            if (best == null || score > bestScore)
            {
                if (excludePartIds != null && excludePartIds.Count > 0)
                {
                    var partIds = parts.Select(p => p.HardwareId).ToHashSet();
                    if (partIds.SetEquals(excludePartIds))
                        continue;
                }
                if (excludeUnlockedByCategory != null && excludeUnlockedByCategory.Count > 0)
                {
                    var partByCategory = parts
                        .Where(p => p.Category?.Type != null)
                        .ToDictionary(p => p.Category!.Type, p => p.HardwareId, StringComparer.OrdinalIgnoreCase);
                    var sameUnlocked = excludeUnlockedByCategory.All(x =>
                        partByCategory.TryGetValue(x.Key, out var id) && id == x.Value);
                    if (sameUnlocked)
                        continue;
                }
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
        {
            if ((excludePartIds != null && excludePartIds.Count > 0) || (excludeUnlockedByCategory != null && excludeUnlockedByCategory.Count > 0))
                return new RecommendResult([], 0, false, ["No different combination found. Try unlocking some components or increasing your budget."]);
            return new RecommendResult([], 0, false, ["Could not build a compatible PC within budget. Try increasing your budget or check if hardware data is loaded."]);
        }

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
