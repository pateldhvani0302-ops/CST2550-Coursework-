using SupermarketApp.DataStructures;
using Xunit;

namespace SupermarketApp.Tests
{
    public class HashTableTests
    {
        [Fact]
        public void Add_AndTryGetValue_ReturnsStoredValue()
        {
            var table = new HashTable<string, int>();
            table.Add("apple", 1);
            table.Add("banana", 2);

            bool found = table.TryGetValue("apple", out var value);

            Assert.True(found);
            Assert.Equal(1, value);
        }

        [Fact]
        public void Add_WithExistingKey_UpdatesValue()
        {
            var table = new HashTable<string, int>();
            table.Add("apple", 1);
            table.Add("apple", 99);

            table.TryGetValue("apple", out var value);

            Assert.Equal(99, value);
            Assert.Equal(1, table.Count);
        }

        [Fact]
        public void Remove_DeletesKeyAndDecreasesCount()
        {
            var table = new HashTable<string, int>();
            table.Add("apple", 1);

            bool removed = table.Remove("apple");

            Assert.True(removed);
            Assert.False(table.ContainsKey("apple"));
        }

        [Fact]
        public void TryGetValue_MissingKey_ReturnsFalse()
        {
            var table = new HashTable<string, int>();
            bool found = table.TryGetValue("missing", out _);
            Assert.False(found);
        }

        [Fact]
        public void Add_BeyondLoadFactor_TriggersResizeWithoutLosingData()
        {
            var table = new HashTable<int, int>(4);
            for (int i = 0; i < 100; i++) table.Add(i, i * 10);

            for (int i = 0; i < 100; i++)
            {
                Assert.True(table.TryGetValue(i, out var value));
                Assert.Equal(i * 10, value);
            }
        }
    }
}
