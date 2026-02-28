using Microsoft.AspNetCore.Mvc;
using Demo.Services;

namespace Demo.Controllers;

public class RecommendController : Controller
{
    private readonly PcRecommendService _recommend;

    public RecommendController(PcRecommendService recommend) => _recommend = recommend;

    public IActionResult Index()
    {
        return View(new RecommendBuildVM { Budget = 5000, TargetTier = 7 });
    }

    [HttpPost]
    public async Task<IActionResult> Index(RecommendBuildVM vm)
    {
        if (vm.Budget < 1000)
        {
            ModelState.AddModelError("Budget", "Minimum budget is RM 1,000.");
            return View(vm);
        }

        var result = await _recommend.RecommendAsync(vm.Budget, vm.TargetTier);
        ViewBag.Result = result;
        ViewBag.Budget = vm.Budget;
        return View(vm);
    }
}
