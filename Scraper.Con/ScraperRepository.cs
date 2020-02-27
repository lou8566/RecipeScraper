using HtmlAgilityPack;
using Scraper.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scraper.Con
{
    public class ScraperRepository
    {
        public static void SaveChanges()
        {
            if (!Program.Context.ChangeTracker.HasChanges()) return;

            Program.Context.SaveChanges();
        }

        public static void AddIngredientsToDb(List<HtmlNode> ingredients, RecipeHeader header)
        {
            foreach (var ingredient in ingredients)
            {
                Console.WriteLine($"{ingredient.InnerHtml}");

                Program.Context.Ingredients.Add(new Ingredients
                {
                    RecipeHeaderId = header.Id,
                    Ingredient = ingredient.InnerHtml
                });
            }
        }

        public static void AddRecipeHeader(RecipeHeader recipeHeader)
        {
            Program.Context.RecipeHeaders.Add(recipeHeader);
        }

        public static RecipeHeader GetRecipeHeader(RecipeHeader recipeHeader)
        {
            return Program.Context.RecipeHeaders.FirstOrDefault(x => x.SurrogateId == recipeHeader.SurrogateId);
        }
    }
}