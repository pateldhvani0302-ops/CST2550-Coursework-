using System;
using System.Linq;
using SupermarketApp.Data;
using SupermarketApp.DataStructures;
using SupermarketApp.Models;

namespace SupermarketApp.Repositories
{
    /// <summary>
    /// Append-only log of stock movements (restocks, sales, adjustments) using a
    /// custom DynamicArray, backed by SQL Server via EF Core.
    ///
    /// Time complexity:
    ///   Restock / Adjust / RecordSaleMovement - O(1) amortised to log + O(1) average
    ///                                            product stock update
    ///   GetHistoryForProduct                  - O(n) (scans the full movement log)
    /// </summary>
    public class StockRepository
    {
        private readonly SupermarketContext _context;
        private readonly ProductRepository _productRepository;
        private readonly DynamicArray<StockRecord> _log = new();

        public StockRepository(SupermarketContext context, ProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
            foreach (var r in _context.StockRecords.ToList())
                _log.Add(r);
        }

        public void Restock(int productId, int quantity, string? notes = null)
        {
            if (quantity <= 0) throw new ArgumentException("Restock quantity must be positive.");
            var product = _productRepository.FindById(productId)
                ?? throw new InvalidOperationException("Product not found.");

            _productRepository.UpdateStock(productId, product.QuantityInStock + quantity);
            LogMovement(productId, StockMovementType.Restock, quantity, notes);
        }

        public void RecordSaleMovement(int productId, int quantitySold)
        {
            LogMovement(productId, StockMovementType.Sale, -quantitySold, "Sold via POS");
        }

        public void Adjust(int productId, int delta, string? notes = null)
        {
            var product = _productRepository.FindById(productId)
                ?? throw new InvalidOperationException("Product not found.");
            int newQty = product.QuantityInStock + delta;
            if (newQty < 0) throw new InvalidOperationException("Stock cannot go negative.");

            _productRepository.UpdateStock(productId, newQty);
            LogMovement(productId, StockMovementType.Adjustment, delta, notes);
        }

        private void LogMovement(int productId, StockMovementType type, int change, string? notes)
        {
            var record = new StockRecord
            {
                ProductId = productId,
                MovementType = type,
                QuantityChange = change,
                Timestamp = DateTime.Now,
                Notes = notes
            };
            _context.StockRecords.Add(record);
            _context.SaveChanges();
            _log.Add(record);
        }

        public StockRecord[] GetHistoryForProduct(int productId)
        {
            var result = new DynamicArray<StockRecord>();
            foreach (var r in _log.ToArray())
                if (r.ProductId == productId) result.Add(r);
            return result.ToArray();
        }

        public StockRecord[] GetAllHistory() => _log.ToArray();
    }
}
