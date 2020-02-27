using Scraper.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scraper.Interfaces.Importers
{
    public interface IRecipesImporter
    {
        Task<List<RecipeHeader>> ImportRecipeAsync(int startPage, int endPage);
        Task<List<Ingredients>> ImportIngredientsAcync(List<RecipeHeader> headers);
    }
}