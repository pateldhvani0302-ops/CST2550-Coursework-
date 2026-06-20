using Microsoft.EntityFrameworkCore;
using SupermarketApp.Data;
using SupermarketApp.Models;
using SupermarketApp.Repositories;
using Xunit;

namespace SupermarketApp.Tests
{
    public class SalesRepositoryTests
    {
        private static SupermarketContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SupermarketContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new SupermarketContext(options);
        }

        [Fact]
        public void RecordSale_ValidLine_DeductsStockAndCalculatesTotal()
        {
            using var context = CreateInMemoryContext(nameof(RecordSale_ValidLine_DeductsStockAndCalculatesTotal));
            var products = new ProductRepository(context);
            var stock = new StockRepository(context, products);
            var sales = new SalesRepository(context, products, stock);

            var product = new Product { Title = "Milk", Barcode = "1", Price = 2m, QuantityInStock = 10 };
            products.Add(product);

            var sale = sales.RecordSale(new[] { (product.ProductId, 3) });

            Assert.Equal(6m, sale.TotalAmount);
            Assert.Equal(7, products.FindById(product.ProductId)!.QuantityInStock);
        }

        [Fact]
        public void RecordSale_InsufficientStock_ThrowsInvalidOperationException()
        {
            using var context = CreateInMemoryContext(nameof(RecordSale_InsufficientStock_ThrowsInvalidOperationException));
            var products = new ProductRepository(context);
            var stock = new StockRepository(context, products);
            var sales = new SalesRepository(context, products, stock);

            var product = new Product { Title = "Milk", Barcode = "1", Price = 2m, QuantityInStock = 2 };
            products.Add(product);

            Assert.Throws<System.InvalidOperationException>(() =>
                sales.RecordSale(new[] { (product.ProductId, 5) }));
        }

        [Fact]
        public void RecordSale_LogsStockMovement()
        {
            using var context = CreateInMemoryContext(nameof(RecordSale_LogsStockMovement));
            var products = new ProductRepository(context);
            var stock = new StockRepository(context, products);
            var sales = new SalesRepository(context, products, stock);

            var product = new Product { Title = "Bread", Barcode = "2", Price = 1m, QuantityInStock = 10 };
            products.Add(product);

            sales.RecordSale(new[] { (product.ProductId, 4) });

            var history = stock.GetHistoryForProduct(product.ProductId);
            Assert.Single(history);
            Assert.Equal(StockMovementType.Sale, history[0].MovementType);
            Assert.Equal(-4, history[0].QuantityChange);
        }
    }
}
