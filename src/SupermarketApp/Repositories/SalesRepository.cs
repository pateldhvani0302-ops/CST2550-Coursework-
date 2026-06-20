using System;
using System.Linq;
using SupermarketApp.Data;
using SupermarketApp.DataStructures;
using SupermarketApp.Models;

namespace SupermarketApp.Repositories
{
    /// <summary>
    /// Records sale transactions (a Sale plus one or more SaleItems), validates stock
    /// availability, deducts stock, and persists everything via EF Core / SQL Server.
    /// </summary>
    public class SalesRepository
    {
        private readonly SupermarketContext _context;
        private readonly ProductRepository _productRepository;
        private readonly StockRepository _stockRepository;
        private readonly DynamicArray<Sale> _sales = new();

        public SalesRepository(SupermarketContext context, ProductRepository productRepository, StockRepository stockRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            foreach (var s in _context.Sales.ToList())
                _sales.Add(s);
        }

        /// <summary>
        /// Records a sale made up of one or more line items (productId, quantity).
        /// O(k) where k = number of line items in the sale.
        /// </summary>
        public Sale RecordSale((int productId, int quantity)[] lines)
        {
            if (lines == null || lines.Length == 0)
                throw new ArgumentException("A sale must contain at least one item.");

            var saleItems = new DynamicArray<SaleItem>();
            decimal total = 0;

            foreach (var (productId, quantity) in lines)
            {
                if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
                var product = _productRepository.FindById(productId)
                    ?? throw new InvalidOperationException($"Product {productId} not found.");
                if (product.QuantityInStock < quantity)
                    throw new InvalidOperationException($"Insufficient stock for '{product.Title}'.");

                var item = new SaleItem { ProductId = productId, Quantity = quantity, UnitPrice = product.Price };
                saleItems.Add(item);
                total += item.LineTotal;
            }

            var sale = new Sale { SaleDate = DateTime.Now, TotalAmount = total };
            _context.Sales.Add(sale);
            _context.SaveChanges(); // generates SaleId

            foreach (var item in saleItems.ToArray())
            {
                item.SaleId = sale.SaleId;
                _context.SaleItems.Add(item);
            }
            _context.SaveChanges();

            // Apply stock deductions and log each movement
            foreach (var item in saleItems.ToArray())
            {
                var product = _productRepository.FindById(item.ProductId)!;
                _productRepository.UpdateStock(item.ProductId, product.QuantityInStock - item.Quantity);
                _stockRepository.RecordSaleMovement(item.ProductId, item.Quantity);
            }

            sale.Items = saleItems.ToArray();
            _sales.Add(sale);
            return sale;
        }

        public Sale[] GetAll() => _sales.ToArray();

        public SaleItem[] GetItemsForSale(int saleId)
            => _context.SaleItems.Where(si => si.SaleId == saleId).ToArray();
    }
}
