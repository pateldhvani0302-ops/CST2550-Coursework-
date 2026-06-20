using SupermarketApp.Models;
using SupermarketApp.Search;
using Xunit;

namespace SupermarketApp.Tests
{
    public class SearchAlgorithmsTests
    {
        private static Product[] SampleProducts() => new[]
        {
            new Product { ProductId = 1, Title = "Milk", CategoryId = 1, SupplierId = 1 },
            new Product { ProductId = 2, Title = "Bread", CategoryId = 2, SupplierId = 1 },
            new Product { ProductId = 3, Title = "Cheese", CategoryId = 1, SupplierId = 2 },
        };

        [Fact]
        public void BinarySearchByTitle_FindsExistingProduct()
        {
            var products = SampleProducts();
            SearchAlgorithms.SortByTitle(products);

            var found = SearchAlgorithms.BinarySearchByTitle(products, "Cheese");

            Assert.NotNull(found);
            Assert.Equal(3, found!.ProductId);
        }

        [Fact]
        public void BinarySearchByTitle_ReturnsNullForMissingProduct()
        {
            var products = SampleProducts();
            SearchAlgorithms.SortByTitle(products);

            var found = SearchAlgorithms.BinarySearchByTitle(products, "Eggs");

            Assert.Null(found);
        }

        [Fact]
        public void LinearSearchByCategory_ReturnsAllMatches()
        {
            var products = SampleProducts();
            var result = SearchAlgorithms.LinearSearchByCategory(products, 1);
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void LinearSearchBySupplier_ReturnsAllMatches()
        {
            var products = SampleProducts();
            var result = SearchAlgorithms.LinearSearchBySupplier(products, 1);
            Assert.Equal(2, result.Length);
        }
    }
}
