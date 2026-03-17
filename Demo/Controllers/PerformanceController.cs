using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

[Authorize]
public class PerformanceController : Controller
{
    private readonly DB db;

    public PerformanceController(DB db)
    {
        this.db = db;
    }

    private int? GetCurrentUserId()
    {
        var name = User.Identity?.Name;
        if (string.IsNullOrEmpty(name)) return null;
        return db.Users.Where(u => u.Username == name).Select(u => u.UserId).FirstOrDefault();
    }

    public IActionResult Index()
    {
        var vm = new PerformanceBuildVM();
        vm.Categories = BuildCategoryOptions(vm.Budget, null);
        return View(vm);
    }

    [HttpPost]
    public IActionResult Index(PerformanceBuildVM vm)
    {
        var selected = vm.Categories ?? [];
        vm.Categories = BuildCategoryOptions(vm.Budget, selected);

        var selectedIds = vm.Categories
            .Where(c => c.SelectedHardwareId.HasValue && c.SelectedHardwareId.Value > 0)
            .Select(c => c.SelectedHardwareId!.Value)
            .ToList();

        if (!selectedIds.Any())
        {
            ModelState.AddModelError("", "Please select at least one component.");
        }

        var parts = db.Hardware
            .Include(h => h.Category)
            .Where(h => selectedIds.Contains(h.HardwareId))
            .ToList();

        // Basic compatibility checks: CPU-Motherboard socket, Motherboard-RAM DDR, Motherboard-Case form factor
        var cpu = parts.FirstOrDefault(p => p.Category.Type.Equals("CPU", StringComparison.OrdinalIgnoreCase));
        var mb = parts.FirstOrDefault(p => p.Category.Type.Equals("Motherboard", StringComparison.OrdinalIgnoreCase));
        var ram = parts.FirstOrDefault(p => p.Category.Type.Equals("RAM", StringComparison.OrdinalIgnoreCase));
        var pcCase = parts.FirstOrDefault(p => p.Category.Type.Equals("PC case", StringComparison.OrdinalIgnoreCase));

        if (cpu != null && mb != null)
        {
            if (!string.IsNullOrEmpty(cpu.Socket) && !string.IsNullOrEmpty(mb.Socket) && !string.Equals(cpu.Socket, mb.Socket, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Selected CPU is not compatible with selected motherboard (socket mismatch).");
            }
        }

        if (mb != null && ram != null)
        {
            if (!string.IsNullOrEmpty(mb.DdrType) && !string.IsNullOrEmpty(ram.DdrType) && !string.Equals(mb.DdrType, ram.DdrType, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Selected RAM is not compatible with selected motherboard (DDR type mismatch).");
            }
        }

        if (mb != null && pcCase != null)
        {
            var mbForm = mb.FormFactor ?? "";
            var caseForm = pcCase.FormFactor ?? "";
            if (!string.IsNullOrEmpty(mbForm) && !string.IsNullOrEmpty(caseForm) && !caseForm.Contains(mbForm, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Selected case may not fit the selected motherboard (form factor mismatch).");
            }
        }

        var totalPrice = parts.Sum(p => p.Price);

        if (!ModelState.IsValid)
        {
            vm.HasResult = false;
            return View(vm);
        }

        // Calculate performance score: sum of Special (null treated as 5)
        var totalScore = parts.Sum(p => p.Special ?? 5);

        vm.TotalPrice = totalPrice;
        vm.TotalScore = totalScore;
        vm.HasResult = true;

        ViewBag.SelectedParts = parts;

        // Automatically save a report for logged-in user
        var userId = GetCurrentUserId();
        if (userId != null)
        {
            var report = new PerformanceReport
            {
                UserId = userId.Value,
                ReportName = $"Performance Test {DateTime.Now:yyyy-MM-dd HH:mm}",
                Budget = vm.Budget,
                TotalPrice = totalPrice,
                TotalScore = totalScore
            };
            db.PerformanceReports.Add(report);
            db.SaveChanges();

            foreach (var p in parts)
            {
                db.PerformanceReportItems.Add(new PerformanceReportItem
                {
                    PerformanceReportId = report.PerformanceReportId,
                    HardwareId = p.HardwareId,
                    CategoryName = p.Category?.Type ?? "",
                    Price = p.Price,
                    Score = p.Special ?? 5
                });
            }
            db.SaveChanges();
            ViewBag.SavedReportId = report.PerformanceReportId;
        }

        return View(vm);
    }

    public IActionResult History()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var list = db.PerformanceReports
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.PerformanceReportId)
            .ToList();

        return View(list);
    }

    public IActionResult Detail(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var report = db.PerformanceReports
            .Include(r => r.Items)
            .ThenInclude(i => i.Hardware)
            .ThenInclude(h => h.Category)
            .FirstOrDefault(r => r.PerformanceReportId == id && r.UserId == userId);

        if (report == null) return RedirectToAction(nameof(History));

        return View(report);
    }

    private List<CategoryOptionVM> BuildCategoryOptions(decimal budget, List<CategoryOptionVM>? selected)
    {
        var order = new[] { "Motherboard", "CPU", "RAM", "GPU", "Storage", "Power supply unit", "CPU cooler", "PC case" };
        var categories = db.Categories.Include(c => c.Hardware).OrderBy(c => c.CategoryId).ToList();

        // Determine selected CPU and motherboard first (needed for compatibility filtering)
        int? selectedCpuId = selected?.FirstOrDefault(c => c.Type.Equals("CPU", StringComparison.OrdinalIgnoreCase))?.SelectedHardwareId;
        int? selectedMbId = selected?.FirstOrDefault(c => c.Type.Equals("Motherboard", StringComparison.OrdinalIgnoreCase))?.SelectedHardwareId;

        Hardware? selectedCpu = null;
        Hardware? selectedMb = null;

        if (selectedCpuId.HasValue)
            selectedCpu = db.Hardware.FirstOrDefault(h => h.HardwareId == selectedCpuId.Value);
        if (selectedMbId.HasValue)
            selectedMb = db.Hardware.FirstOrDefault(h => h.HardwareId == selectedMbId.Value);

        return categories
            .OrderBy(c => Array.IndexOf(order, c.Type) >= 0 ? Array.IndexOf(order, c.Type) : 99)
            .ThenBy(c => c.CategoryId)
            .Select(c =>
            {
                var items = c.Hardware.AsQueryable();

                // Budget filtering: only show items within budget if budget is set
                if (budget > 0)
                    items = items.Where(h => h.Price <= budget);

                // Compatibility filtering
                if (c.Type.Equals("Motherboard", StringComparison.OrdinalIgnoreCase) && selectedCpu != null)
                {
                    var cpuSocket = selectedCpu.Socket ?? "";
                    if (!string.IsNullOrEmpty(cpuSocket))
                        items = items.Where(h => h.Socket == cpuSocket);
                }

                if (c.Type.Equals("RAM", StringComparison.OrdinalIgnoreCase) && selectedMb != null)
                {
                    var ddr = selectedMb.DdrType ?? "";
                    if (!string.IsNullOrEmpty(ddr))
                        items = items.Where(h => h.DdrType == ddr);
                }

                if (c.Type.Equals("PC case", StringComparison.OrdinalIgnoreCase) && selectedMb != null)
                {
                    var mbForm = selectedMb.FormFactor ?? "";
                    if (!string.IsNullOrEmpty(mbForm))
                        items = items.Where(h => (h.FormFactor ?? "").Contains(mbForm));
                }

                var selectedId = selected?.FirstOrDefault(x => x.CategoryId == c.CategoryId)?.SelectedHardwareId;
                var itemList = items.OrderBy(h => h.Name).ToList();
                if (c.Type.Equals("GPU", StringComparison.OrdinalIgnoreCase))
                    itemList = Helper.FilterDuplicateGpuFromDisplay(itemList, "GPU");

                return new CategoryOptionVM
                {
                    CategoryId = c.CategoryId,
                    Type = c.Type,
                    Items = itemList,
                    SelectedHardwareId = selectedId
                };
            }).ToList();
    }
}

