using Scraper.Data.Models;
using Scraper.Interfaces.Importers;
using Scraper.Interfaces.UnitOfWork;
using Scraper.Repository.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scaper.Core
{
    public class RecipeImporter
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private IRecipesImporter _importer;
        private List<RecipeHeader> _headers;
        public RecipeImporter(IRecipesImporter importer)
        {
            _importer = importer;
            _context = new ApplicationDbContext();
            _unitOfWork = new UnitOfWork(_context);
        }

        public async void ScrapeRecipeHeaders(int startPage, int endPage)
        {
            if (startPage < endPage)
                throw new ApplicationException("End page must not be less than the start page.");

            _headers = await _importer.ImportRecipeAsync(startPage, endPage);

            Console.WriteLine($"Importing {_headers.Count} recipes");
            _context.RecipeHeaders.AddRange(_headers);
            Save();
        }

        public async void ScrapeRecipeIngredients()
        {
            _headers = _context.RecipeHeaders.ToList();

            if (_headers == null)
                throw new ApplicationException("No Headers Found");

            var ingredients = await _importer.ImportIngredientsAcync(_headers);

            if (!ingredients.Any()) return;

            _context.Ingredients.AddRange(ingredients);
            Save();
            Console.WriteLine("Completed");
        }

        private void Save()
        {
            if (_context.ChangeTracker.HasChanges())
                _context.SaveChanges();
        }


    }
}
