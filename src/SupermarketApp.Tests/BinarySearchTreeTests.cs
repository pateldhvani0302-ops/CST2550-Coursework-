using SupermarketApp.DataStructures;
using Xunit;

namespace SupermarketApp.Tests
{
    public class BinarySearchTreeTests
    {
        [Fact]
        public void Insert_AndTryGetValue_FindsExactKey()
        {
            var tree = new BinarySearchTree<string, int>();
            tree.Insert("Milk", 1);
            tree.Insert("Bread", 2);
            tree.Insert("Cheese", 3);

            bool found = tree.TryGetValue("Bread", out var value);

            Assert.True(found);
            Assert.Equal(2, value);
        }

        [Fact]
        public void InOrderValues_ReturnsValuesInSortedKeyOrder()
        {
            var tree = new BinarySearchTree<string, string>();
            tree.Insert("Milk", "Milk");
            tree.Insert("Bread", "Bread");
            tree.Insert("Cheese", "Cheese");

            var values = tree.InOrderValues();

            Assert.Equal(new[] { "Bread", "Cheese", "Milk" }, values);
        }

        [Fact]
        public void Remove_DeletesNodeAndKeepsTreeSearchable()
        {
            var tree = new BinarySearchTree<int, string>();
            tree.Insert(5, "five");
            tree.Insert(3, "three");
            tree.Insert(8, "eight");

            bool removed = tree.Remove(5);

            Assert.True(removed);
            Assert.False(tree.TryGetValue(5, out _));
            Assert.True(tree.TryGetValue(3, out _));
            Assert.True(tree.TryGetValue(8, out _));
        }

        [Fact]
        public void SearchByPrefix_ReturnsMatchingKeysOnly()
        {
            var tree = new BinarySearchTree<string, string>();
            tree.Insert("Milk", "Milk");
            tree.Insert("Mint Tea", "Mint Tea");
            tree.Insert("Bread", "Bread");

            var results = tree.SearchByPrefix("Mi", key => key);

            Assert.Equal(2, results.Length);
        }
    }
}
