using System;

namespace SupermarketApp.DataStructures
{
    /// <summary>
    /// Custom (unbalanced) Binary Search Tree keyed by a comparable key, e.g. product
    /// title. Built without relying on SortedDictionary or any other built-in collection.
    /// Used to support alphabetical product-name search and sorted reporting.
    ///
    /// Time complexity:
    ///   Insert / TryGetValue / Remove - O(log n) average (balanced-ish tree),
    ///                                   O(n) worst case (degenerate tree)
    ///   InOrderValues / SearchByPrefix - O(n) (visits every node)
    /// </summary>
    public class BinarySearchTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        private class Node
        {
            public TKey Key;
            public TValue Value;
            public Node? Left;
            public Node? Right;
            public Node(TKey key, TValue value) { Key = key; Value = value; }
        }

        private Node? _root;
        private int _count;

        public int Count => _count;

        public void Insert(TKey key, TValue value)
        {
            _root = InsertRec(_root, key, value);
        }

        private Node InsertRec(Node? node, TKey key, TValue value)
        {
            if (node == null)
            {
                _count++;
                return new Node(key, value);
            }
            int cmp = key.CompareTo(node.Key);
            if (cmp < 0) node.Left = InsertRec(node.Left, key, value);
            else if (cmp > 0) node.Right = InsertRec(node.Right, key, value);
            else node.Value = value; // duplicate key -> update value
            return node;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var node = _root;
            while (node != null)
            {
                int cmp = key.CompareTo(node.Key);
                if (cmp == 0) { value = node.Value; return true; }
                node = cmp < 0 ? node.Left : node.Right;
            }
            value = default!;
            return false;
        }

        public bool Remove(TKey key)
        {
            bool removed = false;
            _root = RemoveRec(_root, key, ref removed);
            return removed;
        }

        private Node? RemoveRec(Node? node, TKey key, ref bool removed)
        {
            if (node == null) return null;
            int cmp = key.CompareTo(node.Key);
            if (cmp < 0)
            {
                node.Left = RemoveRec(node.Left, key, ref removed);
            }
            else if (cmp > 0)
            {
                node.Right = RemoveRec(node.Right, key, ref removed);
            }
            else
            {
                removed = true;
                _count--;
                if (node.Left == null) return node.Right;
                if (node.Right == null) return node.Left;

                // Two children: replace with in-order successor (smallest in right subtree)
                var successor = node.Right;
                while (successor.Left != null) successor = successor.Left;
                node.Key = successor.Key;
                node.Value = successor.Value;
                node.Right = RemoveMin(node.Right);
            }
            return node;
        }

        private Node? RemoveMin(Node? node)
        {
            if (node == null) return null;
            if (node.Left == null) return node.Right;
            node.Left = RemoveMin(node.Left);
            return node;
        }

        /// <summary>In-order traversal returns all values sorted by key. O(n).</summary>
        public TValue[] InOrderValues()
        {
            var result = new DynamicArray<TValue>(_count == 0 ? 4 : _count);
            InOrderRec(_root, result);
            return result.ToArray();
        }

        private void InOrderRec(Node? node, DynamicArray<TValue> result)
        {
            if (node == null) return;
            InOrderRec(node.Left, result);
            result.Add(node.Value);
            InOrderRec(node.Right, result);
        }

        /// <summary>
        /// Returns all values whose key (converted to a string via keyToString) starts
        /// with the given prefix, in alphabetical order. O(n) worst case.
        /// </summary>
        public TValue[] SearchByPrefix(string prefix, Func<TKey, string> keyToString)
        {
            var result = new DynamicArray<TValue>();
            CollectByPrefix(_root, prefix.ToLower(), keyToString, result);
            return result.ToArray();
        }

        private void CollectByPrefix(Node? node, string prefixLower, Func<TKey, string> keyToString, DynamicArray<TValue> result)
        {
            if (node == null) return;
            CollectByPrefix(node.Left, prefixLower, keyToString, result);
            if (keyToString(node.Key).ToLower().StartsWith(prefixLower))
                result.Add(node.Value);
            CollectByPrefix(node.Right, prefixLower, keyToString, result);
        }
    }
}
