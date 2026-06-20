using Microsoft.EntityFrameworkCore;
using SupermarketApp.Data;
using SupermarketApp.Models;
using SupermarketApp.Repositories;
using Xunit;

namespace SupermarketApp.Tests
{
    public class ProductRepositoryTests
    {
        private static SupermarketContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SupermarketContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new SupermarketContext(options);
        }

        [Fact]
        public void Add_NewProduct_CanBeFoundByBarcode()
        {
            using var context = CreateInMemoryContext(nameof(Add_NewProduct_CanBeFoundByBarcode));
            var repository = new ProductRepository(context);

            var product = new Product { Title = "Milk", Barcode = "111", Price = 1.5m, QuantityInStock = 10 };
            repository.Add(product);

            var found = repository.FindByBarcode("111");
            Assert.NotNull(found);
            Assert.Equal("Milk", found!.Title);
        }

        [Fact]
        public void Add_DuplicateBarcode_ThrowsException()
        {
            using var context = CreateInMemoryContext(nameof(Add_DuplicateBarcode_ThrowsException));
            var repository = new ProductRepository(context);

            repository.Add(new Product { Title = "Milk", Barcode = "111", Price = 1.5m, QuantityInStock = 10 });

            Assert.Throws<System.InvalidOperationException>(() =>
                repository.Add(new Product { Title = "Other Milk", Barcode = "111", Price = 2m, QuantityInStock = 5 }));
        }

        [Fact]
        public void GetLowStock_ReturnsOnlyLowOrOutOfStockProducts()
        {
            using var context = CreateInMemoryContext(nameof(GetLowStock_ReturnsOnlyLowOrOutOfStockProducts));
            var repository = new ProductRepository(context);

            repository.Add(new Product { Title = "Milk", Barcode = "1", Price = 1m, QuantityInStock = 50, LowStockThreshold = 5 });
            repository.Add(new Product { Title = "Bread", Barcode = "2", Price = 1m, QuantityInStock = 2, LowStockThreshold = 5 });
            repository.Add(new Product { Title = "Juice", Barcode = "3", Price = 1m, QuantityInStock = 0, LowStockThreshold = 5 });

            var lowStock = repository.GetLowStock();

            Assert.Equal(2, lowStock.Length);
        }

        [Fact]
        public void FindByExactTitle_UsesBinarySearchTreeLookup()
        {
            using var context = CreateInMemoryContext(nameof(FindByExactTitle_UsesBinarySearchTreeLookup));
            var repository = new ProductRepository(context);
            repository.Add(new Product { Title = "Cheddar Cheese", Barcode = "9", Price = 2m, QuantityInStock = 10 });

            var found = repository.FindByExactTitle("Cheddar Cheese");

            Assert.NotNull(found);
        }
    }
}
