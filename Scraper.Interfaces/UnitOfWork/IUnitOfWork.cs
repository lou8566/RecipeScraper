using Scraper.Data.Models;
using Scraper.Interfaces.Repositories;

namespace Scraper.Interfaces.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Ingredients> IngredientRepository { get; }
        IRepository<RecipeHeader> RecipeHeaderRepository { get; }

        void Complete();
        void Dispose();
    }
}