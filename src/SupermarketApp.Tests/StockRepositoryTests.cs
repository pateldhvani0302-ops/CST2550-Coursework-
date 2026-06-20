using Microsoft.EntityFrameworkCore;
using SupermarketApp.Data;
using SupermarketApp.Models;
using SupermarketApp.Repositories;
using Xunit;

namespace SupermarketApp.Tests
{
    public class StockRepositoryTests
    {
        private static SupermarketContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SupermarketContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new SupermarketContext(options);
        }

        [Fact]
        public void Restock_IncreasesQuantityAndLogsMovement()
        {
            using var context = CreateInMemoryContext(nameof(Restock_IncreasesQuantityAndLogsMovement));
            var products = new ProductRepository(context);
            var stock = new StockRepository(context, products);

            var product = new Product { Title = "Milk", Barcode = "1", Price = 1m, QuantityInStock = 10 };
            products.Add(product);

            stock.Restock(product.ProductId, 20, "Delivery from supplier");

            var updated = products.FindById(product.ProductId);
            Assert.Equal(30, updated!.QuantityInStock);

            var history = stock.GetHistoryForProduct(product.ProductId);
            Assert.Single(history);
            Assert.Equal(StockMovementType.Restock, history[0].MovementType);
            Assert.Equal(20, history[0].QuantityChange);
        }

        [Fact]
        public void Restock_ZeroOrNegativeQuantity_ThrowsArgumentException()
        {
            using var context = CreateInMemoryContext(nameof(Restock_ZeroOrNegativeQuantity_ThrowsArgumentException));
            var products = new ProductRepository(context);
            var stock = new StockRepository(context, products);

            var product = new Product { Title = "Milk", Barcode = "1", Price = 1m, QuantityInStock = 10 };
            products.Add(product);

            Assert.Throws<System.ArgumentException>(() => stock.Restock(product.ProductId, 0));
            Assert.Throws<System.ArgumentException>(() => stock.Restock(product.ProductId, -5));
        }

        [Fact]
        public void Adjust_PositiveDelta_IncreasesStockAndLogsAdjustment()
        {
            using var context = CreateInMemoryContext(nameof(Adjust_PositiveDelta_IncreasesStockAndLogsAdjustment));
            var products = new ProductRepository(context);
            var stock = new StockRepository(context, products);

            var product = new Product { Title = "Bread", Barcode = "2", Price = 1m, QuantityInStock = 10 };
            products.Add(product);

            stock.Adjust(product.ProductId, 5, "Stock count correction");

            Assert.Equal(15, products.FindById(product.ProductId)!.QuantityInStock);

            var history = stock.GetHistoryForProduct(product.ProductId);
            Assert.Equal(StockMovementType.Adjustment, history[0].MovementType);
            Assert.Equal(5, history[0].QuantityChange);
        }

        [Fact]
        public void Adjust_NegativeDeltaBelowZero_ThrowsInvalidOperationException()
        {
            using var context = CreateInMemoryContext(nameof(Adjust_NegativeDeltaBelowZero_ThrowsInvalidOperationException));
            var products = new ProductRepository(context);
            var stock = new StockRepository(context, products);

            var product = new Product { Title = "Juice", Barcode = "3", Price = 1m, QuantityInStock = 3 };
            products.Add(product);

            Assert.Throws<System.InvalidOperationException>(() => stock.Adjust(product.ProductId, -10));
        }

        [Fact]
        public void GetHistoryForProduct_ReturnsOnlyThatProductsMovements()
        {
            using var context = CreateInMemoryContext(nameof(GetHistoryForProduct_ReturnsOnlyThatProductsMovements));
            var products = new ProductRepository(context);
            var stock = new StockRepository(context, products);

            var milk = new Product { Title = "Milk", Barcode = "1", Price = 1m, QuantityInStock = 10 };
            var bread = new Product { Title = "Bread", Barcode = "2", Price = 1m, QuantityInStock = 10 };
            products.Add(milk);
            products.Add(bread);

            stock.Restock(milk.ProductId, 5);
            stock.Restock(bread.ProductId, 7);
            stock.Adjust(milk.ProductId, -2);

            var milkHistory = stock.GetHistoryForProduct(milk.ProductId);
            Assert.Equal(2, milkHistory.Length);
        }
    }
}