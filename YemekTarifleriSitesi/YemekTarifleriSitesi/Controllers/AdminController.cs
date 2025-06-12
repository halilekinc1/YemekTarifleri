using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using YemekTarifleriSitesi.Models;

namespace YemekTarifleriSitesi.Controllers
{
    public class AdminController : Controller
    {
        private const string AdminUsername = "admin";
        private const string AdminPassword = "123";
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == AdminUsername && password == AdminPassword)
            {
                HttpContext.Session.SetString("IsAdminLoggedIn", "true");
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
            return View();
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
            {
                return RedirectToAction("Login");
            }

            var recipes = _context.Recipes
                .Select(r => new Recipe {
                    Id = r.Id,
                    Name = r.Name,
                    Ingredients = r.Ingredients,
                    Instructions = r.Instructions,
                    ImageUrl = r.ImageUrl,
                    CategoryId = r.CategoryId,
                    Category = r.Category,
                    RecipeCategories = r.RecipeCategories
                })
                .ToList();
            return View(recipes);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdminLoggedIn");
            return RedirectToAction("Login");
        }

        // Tarif ekleme sayfası
        public IActionResult AddRecipe()
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
            {
                return RedirectToAction("Login");
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRecipe(Recipe recipe, IFormFile? imageFile, int[]? selectedCategories)
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
            {
                return RedirectToAction("Login");
            }
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    recipe.ImageUrl = "/images/" + fileName;
                }
                if (selectedCategories != null)
                {
                    recipe.RecipeCategories = selectedCategories.Select(catId => new RecipeCategory { CategoryId = catId }).ToList();
                }
                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Tarif başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(recipe);
        }

        // Tarif düzenleme sayfası
        public IActionResult EditRecipe(int id)
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
            {
                return RedirectToAction("Login");
            }

            var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
            {
                return NotFound();
            }
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.SelectedCategories = _context.RecipeCategories.Where(rc => rc.RecipeId == id).Select(rc => rc.CategoryId).ToList();
            return View(recipe);
        }

        [HttpPost]
        public IActionResult EditRecipe(Recipe recipe, int[]? selectedCategories)
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var existingRecipe = _context.Recipes.FirstOrDefault(r => r.Id == recipe.Id);
                if (existingRecipe != null)
                {
                    existingRecipe.Name = recipe.Name;
                    existingRecipe.Ingredients = recipe.Ingredients;
                    existingRecipe.Instructions = recipe.Instructions;
                    existingRecipe.ImageUrl = recipe.ImageUrl;

                    // Ana kategori güncelle
                    if (selectedCategories != null && selectedCategories.Length > 0)
                    {
                        existingRecipe.CategoryId = selectedCategories[0];
                    }
                    else
                    {
                        existingRecipe.CategoryId = null;
                    }

                    // Çoklu kategori güncelle
                    var oldCategories = _context.RecipeCategories.Where(rc => rc.RecipeId == recipe.Id).ToList();
                    _context.RecipeCategories.RemoveRange(oldCategories);
                    if (selectedCategories != null)
                    {
                        foreach (var catId in selectedCategories)
                        {
                            _context.RecipeCategories.Add(new RecipeCategory { RecipeId = recipe.Id, CategoryId = catId });
                        }
                    }

                    _context.SaveChanges();
                    TempData["Message"] = "Tarif başarıyla güncellendi.";
                }
                return RedirectToAction("Index");
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(recipe);
        }

        // Tarif silme işlemi
        [HttpPost]
        public IActionResult DeleteRecipe(int id)
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
            {
                return RedirectToAction("Login");
            }

            var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe != null)
            {
                // İlişkili RecipeCategory kayıtlarını sil
                var relatedCategories = _context.RecipeCategories.Where(rc => rc.RecipeId == id).ToList();
                if (relatedCategories.Any())
                {
                    _context.RecipeCategories.RemoveRange(relatedCategories);
                }
                _context.Recipes.Remove(recipe);
                _context.SaveChanges();
                TempData["Message"] = "Tarif başarıyla silindi.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["CategoryMessage"] = "Kategori adı boş olamaz.";
                return RedirectToAction("Index");
            }
            if (_context.Categories.Any(c => c.Name == categoryName))
            {
                TempData["CategoryMessage"] = "Bu isimde bir kategori zaten var.";
                return RedirectToAction("Index");
            }
            _context.Categories.Add(new Category { Name = categoryName });
            _context.SaveChanges();
            TempData["CategoryMessage"] = "Kategori başarıyla eklendi.";
            return RedirectToAction("Index");
        }
    }
}
