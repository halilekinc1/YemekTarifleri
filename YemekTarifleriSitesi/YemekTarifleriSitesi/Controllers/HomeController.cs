using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using YemekTarifleriSitesi.Models;

namespace YemekTarifleriSitesi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index(int? categoryId, string? q)
    {
        var recipes = _context.Recipes.AsQueryable();
        if (categoryId.HasValue)
        {
            recipes = recipes.Where(r => r.CategoryId == categoryId.Value || (r.RecipeCategories != null && r.RecipeCategories.Any(rc => rc.CategoryId == categoryId.Value)));
        }
        if (!string.IsNullOrWhiteSpace(q))
        {
            recipes = recipes.Where(r => (r.Name ?? "").Contains(q) || (r.Ingredients ?? "").Contains(q));
        }
        ViewBag.SearchQuery = q;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.Categories = _context.Categories.ToList();
        return View(recipes.ToList());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
