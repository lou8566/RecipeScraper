using System.ComponentModel.DataAnnotations;

namespace Scraper.Data.Models
{
    public class RecipeHeader
    {
        public int Id { get; set; }

        [Required]
        public int SurrogateId { get; set; }

        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(255)]
        public string ImageUri { get; set; }

        [StringLength(255)]
        public string RecipeUri { get; set; }
    }
}