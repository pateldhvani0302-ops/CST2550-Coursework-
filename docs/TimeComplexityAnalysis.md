# Time Complexity Analysis

This document summarises the time complexity of the key operations implemented
in the system, with pseudo-code, to support the coursework report's "Time
complexity analysis" section.

## 1. Adding a product

**Structure used:** `DynamicArray` (master list) + `HashTable` (barcode index) +
`BinarySearchTree` (title index).

```
function AddProduct(product):
    if HashTable.ContainsKey(product.barcode):
        throw "duplicate barcode"
    Database.Insert(product)          // O(1) write
    DynamicArray.Add(product)         // O(1) amortised
    HashTable.Add(product.barcode, product)   // O(1) average
    BinarySearchTree.Insert(product.title, product) // O(log n) average
```

**Complexity:** O(1) average (HashTable lookup + insert dominate; BST insert is
O(log n) average, O(n) worst case for a degenerate tree).

## 2. Barcode lookup

**Structure used:** `HashTable<string, Product>`.

```
function FindByBarcode(barcode):
    return HashTable.TryGetValue(barcode)
```

**Complexity:** O(1) average, O(n) worst case (all keys hashing to the same
bucket - extremely unlikely with a good hash function and load-factor-triggered
resizing, which is implemented).

## 3. Product-name search

Two implementations are provided, both discussed in the report:

**(a) Binary Search Tree exact-match (`ProductRepository.FindByExactTitle`)**
```
function FindByExactTitle(title):
    return BinarySearchTree.TryGetValue(title)
```
Complexity: O(log n) average, O(n) worst case (unbalanced tree).

**(b) Classic binary search on a sorted array (`SearchAlgorithms.BinarySearchByTitle`)**
```
function BinarySearch(sortedArray, title):
    low = 0, high = length - 1
    while low <= high:
        mid = (low + high) / 2
        if sortedArray[mid].title == title: return sortedArray[mid]
        else if sortedArray[mid].title < title: low = mid + 1
        else: high = mid - 1
    return not found
```
Complexity: O(log n), but requires the array to be sorted first
(insertion sort used here: O(n^2) worst case - acceptable for a small shop's
inventory size, and only needed once per search session in this design).

## 4. Stock update / restock

**Structure used:** `ProductRepository` (via `FindById`, O(n)) + `StockRepository`
append-only log (`DynamicArray`).

```
function Restock(productId, quantity):
    product = ProductRepository.FindById(productId)   // O(n)
    product.quantityInStock += quantity                // O(1)
    Database.Update(product)                            // O(1)
    StockLog.Add(new StockRecord(...))                  // O(1) amortised
```

**Complexity:** O(n) overall, dominated by the linear `FindById` scan. This is
an explicit, documented limitation/trade-off (see README §8) - acceptable given
the catalogue size expected for a small shop, and easily improved by adding a
second HashTable keyed by ProductId if required.

## 5. Recording a sale

**Structures used:** `ProductRepository`, `StockRepository`, `DynamicArray` (sale
items buffer).

```
function RecordSale(lines):
    for each (productId, quantity) in lines:
        product = ProductRepository.FindById(productId)    // O(n)
        if product.quantityInStock < quantity: throw "insufficient stock"
        saleItems.Add(new SaleItem(...))                    // O(1) amortised
    Database.Insert(sale)                                    // O(1)
    Database.InsertRange(saleItems)                          // O(k)
    for each item in saleItems:
        ProductRepository.UpdateStock(...)                   // O(n) (FindById)
        StockLog.Add(new StockRecord(...))                    // O(1) amortised
```

**Complexity:** O(k * n) where k = number of line items in the sale and n =
number of products (because each line item does an O(n) `FindById`). For a
typical small-shop sale (a handful of items against a catalogue of a few
hundred to a few thousand products) this remains fast in practice; it is the
same FindById trade-off discussed above.

## 6. Report generation

**Low-stock report:**
```
function LowStockReport():
    result = []
    for each product in DynamicArray:        // O(n)
        if product.status != InStock:
            result.Add(product)
    return result
```
Complexity: O(n).

**Sales-by-product report (aggregation):**
```
function SalesByProduct():
    totals = new HashTable<productId, (qty, revenue)>()
    for each sale in Sales:                   // O(s)
        for each item in sale.items:          // O(i) per sale
            totals.Add(item.productId, accumulate(item))   // O(1) average
    return totals joined with product titles  // O(n)
```
Complexity: O(s * i) = O(total sale items), i.e. effectively O(n) over the
total number of line items ever recorded, thanks to the HashTable's O(1)
average accumulation.

## 7. Summary table

| Operation                          | Data structure(s)            | Average case | Worst case |
|-------------------------------------|-------------------------------|--------------|------------|
| Add product                        | HashTable + BST + DynamicArray| O(1)         | O(n)       |
| Barcode lookup                     | HashTable                     | O(1)         | O(n)       |
| Exact name lookup                  | BinarySearchTree               | O(log n)     | O(n)       |
| Binary search (sorted array)       | Array + binary search          | O(log n)     | O(log n)   |
| Linear search (category/supplier)  | Array scan                     | O(n)         | O(n)       |
| Find by ID                         | DynamicArray scan               | O(n)         | O(n)       |
| Restock / adjust stock              | FindById + DynamicArray log     | O(n)         | O(n)       |
| Record a sale (k items)            | FindById (×k) + DynamicArray    | O(k·n)       | O(k·n)     |
| Low-stock report                   | DynamicArray scan               | O(n)         | O(n)       |
| Sales-by-product report            | HashTable aggregation           | O(total items)| O(total items)|
