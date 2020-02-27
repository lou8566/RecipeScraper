using System.ComponentModel.DataAnnotations;

namespace Scraper.Data.Models
{
    public class Ingredients
    {
        public int Id { get; set; }

        [Required]
        public int RecipeHeaderId { get; set; }

        [StringLength(255)]
        public string Ingredient { get; set; }

        public RecipeHeader RecipeHeader { get; set; }
    }
}