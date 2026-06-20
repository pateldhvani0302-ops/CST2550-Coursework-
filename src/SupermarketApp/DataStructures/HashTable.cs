using System;

namespace SupermarketApp.DataStructures
{
    /// <summary>
    /// Custom hash table using separate chaining (singly-linked buckets), built without
    /// relying on Dictionary&lt;TKey, TValue&gt; or any other built-in collection.
    /// Used for O(1)-average lookups such as Product-by-barcode and Supplier-by-ID.
    ///
    /// Time complexity:
    ///   Add / TryGetValue / Remove - O(1) average, O(n) worst case (many collisions)
    ///   Resize (triggered automatically) - O(n)
    /// </summary>
    public class HashTable<TKey, TValue> where TKey : notnull
    {
        private class Node
        {
            public TKey Key;
            public TValue Value;
            public Node? Next;
            public Node(TKey key, TValue value) { Key = key; Value = value; }
        }

        private Node?[] _buckets;
        private int _count;
        private const double LoadFactorThreshold = 0.75;

        public HashTable(int capacity = 16)
        {
            _buckets = new Node?[capacity < 1 ? 16 : capacity];
            _count = 0;
        }

        public int Count => _count;

        private static int GetBucketIndex(TKey key, int bucketLength)
        {
            int hash = key.GetHashCode();
            int index = hash % bucketLength;
            return index < 0 ? index + bucketLength : index;
        }

        public void Add(TKey key, TValue value)
        {
            if ((double)_count / _buckets.Length >= LoadFactorThreshold)
                Resize();

            int index = GetBucketIndex(key, _buckets.Length);
            var node = _buckets[index];
            while (node != null)
            {
                if (node.Key.Equals(key))
                {
                    node.Value = value; // update existing key
                    return;
                }
                node = node.Next;
            }

            var newNode = new Node(key, value) { Next = _buckets[index] };
            _buckets[index] = newNode;
            _count++;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = GetBucketIndex(key, _buckets.Length);
            var node = _buckets[index];
            while (node != null)
            {
                if (node.Key.Equals(key))
                {
                    value = node.Value;
                    return true;
                }
                node = node.Next;
            }
            value = default!;
            return false;
        }

        public bool ContainsKey(TKey key) => TryGetValue(key, out _);

        public bool Remove(TKey key)
        {
            int index = GetBucketIndex(key, _buckets.Length);
            Node? node = _buckets[index];
            Node? prev = null;
            while (node != null)
            {
                if (node.Key.Equals(key))
                {
                    if (prev == null) _buckets[index] = node.Next;
                    else prev.Next = node.Next;
                    _count--;
                    return true;
                }
                prev = node;
                node = node.Next;
            }
            return false;
        }

        public TValue[] GetAllValues()
        {
            var result = new DynamicArray<TValue>(_count == 0 ? 4 : _count);
            foreach (var bucket in _buckets)
            {
                var node = bucket;
                while (node != null)
                {
                    result.Add(node.Value);
                    node = node.Next;
                }
            }
            return result.ToArray();
        }

        private void Resize()
        {
            var oldBuckets = _buckets;
            _buckets = new Node?[oldBuckets.Length * 2];
            _count = 0;
            foreach (var bucket in oldBuckets)
            {
                var node = bucket;
                while (node != null)
                {
                    Add(node.Key, node.Value);
                    node = node.Next;
                }
            }
        }
    }
}
