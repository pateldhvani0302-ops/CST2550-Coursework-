using System;
using Microsoft.EntityFrameworkCore;
using SupermarketApp.Data;
using SupermarketApp.Models;
using SupermarketApp.Repositories;
using SupermarketApp.Services;
using Xunit;

namespace SupermarketApp.Tests
{
    public class ValidationServiceTests
    {
        private static SupermarketContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SupermarketContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new SupermarketContext(options);
        }

        [Fact]
        public void ValidateProduct_NegativePrice_ThrowsArgumentException()
        {
            using var context = CreateInMemoryContext(nameof(ValidateProduct_NegativePrice_ThrowsArgumentException));
            var repository = new ProductRepository(context);
            var product = new Product { Title = "Milk", Barcode = "1", Price = -5m, QuantityInStock = 10 };

            Assert.Throws<ArgumentException>(() => ValidationService.ValidateProduct(product, repository, true));
        }

        [Fact]
        public void ValidateProduct_DuplicateBarcodeOnNewProduct_ThrowsArgumentException()
        {
            using var context = CreateInMemoryContext(nameof(ValidateProduct_DuplicateBarcodeOnNewProduct_ThrowsArgumentException));
            var repository = new ProductRepository(context);
            repository.Add(new Product { Title = "Milk", Barcode = "1", Price = 1m, QuantityInStock = 10 });

            var duplicate = new Product { Title = "Other", Barcode = "1", Price = 1m, QuantityInStock = 5 };

            Assert.Throws<ArgumentException>(() => ValidationService.ValidateProduct(duplicate, repository, true));
        }

        [Fact]
        public void ValidateSupplier_MissingContactDetails_ThrowsArgumentException()
        {
            var supplier = new Supplier { Name = "Test Supplier", ContactName = "Jo" };
            Assert.Throws<ArgumentException>(() => ValidationService.ValidateSupplier(supplier));
        }

        [Fact]
        public void ValidateSupplier_ValidSupplier_DoesNotThrow()
        {
            var supplier = new Supplier { Name = "Test Supplier", ContactName = "Jo", Phone = "12345" };
            var exception = Record.Exception(() => ValidationService.ValidateSupplier(supplier));
            Assert.Null(exception);
        }
    }
}
