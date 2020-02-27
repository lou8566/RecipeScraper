using Scraper.Data.Models;
using Scraper.Interfaces.Repositories;
using Scraper.Interfaces.UnitOfWork;
using System;

namespace Scraper.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<Ingredients> IngredientRepository { get; }
        public IRepository<RecipeHeader> RecipeHeaderRepository { get; }
        public void Complete()
        {
            _context.SaveChanges();
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}