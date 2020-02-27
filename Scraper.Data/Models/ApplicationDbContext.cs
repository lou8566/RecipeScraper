using System.Data.Entity;

namespace Scraper.Data.Models
{

    public class ApplicationDbContext : DbContext
    {

        public DbSet<RecipeHeader> RecipeHeaders { get; set; }
        public DbSet<Ingredients> Ingredients { get; set; }

        public ApplicationDbContext() : base("Scrapper")
        {

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecipeHeader>()
                .HasIndex(r => r.SurrogateId);

            modelBuilder.Entity<Ingredients>()
                .HasRequired(r => r.RecipeHeader);
        }
    }
}
