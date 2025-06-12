using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace YemekTarifleriSitesi.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string? Name { get; set; } // Renamed from Title and made nullable

        [Required]
        public string? Ingredients { get; set; } // Made nullable

        [Required]
        public string? Instructions { get; set; } // Made nullable

        public string? ImageUrl { get; set; } // Made nullable

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<RecipeCategory>? RecipeCategories { get; set; }
    }
}
