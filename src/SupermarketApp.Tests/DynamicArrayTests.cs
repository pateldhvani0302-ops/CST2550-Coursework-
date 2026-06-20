using SupermarketApp.DataStructures;
using Xunit;

namespace SupermarketApp.Tests
{
    public class DynamicArrayTests
    {
        [Fact]
        public void Add_IncreasesCountAndStoresItem()
        {
            var array = new DynamicArray<int>();
            array.Add(10);
            array.Add(20);

            Assert.Equal(2, array.Count);
            Assert.Equal(10, array[0]);
            Assert.Equal(20, array[1]);
        }

        [Fact]
        public void Add_ResizesBeyondInitialCapacity()
        {
            var array = new DynamicArray<int>(2);
            for (int i = 0; i < 50; i++) array.Add(i);

            Assert.Equal(50, array.Count);
            Assert.Equal(49, array[49]);
        }

        [Fact]
        public void RemoveAt_RemovesItemAndShiftsRemaining()
        {
            var array = new DynamicArray<string>();
            array.Add("a"); array.Add("b"); array.Add("c");

            var removed = array.RemoveAt(1);

            Assert.True(removed);
            Assert.Equal(2, array.Count);
            Assert.Equal("a", array[0]);
            Assert.Equal("c", array[1]);
        }

        [Fact]
        public void IndexOf_FindsMatchingPredicate()
        {
            var array = new DynamicArray<int>();
            array.Add(5); array.Add(15); array.Add(25);

            int index = array.IndexOf(x => x == 15);

            Assert.Equal(1, index);
        }
    }
}
