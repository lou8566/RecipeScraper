using HtmlAgilityPack;
using Scaper.Core;
using Scaper.Core.Importers;
using Scraper.Data.Models;
using Scraper.Interfaces.Importers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Scraper.Con
{
    internal class Program
    {
        private static readonly List<RecipeHeader> Headers = new List<RecipeHeader>();

        internal static readonly ApplicationDbContext Context = new ApplicationDbContext();

        static void Main(string[] args)
        {
            IRecipesImporter importer = new KraftRecipesImporter();
            var imp = new RecipeImporter(importer);

            imp.ScrapeRecipeHeaders(2, 2);
            //imp.ScrapeRecipeIngredients();
            Console.WriteLine("Completed");
            Console.Read();
        }

        private static async void GetRecipeHeadersAsync(int startPage, int endPage)
        {
            if (startPage > endPage)
                throw ArgumentException("Start Page must be less than or equal to the End Page");

            const string baseUrl = "https://www.allrecipes.com/recipes/87/everyday-cooking/vegetarian?page=";

            var urls = new List<string>();

            for (var i = startPage; i <= endPage; i++) // 414
            {
                urls.Add(baseUrl + i);
            }

            foreach (var url in urls)
            {
                Console.WriteLine($"Getting recipes from :{url}");
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(url);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var recipeList = GetRecipeList(htmlDocument);
                ExtractDetailsIntoHeaders(recipeList);
                ProcessHeaders();
            }



            Console.WriteLine("Completed");
            Console.Read();
        }

        private static Exception ArgumentException(string v)
        {
            throw new NotImplementedException();
        }

        private static void ExtractDetailsIntoHeaders(List<HtmlNode> recipeList)
        {
            foreach (var recipe in recipeList)
            {
                var detail = recipe.Descendants("ar-save-item").ToList();
                var dataId = detail[0].GetAttributeValue("data-id", "");
                var type = detail[0].GetAttributeValue("data-type", "");
                var uri = detail[0].GetAttributeValue("data-imageurl", "");

                var recipeCard = recipe.Descendants("div")
                    .Where(node => node.GetAttributeValue("class", "")
                        .Equals("grid-card-image-container")).ToList();

                var link = recipeCard[0].Descendants("a")
                    .ToList();

                var link2 = link[0].GetAttributeValue("href", "");

                Headers.Add(new RecipeHeader
                {
                    SurrogateId = int.Parse(dataId),
                    Type = type.Replace("'", ""),
                    ImageUri = uri.Replace("'", ""),
                    RecipeUri = link2
                });
            }
        }

        private static void ProcessHeaders()
        {
            var count = 0;
            foreach (var recipeHeader in Headers)
            {
                // Check if we have this id in the database
                var recipe = ScraperRepository.GetRecipeHeader(recipeHeader);
                if (recipe != null) continue;

                // This is a new Recipe... Add it to the database.
                ScraperRepository.AddRecipeHeader(recipeHeader);
                count++;
            }

            if (!Context.ChangeTracker.HasChanges()) return;

            ScraperRepository.SaveChanges();
            Console.WriteLine($"\tAdded {count} recipes");

        }



        private static List<HtmlNode> GetRecipeList(HtmlDocument htmlDocument)
        {
            var recipes = htmlDocument.DocumentNode.Descendants("section")
                .Where(node => node.GetAttributeValue("id", "")
                    .Equals("fixedGridSection")).ToList();

            var recipeList = recipes[0].Descendants("article")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("fixed-recipe-card")).ToList();

            var details = recipeList[0].Descendants("ar-save-item").ToList();
            return recipeList;
        }

        private static async void GetRecipeDetailsAsync()
        {
            //var recipeHeader = Context.RecipeHeaders.Where(x => x.Id >= 541).ToList();
            var recipeHeader = Context.RecipeHeaders.ToList();

            foreach (var header in recipeHeader)
            {
                Console.WriteLine($"Getting recipe from {header.RecipeUri}");
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(header.RecipeUri);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var ingredientsList = GetIngredientsList(htmlDocument);

                var ingredients = new List<HtmlNode>();

                if (ingredientsList.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"No Ingredients Found for {header.RecipeUri} attempting 2nd approach");
                    Console.ForegroundColor = ConsoleColor.White;

                    // Use 2nd Approach
                    ingredientsList = GetIngredientsList2(htmlDocument);

                    GetDetails2(ingredientsList, ingredients);
                }
                else
                {
                    GetDetails(ingredientsList, ingredients);
                }


                if (ingredients.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No Ingredients Found");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                ScraperRepository.AddIngredientsToDb(ingredients, header);

                ScraperRepository.SaveChanges();

                Console.WriteLine($"\tAdded {ingredients.Count} ingredients");
            }

        }

        private static void GetDetails(List<HtmlNode> ingredientsList, List<HtmlNode> ingredients)
        {
            foreach (var t in ingredientsList)
            {
                ingredients.AddRange(t.Descendants("span")
                    .Where(node => node.GetAttributeValue("itemprop", "")
                        .Equals("recipeIngredient")).ToList());
            }
        }

        private static void GetDetails2(List<HtmlNode> ingredientsList, List<HtmlNode> ingredients)
        {
            foreach (var t in ingredientsList)
            {
                ingredients.AddRange(t.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                        .Equals("ingredients-item-name")).ToList());
            }
        }

        private static List<HtmlNode> GetIngredientsList2(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("ul")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("ingredients-section")).ToList();
        }

        private static List<HtmlNode> GetIngredientsList(HtmlDocument htmlDocument)
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
