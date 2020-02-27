using HtmlAgilityPack;
using Scaper.Core.Services;
using Scraper.Data.Models;
using Scraper.Interfaces.Importers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scaper.Core.Importers
{
    public class KraftRecipesImporter : IRecipesImporter
    {
        private const string BaseUrl = "http://www.kraftcanada.com/recipes";

        private List<string> _categoriesUrls = new List<string>();
        private List<string> _subCategoryUrls = new List<string>();
        private List<string> _recipeUrls = new List<string>();
        private PageServices _pageServices = new PageServices();

        public async Task<List<RecipeHeader>> ImportRecipeAsync(int startPage = 1, int endPage = 1)
        {
            await GetRecipeCategoriesAsync();
            await GetRecipeSubCategoriesAsync();
            await GetRecipesInSubCategoryAsync();
            return null;
        }

        private async Task GetRecipesInSubCategoryAsync()
        {
            foreach (var subCategoryUrl in _subCategoryUrls)
            {
                var url = BaseUrl + subCategoryUrl.Replace("/recipes", "");
                var htmlDocument = await _pageServices.GetHtmlDocumentAsync(url);
                GetRecipeUrl(htmlDocument);
            }
        }

        private void GetRecipeUrl(HtmlDocument htmlDocument)
        {
            var items = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("listicle-item")).ToList();

            foreach (var item in items)
            {
                var link = item.Descendants("a").First();
                _recipeUrls.Add(link.Attributes["href"].Value);
            }
        }

        private async Task GetRecipeSubCategoriesAsync()
        {
            foreach (var category in _categoriesUrls)
            {
                var categoryUrl = BaseUrl + category.Replace("/recipes", "");
                var htmlDocument = await _pageServices.GetHtmlDocumentAsync(categoryUrl);
                GetSubCategoryUrls(htmlDocument);
            }
        }

        private void GetSubCategoryUrls(HtmlDocument htmlDocument)
        {
            var cards = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("card-top")).ToList();

            var links = new List<string>();

            foreach (var card in cards)
            {
                var link = card.Descendants("a").First();
                _subCategoryUrls.Add(link.Attributes["href"].Value);
            }
        }

        public Task<List<Ingredients>> ImportIngredientsAcync(List<RecipeHeader> headers)
        {
            return null;
        }

        private async Task GetRecipeCategoriesAsync()
        {
            var htmlDocument = await _pageServices.GetHtmlDocumentAsync(BaseUrl);
            GetCategoriesUrls(htmlDocument);
        }

        private void GetCategoriesUrls(HtmlDocument htmlDocument)
        {
            var cards = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("card-top")).ToList();

            var links = new List<string>();

            foreach (var card in cards)
            {
                var link = card.Descendants("a").First();
                _categoriesUrls.Add(link.Attributes["href"].Value);
            }
        }
    }
}
