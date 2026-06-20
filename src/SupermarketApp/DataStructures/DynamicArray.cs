using System;

namespace SupermarketApp.DataStructures
{
    /// <summary>
    /// Custom resizable array implementation (does not use List&lt;T&gt; or any other
    /// built-in .NET collection). This is the underlying storage primitive used by
    /// the repositories to hold products, suppliers, stock records and sales.
    ///
    /// Time complexity:
    ///   Add        - O(1) amortised (occasional O(n) when the backing array doubles)
    ///   this[i]    - O(1)
    ///   RemoveAt   - O(n) (elements after the removed index are shifted left)
    ///   IndexOf    - O(n)
    /// </summary>
    public class DynamicArray<T>
    {
        private T[] _items;
        private int _count;

        public DynamicArray(int capacity = 4)
        {
            if (capacity < 1) capacity = 4;
            _items = new T[capacity];
            _count = 0;
        }

        public int Count => _count;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
                return _items[index];
            }
            set
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
                _items[index] = value;
            }
        }

        public void Add(T item)
        {
            if (_count == _items.Length) Resize(_items.Length * 2);
            _items[_count++] = item;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= _count) return false;
            for (int i = index; i < _count - 1; i++)
                _items[i] = _items[i + 1];
            _count--;
            _items[_count] = default!;
            return true;
        }

        public int IndexOf(Predicate<T> match)
        {
            for (int i = 0; i < _count; i++)
                if (match(_items[i])) return i;
            return -1;
        }

        public T[] ToArray()
        {
            var result = new T[_count];
            Array.Copy(_items, result, _count);
            return result;
        }

        private void Resize(int newCapacity)
        {
            var newArr = new T[newCapacity];
            Array.Copy(_items, newArr, _count);
            _items = newArr;
        }
    }
}
