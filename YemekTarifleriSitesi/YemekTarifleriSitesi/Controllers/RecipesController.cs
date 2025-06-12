using Microsoft.AspNetCore.Mvc;
using YemekTarifleriSitesi.Models;
using System.Linq;

namespace YemekTarifleriSitesi.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var recipes = _context.Recipes.ToList();
            return View(recipes);
        }

        public IActionResult Details(int id)
        {
            var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null) return NotFound();
            return View(recipe);
        }
    }
}
