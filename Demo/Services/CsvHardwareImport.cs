using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services;

/// <summary>Imports hardware from pc_hardware_dataset_v2.csv with Socket, DdrType for compatibility.</summary>
public class CsvHardwareImport
{
    private static readonly Dictionary<string, string> CategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CY_CPU"] = "CPU",
        ["CY_MB"] = "Motherboard",
        ["CY_GPU"] = "GPU",
        ["CY_RAM"] = "RAM",
        ["CY_STORAGE"] = "Storage",
        ["CY_PSU"] = "Power supply unit",
        ["CY_CASE"] = "PC case"
    };

    public static async Task ImportFromCsvAsync(Models.DB db, string csvPath, ILogger? log = null)
    {
        if (!File.Exists(csvPath)) return;

        var lines = await File.ReadAllLinesAsync(csvPath);
        if (lines.Length < 2) return;

        var headerParts = ParseCsvLine(lines[0]);
        var categoryByName = await db.Categories.ToDictionaryAsync(c => c.Type, StringComparer.OrdinalIgnoreCase);
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < lines.Length; i++)
        {
            var parts = ParseCsvLine(lines[i]);
            if (parts.Count < 4) continue;

            var name = GetByHeader(parts, headerParts, "Name");
            var catId = GetByHeader(parts, headerParts, "CategoryId");
            if (string.IsNullOrEmpty(name) || !CategoryMap.TryGetValue(catId, out var catType) || !categoryByName.TryGetValue(catType, out var cat))
                continue;
            var dedupKey = $"{name}|{cat.CategoryId}";
            if (!seenKeys.Add(dedupKey))
                continue;

            var priceStr = GetByHeader(parts, headerParts, "Price");
            if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price)) continue;

            var detail = GetByHeader(parts, headerParts, "Detail");
            var brand = GetByHeader(parts, headerParts, "Brand");
            var picture = GetByHeader(parts, headerParts, "Picture");
            var socket = GetByHeader(parts, headerParts, "Socket");
            var ddrType = ParseDdrType(detail, name, catType);
            var formFactor = ParseFormFactor(detail, catType);
            var specialStr = GetByHeader(parts, headerParts, "Special");
            int? special = int.TryParse(specialStr, out var s) && s >= 1 && s <= 10 ? s : null;

            var ramSlots = catType == "Motherboard" ? 4 : (int?)null;

            var existing = await db.Hardware.FirstOrDefaultAsync(h => h.Name == name && h.CategoryId == cat.CategoryId);
            if (existing != null)
            {
                existing.Price = price;
                existing.Detail = detail;
                existing.Brand = brand;
                existing.Picture = string.IsNullOrEmpty(picture) ? null : picture;
                existing.Socket = string.IsNullOrEmpty(socket) ? null : socket;
                existing.DdrType = ddrType;
                existing.FormFactor = formFactor;
                existing.Special = special;
                if (ramSlots.HasValue) existing.RamSlots = ramSlots;
            }
            else
            {
                db.Hardware.Add(new Models.Hardware
                {
                    Name = name,
                    CategoryId = cat.CategoryId,
                    Price = price,
                    Detail = detail,
                    Brand = brand,
                    Picture = string.IsNullOrEmpty(picture) ? null : picture,
                    Socket = string.IsNullOrEmpty(socket) ? null : socket,
                    DdrType = ddrType,
                    FormFactor = formFactor,
                    Special = special,
                    RamSlots = ramSlots
                });
            }
        }

        await db.SaveChangesAsync();
        log?.LogInformation("CSV hardware import completed from {Path}", csvPath);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = "";
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"') inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.Trim().Trim('"'));
                current = "";
            }
            else current += c;
        }
        result.Add(current.Trim().Trim('"'));
        return result;
    }

    private static string GetByHeader(List<string> parts, List<string> headers, string headerName)
    {
        var idx = headers.FindIndex(h => string.Equals(h.Trim(), headerName, StringComparison.OrdinalIgnoreCase));
        return idx >= 0 && idx < parts.Count ? parts[idx] : "";
    }

    private static string? ParseDdrType(string detail, string name, string catType)
    {
        var text = (detail + " " + name).ToUpperInvariant();
        if (text.Contains("DDR5")) return "DDR5";
        if (text.Contains("DDR4")) return "DDR4";
        return null;
    }

    private static string? ParseFormFactor(string detail, string catType)
    {
        var text = (detail ?? "").ToUpperInvariant();
        if (text.Contains("MICRO-ATX") || text.Contains("M-ATX") || text.Contains("MICRO ATX")) return "Micro-ATX";
        if (text.Contains("ATX")) return "ATX";
        return null;
    }
}
