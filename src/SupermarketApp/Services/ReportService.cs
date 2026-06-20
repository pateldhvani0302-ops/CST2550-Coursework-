using SupermarketApp.DataStructures;
using SupermarketApp.Models;
using SupermarketApp.Repositories;

namespace SupermarketApp.Services
{
    /// <summary>
    /// Produces day-to-day operational reports for the shop. All report methods are
    /// O(n) (or O(n) over sale items) since a report must, by definition, visit every
    /// relevant record at least once.
    /// </summary>
    public class ReportService
    {
        private readonly ProductRepository _products;
        private readonly SupplierRepository _suppliers;
        private readonly CategoryRepository _categories;
        private readonly SalesRepository _sales;

        public ReportService(ProductRepository products, SupplierRepository suppliers,
            CategoryRepository categories, SalesRepository sales)
        {
            _products = products;
            _suppliers = suppliers;
            _categories = categories;
            _sales = sales;
        }

        /// <summary>O(n).</summary>
        public Product[] LowStockReport() => _products.GetLowStock();

        /// <summary>O(n).</summary>
        public Product[] ProductsByCategory(int categoryId)
        {
            var result = new DynamicArray<Product>();
            foreach (var p in _products.GetAll())
                if (p.CategoryId == categoryId) result.Add(p);
            return result.ToArray();
        }

        /// <summary>O(n).</summary>
        public Product[] SupplierStockList(int supplierId)
        {
            var result = new DynamicArray<Product>();
            foreach (var p in _products.GetAll())
                if (p.SupplierId == supplierId) result.Add(p);
            return result.ToArray();
        }

        /// <summary>
        /// Aggregates total quantity sold and revenue per product across all sales.
        /// O(s * i) where s = number of sales and i = average items per sale -
        /// effectively O(n) over the total number of sale items, using a HashTable
        /// for O(1) average accumulation per product.
        /// </summary>
        public (int productId, string title, int totalQuantitySold, decimal totalRevenue)[] SalesByProduct()
        {
            var totals = new HashTable<int, (int quantity, decimal revenue)>();

            foreach (var sale in _sales.GetAll())
            {
                foreach (var item in _sales.GetItemsForSale(sale.SaleId))
                {
                    if (totals.TryGetValue(item.ProductId, out var current))
                        totals.Add(item.ProductId, (current.quantity + item.Quantity, current.revenue + item.LineTotal));
                    else
                        totals.Add(item.ProductId, (item.Quantity, item.LineTotal));
                }
            }

            var result = new DynamicArray<(int, string, int, decimal)>();
            foreach (var product in _products.GetAll())
            {
                if (totals.TryGetValue(product.ProductId, out var t))
                    result.Add((product.ProductId, product.Title, t.quantity, t.revenue));
            }
            return result.ToArray();
        }
    }
}
