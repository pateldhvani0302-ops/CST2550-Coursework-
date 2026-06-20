using System;
using System.Linq;
using SupermarketApp.Models;

namespace SupermarketApp.Data
{
    public static class SeedData
    {
        public static void EnsureSeeded(SupermarketContext context)
        {
            context.Database.EnsureCreated();

            if (context.Categories.Any()) return; // already seeded

            var dairy = new Category { Name = "Dairy", Description = "Milk, cheese, yoghurt" };
            var bakery = new Category { Name = "Bakery", Description = "Bread and baked goods" };
            var drinks = new Category { Name = "Drinks", Description = "Soft drinks and juices" };
            context.Categories.AddRange(dairy, bakery, drinks);
            context.SaveChanges();

            var freshFarms = new Supplier
            {
                Name = "Fresh Farms Ltd",
                ContactName = "Lisa Grant",
                Phone = "020 7946 0958",
                Email = "sales@freshfarms.co.uk",
                Address = "12 Market St, London"
            };
            var goldenBakery = new Supplier
            {
                Name = "Golden Bakery Co",
                ContactName = "Tom Reid",
                Phone = "020 7946 0123",
                Email = "orders@goldenbakery.co.uk",
                Address = "5 Mill Lane, London"
            };
            context.Suppliers.AddRange(freshFarms, goldenBakery);
            context.SaveChanges();

            var products = new[]
            {
                new Product
                {
                    Title = "Whole Milk 1L", Barcode = "5000112637922",
                    CategoryId = dairy.CategoryId, SupplierId = freshFarms.SupplierId,
                    Price = 1.20m, QuantityInStock = 40, LowStockThreshold = 10,
                    ExpiryOrRestockDate = DateTime.Today.AddDays(10)
                },
                new Product
                {
                    Title = "Cheddar Cheese 200g", Barcode = "5000112637939",
                    CategoryId = dairy.CategoryId, SupplierId = freshFarms.SupplierId,
                    Price = 2.50m, QuantityInStock = 25, LowStockThreshold = 8,
                    ExpiryOrRestockDate = DateTime.Today.AddDays(20)
                },
                new Product
                {
                    Title = "White Bread Loaf", Barcode = "5000112637946",
                    CategoryId = bakery.CategoryId, SupplierId = goldenBakery.SupplierId,
                    Price = 1.10m, QuantityInStock = 4, LowStockThreshold = 10,
                    ExpiryOrRestockDate = DateTime.Today.AddDays(3)
                },
                new Product
                {
                    Title = "Orange Juice 1L", Barcode = "5000112637953",
                    CategoryId = drinks.CategoryId, SupplierId = freshFarms.SupplierId,
                    Price = 1.80m, QuantityInStock = 0, LowStockThreshold = 5,
                    ExpiryOrRestockDate = DateTime.Today.AddDays(15)
                },
            };
            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
