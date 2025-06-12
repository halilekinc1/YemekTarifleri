using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YemekTarifleriSitesi.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public ICollection<RecipeCategory>? RecipeCategories { get; set; }
    }

    public class RecipeCategory
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}