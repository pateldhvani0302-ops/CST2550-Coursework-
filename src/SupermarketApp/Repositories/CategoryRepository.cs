using System.Linq;
using SupermarketApp.Data;
using SupermarketApp.DataStructures;
using SupermarketApp.Models;

namespace SupermarketApp.Repositories
{
    /// <summary>Manages categories, backed by SQL Server via EF Core.</summary>
    public class CategoryRepository
    {
        private readonly SupermarketContext _context;
        private readonly DynamicArray<Category> _all = new();

        public CategoryRepository(SupermarketContext context)
        {
            _context = context;
            foreach (var c in _context.Categories.ToList())
                _all.Add(c);
        }

        public void Add(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            _all.Add(category);
        }

        public Category[] GetAll() => _all.ToArray();

        /// <summary>O(n) - small reference table, linear scan is sufficient.</summary>
        public Category? FindById(int id)
        {
            foreach (var c in _all.ToArray())
                if (c.CategoryId == id) return c;
            return null;
        }
    }
}
