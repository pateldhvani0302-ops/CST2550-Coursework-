using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SupermarketApp.Data;
using SupermarketApp.DataStructures;
using SupermarketApp.Models;

namespace SupermarketApp.Repositories
{
    /// <summary>
    /// Manages products using custom in-memory data structures for fast runtime
    /// operations, backed by SQL Server (via EF Core) for persistence.
    ///
    /// - HashTable&lt;string, Product&gt; keyed by Barcode  -> O(1) average barcode lookup.
    /// - BinarySearchTree&lt;string, Product&gt; keyed by Title -> O(log n) average exact-name
    ///   search, plus O(n) sorted (alphabetical) traversal for reports.
    /// - DynamicArray&lt;Product&gt; as the master list -> O(1) amortised add, O(n) full scan.
    /// </summary>
    public class ProductRepository
    {
        private readonly SupermarketContext _context;
        private readonly HashTable<string, Product> _byBarcode = new();
        private readonly BinarySearchTree<string, Product> _byTitle = new();
        private readonly DynamicArray<Product> _all = new();

        public ProductRepository(SupermarketContext context)
        {
            _context = context;
            LoadFromDatabase();
        }

        private void LoadFromDatabase()
        {
            foreach (var p in _context.Products.AsNoTracking().ToList())
                IndexProduct(p);
        }

        private void IndexProduct(Product p)
        {
            _all.Add(p);
            _byBarcode.Add(p.Barcode, p);
            _byTitle.Insert(p.Title, p);
        }

        /// <summary>O(1) average in-memory index update + database insert.</summary>
        public void Add(Product product)
        {
            if (_byBarcode.ContainsKey(product.Barcode))
                throw new InvalidOperationException($"A product with barcode '{product.Barcode}' already exists.");

            _context.Products.Add(product);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // Detach the failed entity so it doesn't get retried (and keep failing)
                // on every future SaveChanges() call.
                _context.Entry(product).State = EntityState.Detached;
                throw new InvalidOperationException(
                    "Could not save the product. Check that the Category ID and Supplier ID " +
                    $"exist (foreign key) and that field lengths/values are valid. Details: {ex.InnerException?.Message ?? ex.Message}",
                    ex);
            }
            IndexProduct(product);
        }

        /// <summary>O(1) average - HashTable lookup.</summary>
        public Product? FindByBarcode(string barcode)
            => _byBarcode.TryGetValue(barcode, out var product) ? product : null;

        /// <summary>O(log n) average - Binary Search Tree lookup.</summary>
        public Product? FindByExactTitle(string title)
            => _byTitle.TryGetValue(title, out var product) ? product : null;

        /// <summary>O(n) worst case - in-order prefix scan over the BST.</summary>
        public Product[] SearchByTitlePrefix(string prefix)
            => _byTitle.SearchByPrefix(prefix, key => key);

        /// <summary>O(n) - returns every product.</summary>
        public Product[] GetAll() => _all.ToArray();

        /// <summary>O(n) - alphabetical order via BST in-order traversal.</summary>
        public Product[] GetAllSortedByTitle() => _byTitle.InOrderValues();

        public void UpdateStock(int productId, int newQuantity)
        {
            var product = FindById(productId)
                ?? throw new InvalidOperationException("Product not found.");
            product.QuantityInStock = newQuantity;
            _context.Products.Update(product);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                _context.Entry(product).State = EntityState.Detached;
                throw new InvalidOperationException(
                    $"Could not update stock for product {productId}. Details: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        /// <summary>
        /// O(n) - no dedicated ID index is kept (acceptable for a small shop's
        /// catalogue size); this could be upgraded to a second HashTable keyed by
        /// ProductId if the catalogue grew significantly.
        /// </summary>
        public Product? FindById(int productId)
        {
            foreach (var p in _all.ToArray())
                if (p.ProductId == productId) return p;
            return null;
        }

        public bool Remove(int productId)
        {
            var product = FindById(productId);
            if (product == null) return false;

            _context.Products.Remove(product);
            _context.SaveChanges();

            _byBarcode.Remove(product.Barcode);
            _byTitle.Remove(product.Title);

            int idx = _all.IndexOf(p => p.ProductId == productId);
            if (idx >= 0) _all.RemoveAt(idx);
            return true;
        }

        /// <summary>O(n) - returns products that are LowStock or OutOfStock.</summary>
        public Product[] GetLowStock()
        {
            var result = new DynamicArray<Product>();
            foreach (var p in _all.ToArray())
                if (p.GetStockStatus() != StockStatus.InStock) result.Add(p);
            return result.ToArray();
        }
    }
}