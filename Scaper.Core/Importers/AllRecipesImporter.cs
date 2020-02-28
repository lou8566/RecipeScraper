using HtmlAgilityPack;
using Scraper.Data.Models;
using Scraper.Interfaces.Importers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Scaper.Core.Importers
{


    public class AllRecipesImporter : IRecipesImporter
    {
        private const string BaseUrl = "https://www.allrecipes.com/recipes/87/everyday-cooking/vegetarian?page=";
        List<RecipeHeader> _recipeHeaders = new List<RecipeHeader>();
        static List<Ingredients> _ingredients = new List<Ingredients>();
        private int _currentRecipeId;

        public async Task<List<RecipeHeader>> ImportRecipeAsync(int startPage, int endPage)
        {
            if (endPage < startPage)
                throw new ArgumentException("Start Page must be less than or equal to the End Page");

            await ImportHeadersAsync(startPage, endPage);

            return _recipeHeaders;
        }

        private async Task ImportHeadersAsync(int startPage, int endPage)
        {
            if (endPage < startPage)
                throw new ArgumentException("Start Page must be less than or equal to the End Page");

            foreach (var url in GenerateUrls(startPage, endPage))
            {
                Console.WriteLine($"Getting Headers for {url}");
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(url);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var recipeList = GetRecipeList(htmlDocument);
                _recipeHeaders.AddRange(CreateRecipeHeaders(recipeList));
            }
        }

        private List<RecipeHeader> CreateRecipeHeaders(List<HtmlNode> recipeList)
        {
            var headers = new List<RecipeHeader>();
            foreach (var recipe in recipeList)
            {
                var detail = recipe.Descendants("ar-save-item").ToList();
                var dataId = detail[0].GetAttributeValue("data-id", "");
                var type = detail[0].GetAttributeValue("data-type", "");
                var uri = detail[0].GetAttributeValue("data-imageurl", "");

                var recipeCard = recipe.Descendants("div")
                    .Where(node => node.GetAttributeValue("class", "")
                        .Equals("grid-card-image-container")).ToList();

                var anchorTag = recipeCard[0].Descendants("a")
                    .ToList();

                var href = anchorTag[0].GetAttributeValue("href", "");

                headers.Add(new RecipeHeader
                {
                    SurrogateId = int.Parse(dataId),
                    Type = type.Replace("'", ""),
                    ImageUri = uri.Replace("'", ""),
                    RecipeUri = href
                });
            }

            return headers;
        }

        private static List<HtmlNode> GetRecipeList(HtmlDocument htmlDocument)
        {
            var recipes = htmlDocument.DocumentNode.Descendants("section")
                .Where(node => node.GetAttributeValue("id", "")
                    .Equals("fixedGridSection")).ToList();

            var recipeList = recipes[0].Descendants("article")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("fixed-recipe-card")).ToList();

            //var details = recipeList[0].Descendants("ar-save-item").ToList();
            return recipeList;
        }

        private List<string> GenerateUrls(int startPage, int endPage)
        {
            var urls = new List<string>();
            for (var i = startPage; i <= endPage; i++)
                urls.Add(BaseUrl + i);

            return urls;
        }

        public async Task<List<Ingredients>> ImportIngredientsAcync(List<RecipeHeader> headers)
        {
            if (!headers.Any()) return null;

            foreach (var header in headers)
            {
                Console.WriteLine($"Getting Recipe From {header.RecipeUri}");
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(header.RecipeUri);

                GetIngredients(html, header);
            }

            return _ingredients;

        }

        private static void GetIngredients(string html, RecipeHeader header)
        {
            var htmlDocument = new HtmlDocument();

            try
            {
                htmlDocument.LoadHtml(html);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }



            var ingredientsList = GetIngredientsList(htmlDocument);

            var ingredients = new List<HtmlNode>();
            if (!ingredientsList.Any())
            {
                ingredientsList = GetIngredientsList2(htmlDocument);
                GetIngredientDetails2(ingredientsList, ingredients);
            }
            else
                GetIngredientDetails(ingredientsList, ingredients);

            if (!ingredientsList.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Ingredients Found");
                Console.ForegroundColor = ConsoleColor.White;
            }


            ConvertIngredients(ingredients, header);
        }

        private static void ConvertIngredients(List<HtmlNode> ingredients, RecipeHeader header)
        {
            foreach (var ingredient in ingredients)
            {
                var i = new Ingredients
                {
                    RecipeHeaderId = header.Id,
                    Ingredient = ingredient.InnerHtml.Trim()

                };

                _ingredients.Add(i);
            }
        }

        private static void GetIngredientDetails(IEnumerable<HtmlNode> ingredientsList, List<HtmlNode> ingredients)
        {
            foreach (var t in ingredientsList)
            {
                ingredients.AddRange(t.Descendants("span")
                    .Where(node => node.GetAttributeValue("itemprop", "")
                        .Equals("recipeIngredient")).ToList());
            }
        }

        private static void GetIngredientDetails2(IEnumerable<HtmlNode> ingredientsList, List<HtmlNode> ingredients)
        {
            foreach (var t in ingredientsList)
            {
                ingredients.AddRange(t.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                        .Equals("ingredients-item-name")).ToList());
            }
        }

        private static IEnumerable<HtmlNode> GetIngredientsList2(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("ul")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("ingredients-section")).ToList();
        }

        private static IEnumerable<HtmlNode> GetIngredientsList(HtmlDocument htmlDocument)
        {
            var ingredientsList = GetIngredientsList(htmlDocument, "lst_ingredients_1");
            var ingredientsList2 = GetIngredientsList(htmlDocument, "lst_ingredients_2");
            ingredientsList.AddRange(ingredientsList2);
            return ingredientsList;
        }

        private static List<HtmlNode> GetIngredientsList(HtmlDocument htmlDocument, string identifier)
        {
            return htmlDocument.DocumentNode.Descendants("ul")
                .Where(node => node.GetAttributeValue("id", "")
                    .Equals(identifier)).ToList();
        }
    }
}
