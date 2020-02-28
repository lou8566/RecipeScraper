using HtmlAgilityPack;
using Scaper.Core.Services;
using Scraper.Data.Models;
using Scraper.Interfaces.Importers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scaper.Core.Importers
{
    public class KraftRecipesImporter : IRecipesImporter
    {
        private const string BaseUrl = "http://www.kraftcanada.com/recipes";
        private List<string> _errors = new List<string>();
        private List<string> _categoriesUrls = new List<string>();
        private List<string> _subCategoryUrls = new List<string>();
        private List<RecipeHeader> _recipeUrls = new List<RecipeHeader>();
        private PageServices _pageServices = new PageServices();

        public async Task<List<RecipeHeader>> ImportRecipeAsync(int startPage = 0, int endPage = 0)
        {
            await GetRecipeCategoriesAsync();
            await GetRecipeSubCategoriesAsync();
            await GetRecipesInSubCategoryAsync();

            return _recipeUrls;
        }

        private async Task GetRecipesInSubCategoryAsync()
        {
            foreach (var subCategoryUrl in _subCategoryUrls)
            {
                var url = BaseUrl + subCategoryUrl;
                try
                {
                    var htmlDocument = await _pageServices.GetHtmlDocumentAsync(url);
                    GetRecipeUrl(htmlDocument);
                }
                catch (Exception e)
                {
                    _errors.Add(url);
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"{url} added to errors");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        private void GetRecipeUrl(HtmlDocument htmlDocument)
        {
            var items = _pageServices.GetHtml(htmlDocument, "div", "class", "listicle-item");
            if (!items.Any()) items = _pageServices.GetHtml(htmlDocument, "div", "class", "card-top");



            foreach (var item in items)
            {
                var link = item.Descendants("a").First();
                var href = link.Attributes["href"].Value.Replace("/recipes", "");

                var image = item.Descendants("img").First().Attributes["data-yo-src"].Value;

                var recipeHeader = new RecipeHeader
                {
                    ImageUri = "http:" + image,
                    RecipeUri = BaseUrl + href,
                    Type = "Recipe",
                    SurrogateId = int.Parse(href.Substring(href.LastIndexOf("-") + 1))
                };

                _recipeUrls.Add(recipeHeader);
                Console.WriteLine($"Recipe URL: {href}");
            }
        }

        private async Task GetRecipeSubCategoriesAsync()
        {
            foreach (var category in _categoriesUrls)
            {
                var categoryUrl = BaseUrl + category;
                try
                {
                    var htmlDocument = await _pageServices.GetHtmlDocumentAsync(categoryUrl);
                    GetSubCategoryUrls(htmlDocument);
                }
                catch (Exception e)
                {
                    _errors.Add(categoryUrl);
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"{categoryUrl} added to errors");
                    Console.ForegroundColor = ConsoleColor.White;
                }

            }
        }

        private void GetSubCategoryUrls(HtmlDocument htmlDocument)
        {
            var cards = _pageServices.GetHtml(htmlDocument, "div", "class", "card-top");

            var links = new List<string>();

            foreach (var card in cards)
            {
                var link = card.Descendants("a").First();
                var href = link.Attributes["href"].Value;

                if (href.StartsWith("/recipes/"))
                    href = href.Replace("/recipes", "");

                if (href.StartsWith(BaseUrl))
                    href = href.Replace(BaseUrl, "");


                _subCategoryUrls.Add(href);

                Console.WriteLine($"Subcategory URL: {href}");
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
            var cards = _pageServices.GetHtml(htmlDocument, "div", "class", "card-top");

            foreach (var card in cards)
            {
                var link = card.Descendants("a").First();
                var href = link.Attributes["href"].Value.Replace("/recipes", "");
                _categoriesUrls.Add(href);
                Console.WriteLine($"Category URL: {href}");
            }
        }
    }
}
