using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services;

/// <summary>Imports hardware from CSV with full attributes: Socket, DdrType, FormFactor, ReleaseYear, Rating, PerformanceScore, PowerConsumption, GamingScore, OfficeScore, EditingScore.</summary>
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
            var catIdStr = GetByHeader(parts, headerParts, "CategoryId");
            if (string.IsNullOrEmpty(name)) continue;

            Models.Category? cat = null;
            if (int.TryParse(catIdStr, out var catIdNum) && catIdNum >= 1 && catIdNum <= 8)
            {
                cat = await db.Categories.FindAsync(catIdNum);
            }
            else if (CategoryMap.TryGetValue(catIdStr, out var mappedCatType))
            {
                categoryByName.TryGetValue(mappedCatType, out cat);
            }
            if (cat == null) continue;
            var catType = cat.Type;
            var dedupKey = $"{name}|{cat.CategoryId}";
            if (!seenKeys.Add(dedupKey))
                continue;

            var priceStr = GetByHeader(parts, headerParts, "Price");
            if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price)) continue;

            var detail = GetByHeader(parts, headerParts, "Detail");
            var brand = GetByHeader(parts, headerParts, "Brand");
            var picture = GetByHeader(parts, headerParts, "Picture");
            var socket = GetByHeader(parts, headerParts, "Socket");
            var ddrType = GetByHeader(parts, headerParts, "DdrType");
            if (string.IsNullOrEmpty(ddrType)) ddrType = ParseDdrType(detail, name, catType);
            var formFactor = GetByHeader(parts, headerParts, "FormFactor");
            if (string.IsNullOrEmpty(formFactor)) formFactor = ParseFormFactor(detail, catType);
            var specialStr = GetByHeader(parts, headerParts, "Special");
            int? special = int.TryParse(specialStr, out var s) && s >= 1 && s <= 10 ? s : null;

            int? ramSlots = null;
            if (catType == "Motherboard")
            {
                var rs = GetByHeader(parts, headerParts, "RamSlots");
                ramSlots = int.TryParse(rs, out var r) && r >= 2 ? r : 4;
            }

            var releaseYear = ParseInt(GetByHeader(parts, headerParts, "ReleaseYear"), 1990, 2030);
            var rating = ParseInt(GetByHeader(parts, headerParts, "Rating"), 1, 5);
            var performanceScore = ParseInt(GetByHeader(parts, headerParts, "PerformanceScore"), 0, 100);
            var powerConsumption = ParseInt(GetByHeader(parts, headerParts, "PowerConsumption"), 0, 1000);
            var gamingScore = ParseInt(GetByHeader(parts, headerParts, "GamingScore"), 1, 10);
            var officeScore = ParseInt(GetByHeader(parts, headerParts, "OfficeScore"), 1, 10);
            var editingScore = ParseInt(GetByHeader(parts, headerParts, "EditingScore"), 1, 10);
            var reference = GetByHeader(parts, headerParts, "Reference");
            if (string.IsNullOrWhiteSpace(reference)) reference = null;

            var existing = await db.Components.FirstOrDefaultAsync(h => h.Name == name && h.CategoryId == cat.CategoryId);
            if (existing != null)
            {
                // Do NOT overwrite existing records - user edits in DB (Socket, etc.) must persist.
                // CSV import only adds new hardware; existing rows keep their current values.
            }
            else
            {
                db.Components.Add(new Models.Component
                {
                    Name = name,
                    CategoryId = cat.CategoryId,
                    Price = price,
                    Detail = detail,
                    Brand = brand,
                    Picture = string.IsNullOrEmpty(picture) ? null : picture,
                    Socket = string.IsNullOrEmpty(socket) ? null : socket,
                    DdrType = string.IsNullOrEmpty(ddrType) ? null : ddrType,
                    FormFactor = string.IsNullOrEmpty(formFactor) ? null : formFactor,
                    Special = special,
                    RamSlots = ramSlots,
                    ReleaseYear = releaseYear,
                    Rating = rating,
                    PerformanceScore = performanceScore,
                    PowerConsumption = powerConsumption,
                    GamingScore = gamingScore,
                    OfficeScore = officeScore,
                    EditingScore = editingScore,
                    Reference = reference
                });
            }
        }

        await db.SaveChangesAsync();
        log?.LogInformation("CSV hardware import completed from {Path}", csvPath);
    }

    private static int? ParseInt(string s, int min, int max)
    {
        if (!int.TryParse(s, out var v)) return null;
        return v >= min && v <= max ? v : null;
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
