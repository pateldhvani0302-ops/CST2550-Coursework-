using System;
using SupermarketApp.Models;

namespace SupermarketApp.Search
{
    /// <summary>
    /// Classic textbook search algorithms used across the system. These are kept
    /// distinct from the custom HashTable/BinarySearchTree data-structure lookups
    /// in the repositories, to satisfy the "at least two different search algorithms"
    /// requirement explicitly (binary search and linear search).
    /// </summary>
    public static class SearchAlgorithms
    {
        /// <summary>
        /// Binary search for a product by exact title. Requires the array to already
        /// be sorted by Title (use SortByTitle first). O(log n).
        /// </summary>
        public static Product? BinarySearchByTitle(Product[] sortedByTitle, string title)
        {
            int low = 0, high = sortedByTitle.Length - 1;
            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                int cmp = string.Compare(sortedByTitle[mid].Title, title, StringComparison.OrdinalIgnoreCase);
                if (cmp == 0) return sortedByTitle[mid];
                if (cmp < 0) low = mid + 1;
                else high = mid - 1;
            }
            return null;
        }

        /// <summary>Linear search across all products by category. O(n).</summary>
        public static Product[] LinearSearchByCategory(Product[] products, int categoryId)
        {
            var result = new SupermarketApp.DataStructures.DynamicArray<Product>();
            foreach (var p in products)
                if (p.CategoryId == categoryId) result.Add(p);
            return result.ToArray();
        }

        /// <summary>Linear search across all products by supplier. O(n).</summary>
        public static Product[] LinearSearchBySupplier(Product[] products, int supplierId)
        {
            var result = new SupermarketApp.DataStructures.DynamicArray<Product>();
            foreach (var p in products)
                if (p.SupplierId == supplierId) result.Add(p);
            return result.ToArray();
        }

        /// <summary>
        /// Simple insertion sort by Title, used to prepare an array for BinarySearchByTitle.
        /// O(n^2) worst case - acceptable for the size of inventory a small shop holds.
        /// </summary>
        public static void SortByTitle(Product[] products)
        {
            for (int i = 1; i < products.Length; i++)
            {
                var key = products[i];
                int j = i - 1;
                while (j >= 0 && string.Compare(products[j].Title, key.Title, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    products[j + 1] = products[j];
                    j--;
                }
                products[j + 1] = key;
            }
        }
    }
}
