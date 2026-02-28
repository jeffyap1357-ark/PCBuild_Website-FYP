using Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services;

public record RecommendResult(
    List<Hardware> Parts,
    decimal TotalPrice,
    bool Success,
    List<string> Warnings
);

public class PcRecommendService
{
    private readonly Models.DB _db;

    public PcRecommendService(Models.DB db) => _db = db;

    public async Task<RecommendResult> RecommendAsync(decimal budget, int targetTier)
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

        RecommendResult? best = null;
        foreach (var mb in mbList)
        {
            var socket = mb.Socket ?? "";
            var ddrType = mb.DdrType ?? "";
            var mbForm = mb.FormFactor ?? "ATX";

            var cpus = cpuCat.Where(c => string.IsNullOrEmpty(socket) || c.Socket == socket).OrderByDescending(c => c.Special ?? 0).ToList();
            var rams = ramCat.Where(r => string.IsNullOrEmpty(ddrType) || r.DdrType == ddrType).OrderByDescending(r => r.Special ?? 0).ToList();
            var cases = caseCat.Where(c => mbForm.Contains("Micro") ? (c.FormFactor?.Contains("Micro") == true || c.FormFactor == null) : true).ToList();

            if (!cpus.Any() || !rams.Any())
                continue;

            var cpu = cpus.FirstOrDefault(c => (c.Special ?? 0) >= targetTier - 1) ?? cpus.FirstOrDefault();
            var ram = rams.FirstOrDefault(r => (r.Special ?? 0) >= targetTier - 1) ?? rams.FirstOrDefault();
            var gpu = gpuCat.OrderByDescending(g => g.Special ?? 0).FirstOrDefault();
            var storage = storageCat.OrderByDescending(s => s.Special ?? 0).FirstOrDefault();
            var psu = psuCat.OrderByDescending(p => p.Special ?? 0).FirstOrDefault();
            var pcCase = cases.OrderByDescending(c => c.Special ?? 0).FirstOrDefault();

            if (cpu == null || ram == null || gpu == null || storage == null || psu == null || pcCase == null)
                continue;

            var parts = new List<Hardware> { mb, cpu, ram, gpu, storage, psu, pcCase };
            var total = parts.Sum(p => p.Price);
            if (total > budget)
            {
                var remaining = budget - mb.Price - cpu.Price - ram.Price;
                if (remaining < 500) continue;
                gpu = gpuCat.Where(g => g.Price <= remaining).OrderByDescending(g => g.Special ?? 0).FirstOrDefault();
                if (gpu == null) continue;
                remaining -= gpu.Price;
                storage = storageCat.Where(s => s.Price <= remaining).OrderByDescending(s => s.Special ?? 0).FirstOrDefault();
                if (storage == null) continue;
                remaining -= storage.Price;
                psu = psuCat.Where(p => p.Price <= remaining).OrderByDescending(p => p.Special ?? 0).FirstOrDefault();
                if (psu == null) continue;
                remaining -= psu.Price;
                pcCase = caseCat.Where(c => c.Price <= Math.Max(remaining, 0)).OrderByDescending(c => c.Special ?? 0).FirstOrDefault();
                if (pcCase == null) continue;
                parts = [mb, cpu, ram, gpu, storage, psu, pcCase];
                total = parts.Sum(p => p.Price);
            }

            var score = parts.Sum(p => p.Special ?? 0);
            if (best == null || score > best.Parts.Sum(p => p.Special ?? 0))
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
}
