using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Models;

namespace Demo.Controllers;

[Authorize(Roles = Roles.Admin)]
public class HardwareAdminController : Controller
{
    private readonly DB db;
    private readonly Helper helper;

    public HardwareAdminController(DB db, Helper helper)
    {
        this.db = db;
        this.helper = helper;
    }

    public IActionResult Index(int? categoryId)
    {
        var query = db.Hardware.Include(h => h.Category).AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(h => h.CategoryId == categoryId.Value);
        var list = query.OrderBy(h => h.Category!.Type).ThenBy(h => h.Name).ToList();
        ViewBag.Categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
        ViewBag.SelectedCategoryId = categoryId;
        return View(list);
    }

    public IActionResult Create()
    {
        ViewBag.Categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
        return View(new HardwareEditVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(HardwareEditVM vm)
    {
        ViewBag.Categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), "Name is required.");
        if (vm.CategoryId <= 0)
            ModelState.AddModelError(nameof(vm.CategoryId), "Please select a category.");
        if (vm.PictureFile == null)
            ModelState.AddModelError(nameof(vm.PictureFile), "Display image is required.");
        if (vm.CleanPictureFile == null)
            ModelState.AddModelError(nameof(vm.CleanPictureFile), "Clean image is required.");

        if (ModelState.IsValid)
        {
            var pictureName = helper.SaveHardwareImage(vm.PictureFile!, "hardware");
            var cleanName = helper.SaveHardwareImage(vm.CleanPictureFile!, "implant");
            var h = new Hardware
            {
                Name = vm.Name!.Trim(),
                CategoryId = vm.CategoryId,
                Price = vm.Price,
                Detail = vm.Detail?.Trim(),
                Brand = vm.Brand?.Trim(),
                Picture = pictureName,
                CleanPicture = cleanName,
                Socket = vm.Socket?.Trim(),
                DdrType = vm.DdrType?.Trim(),
                FormFactor = vm.FormFactor?.Trim(),
                Special = vm.Special,
                RamSlots = vm.RamSlots,
                ReleaseYear = vm.ReleaseYear,
                Rating = vm.Rating,
                PerformanceScore = vm.PerformanceScore,
                PowerConsumption = vm.PowerConsumption,
                GamingScore = vm.GamingScore,
                OfficeScore = vm.OfficeScore,
                EditingScore = vm.EditingScore,
                Reference = vm.Reference?.Trim()
            };
            db.Hardware.Add(h);
            db.SaveChanges();
            TempData["Info"] = "Hardware added.";
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    public IActionResult Edit(int id)
    {
        var h = db.Hardware.Include(x => x.Category).FirstOrDefault(x => x.HardwareId == id);
        if (h == null)
            return RedirectToAction(nameof(Index));
        ViewBag.Categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
        var vm = new HardwareEditVM
        {
            HardwareId = h.HardwareId,
            Name = h.Name,
            CategoryId = h.CategoryId,
            Price = h.Price,
            Detail = h.Detail,
            Brand = h.Brand,
            Socket = h.Socket,
            DdrType = h.DdrType,
            FormFactor = h.FormFactor,
            Special = h.Special,
            RamSlots = h.RamSlots,
            ReleaseYear = h.ReleaseYear,
            Rating = h.Rating,
            PerformanceScore = h.PerformanceScore,
            PowerConsumption = h.PowerConsumption,
            GamingScore = h.GamingScore,
            OfficeScore = h.OfficeScore,
            EditingScore = h.EditingScore,
            Reference = h.Reference,
            CurrentPicture = h.Picture,
            CurrentCleanPicture = h.CleanPicture
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, HardwareEditVM vm)
    {
        var h = db.Hardware.FirstOrDefault(x => x.HardwareId == id);
        if (h == null)
            return RedirectToAction(nameof(Index));
        ViewBag.Categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
        vm.HardwareId = id;
        vm.CurrentPicture = h.Picture;
        vm.CurrentCleanPicture = h.CleanPicture;
        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), "Name is required.");
        if (vm.CategoryId <= 0)
            ModelState.AddModelError(nameof(vm.CategoryId), "Please select a category.");

        if (ModelState.IsValid)
        {
            h.Name = vm.Name!.Trim();
            h.CategoryId = vm.CategoryId;
            h.Price = vm.Price;
            h.Detail = vm.Detail?.Trim();
            h.Brand = vm.Brand?.Trim();
            h.Socket = vm.Socket?.Trim();
            h.DdrType = vm.DdrType?.Trim();
            h.FormFactor = vm.FormFactor?.Trim();
            h.Special = vm.Special;
            h.RamSlots = vm.RamSlots;
            h.ReleaseYear = vm.ReleaseYear;
            h.Rating = vm.Rating;
            h.PerformanceScore = vm.PerformanceScore;
            h.PowerConsumption = vm.PowerConsumption;
            h.GamingScore = vm.GamingScore;
            h.OfficeScore = vm.OfficeScore;
            h.EditingScore = vm.EditingScore;
            h.Reference = vm.Reference?.Trim();
            if (vm.PictureFile != null)
            {
                if (!string.IsNullOrEmpty(h.Picture))
                    helper.DeletePhoto(h.Picture, "hardware");
                h.Picture = helper.SaveHardwareImage(vm.PictureFile, "hardware");
            }
            if (vm.CleanPictureFile != null)
            {
                if (!string.IsNullOrEmpty(h.CleanPicture))
                    helper.DeletePhoto(h.CleanPicture, "implant");
                h.CleanPicture = helper.SaveHardwareImage(vm.CleanPictureFile, "implant");
            }
            db.SaveChanges();
            TempData["Info"] = "Hardware updated.";
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var h = db.Hardware.FirstOrDefault(x => x.HardwareId == id);
        if (h == null)
            return RedirectToAction(nameof(Index));
        if (!string.IsNullOrEmpty(h.Picture))
            helper.DeletePhoto(h.Picture, "hardware");
        if (!string.IsNullOrEmpty(h.CleanPicture))
            helper.DeletePhoto(h.CleanPicture, "implant");
        db.Hardware.Remove(h);
        db.SaveChanges();
        TempData["Info"] = "Hardware deleted.";
        return RedirectToAction(nameof(Index));
    }
}
