using System;
using System.Collections.Generic;
using SupermarketApp.Models;
using SupermarketApp.Repositories;
using SupermarketApp.Search;
using SupermarketApp.Services;

namespace SupermarketApp.UI
{
    public class ConsoleMenu
    {
        private readonly ProductRepository _products;
        private readonly SupplierRepository _suppliers;
        private readonly CategoryRepository _categories;
        private readonly StockRepository _stock;
        private readonly SalesRepository _sales;
        private readonly ReportService _reports;

        public ConsoleMenu(ProductRepository products, SupplierRepository suppliers,
            CategoryRepository categories, StockRepository stock, SalesRepository sales,
            ReportService reports)
        {
            _products = products;
            _suppliers = suppliers;
            _categories = categories;
            _stock = stock;
            _sales = sales;
            _reports = reports;
        }

        public void Run()
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("===== Local Supermarket Management System =====");
                Console.WriteLine("1. Product management");
                Console.WriteLine("2. Stock management");
                Console.WriteLine("3. Supplier management");
                Console.WriteLine("4. Search products");
                Console.WriteLine("5. Record a sale");
                Console.WriteLine("6. Reports");
                Console.WriteLine("0. Exit");
                Console.Write("Select an option: ");
                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": ProductMenu(); break;
                        case "2": StockMenu(); break;
                        case "3": SupplierMenu(); break;
                        case "4": SearchMenu(); break;
                        case "5": RecordSaleMenu(); break;
                        case "6": ReportsMenu(); break;
                        case "0": running = false; break;
                        default: Console.WriteLine("Invalid option."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // ---------------- Products ----------------
        private void ProductMenu()
        {
            Console.WriteLine("\n-- Product Management --");
            Console.WriteLine("1. Add product");
            Console.WriteLine("2. Update product price/quantity");
            Console.WriteLine("3. Remove product");
            Console.WriteLine("4. List all products (alphabetical)");
            Console.Write("Select: ");
            switch (Console.ReadLine())
            {
                case "1": AddProduct(); break;
                case "2": UpdateProduct(); break;
                case "3": RemoveProduct(); break;
                case "4": ListAllProducts(); break;
            }
        }

        private void AddProduct()
        {
            Console.Write("Title: "); var title = Console.ReadLine() ?? "";
            Console.Write("Barcode: "); var barcode = Console.ReadLine() ?? "";
            Console.Write("Category ID: "); int categoryId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Supplier ID: "); int supplierId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Price: "); decimal price = decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Quantity in stock: "); int qty = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Low stock threshold (default 5): ");
            var thresholdInput = Console.ReadLine();
            int threshold = string.IsNullOrWhiteSpace(thresholdInput) ? 5 : int.Parse(thresholdInput);
            Console.Write("Expiry/restock date (yyyy-MM-dd): ");
            var dateInput = Console.ReadLine();
            var date = string.IsNullOrWhiteSpace(dateInput) ? DateTime.Today : DateTime.Parse(dateInput);

            var product = new Product
            {
                Title = title,
                Barcode = barcode,
                CategoryId = categoryId,
                SupplierId = supplierId,
                Price = price,
                QuantityInStock = qty,
                LowStockThreshold = threshold,
                ExpiryOrRestockDate = date
            };

            ValidationService.ValidateProduct(product, _products, isNew: true);
            _products.Add(product);
            Console.WriteLine($"Product added with ID {product.ProductId}.");
        }

        private void UpdateProduct()
        {
            Console.Write("Product ID to update: ");
            int id = int.Parse(Console.ReadLine() ?? "0");
            var product = _products.FindById(id);
            if (product == null) { Console.WriteLine("Product not found."); return; }

            Console.Write($"New price (blank to keep {product.Price:C}): ");
            var priceInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(priceInput)) product.Price = decimal.Parse(priceInput);

            Console.Write($"New quantity (blank to keep {product.QuantityInStock}): ");
            var qtyInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(qtyInput)) product.QuantityInStock = int.Parse(qtyInput);

            ValidationService.ValidateProduct(product, _products, isNew: false);
            _products.UpdateStock(product.ProductId, product.QuantityInStock);
            Console.WriteLine("Product updated.");
        }

        private void RemoveProduct()
        {
            Console.Write("Product ID to remove: ");
            int id = int.Parse(Console.ReadLine() ?? "0");
            Console.WriteLine(_products.Remove(id) ? "Removed." : "Product not found.");
        }

        private void ListAllProducts()
        {
            foreach (var p in _products.GetAllSortedByTitle())
                Console.WriteLine(p);
        }

        // ---------------- Stock ----------------
        private void StockMenu()
        {
            Console.WriteLine("\n-- Stock Management --");
            Console.WriteLine("1. Restock product");
            Console.WriteLine("2. Adjust stock (correction)");
            Console.WriteLine("3. View low-stock items");
            Console.WriteLine("4. View stock history for a product");
            Console.Write("Select: ");
            switch (Console.ReadLine())
            {
                case "1": Restock(); break;
                case "2": AdjustStock(); break;
                case "3": ViewLowStock(); break;
                case "4": StockHistory(); break;
            }
        }

        private void Restock()
        {
            Console.Write("Product ID: "); int id = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Quantity to add: "); int qty = int.Parse(Console.ReadLine() ?? "0");
            _stock.Restock(id, qty);
            Console.WriteLine("Stock updated.");
        }

        private void AdjustStock()
        {
            Console.Write("Product ID: "); int id = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Quantity change (negative for a reduction): ");
            int delta = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Notes: "); var notes = Console.ReadLine();
            _stock.Adjust(id, delta, notes);
            Console.WriteLine("Stock adjusted.");
        }

        private void ViewLowStock()
        {
            foreach (var p in _products.GetLowStock())
                Console.WriteLine(p);
        }

        private void StockHistory()
        {
            Console.Write("Product ID: "); int id = int.Parse(Console.ReadLine() ?? "0");
            foreach (var r in _stock.GetHistoryForProduct(id))
                Console.WriteLine($"{r.Timestamp:g} | {r.MovementType} | Change: {r.QuantityChange} | {r.Notes}");
        }

        // ---------------- Suppliers ----------------
        private void SupplierMenu()
        {
            Console.WriteLine("\n-- Supplier Management --");
            Console.WriteLine("1. Add supplier");
            Console.WriteLine("2. List suppliers");
            Console.WriteLine("3. Remove supplier");
            Console.WriteLine("4. Update supplier");
            Console.Write("Select: ");
            switch (Console.ReadLine())
            {
                case "1": AddSupplier(); break;
                case "2": ListSuppliers(); break;
                case "3": RemoveSupplier(); break;
                case "4": UpdateSupplier(); break;
            }
        }

        private void AddSupplier()
        {
            var supplier = new Supplier();
            Console.Write("Name: "); supplier.Name = Console.ReadLine() ?? "";
            Console.Write("Contact name: "); supplier.ContactName = Console.ReadLine() ?? "";
            Console.Write("Phone: "); supplier.Phone = Console.ReadLine() ?? "";
            Console.Write("Email: "); supplier.Email = Console.ReadLine() ?? "";
            Console.Write("Address: "); supplier.Address = Console.ReadLine() ?? "";

            ValidationService.ValidateSupplier(supplier);
            _suppliers.Add(supplier);
            Console.WriteLine($"Supplier added with ID {supplier.SupplierId}.");
        }

        private void ListSuppliers()
        {
            foreach (var s in _suppliers.GetAll())
                Console.WriteLine($"[{s.SupplierId}] {s.Name} | {s.ContactName} | {s.Phone} | {s.Email}");
        }

       private void RemoveSupplier()
        {
            Console.Write("Supplier ID: "); int id = int.Parse(Console.ReadLine() ?? "0");
            Console.WriteLine(_suppliers.Remove(id) ? "Removed." : "Supplier not found.");
        }

        private void UpdateSupplier()
        {
            Console.Write("Supplier ID to update: ");
            int id = int.Parse(Console.ReadLine() ?? "0");
            var existing = _suppliers.FindById(id);
            if (existing == null) { Console.WriteLine("Supplier not found."); return; }

            Console.Write($"Name (blank to keep '{existing.Name}'): ");
            var name = Console.ReadLine();
            Console.Write($"Contact name (blank to keep '{existing.ContactName}'): ");
            var contactName = Console.ReadLine();
            Console.Write($"Phone (blank to keep '{existing.Phone}'): ");
            var phone = Console.ReadLine();
            Console.Write($"Email (blank to keep '{existing.Email}'): ");
            var email = Console.ReadLine();
            Console.Write($"Address (blank to keep '{existing.Address}'): ");
            var address = Console.ReadLine();

            _suppliers.Update(
                id,
                string.IsNullOrWhiteSpace(name) ? existing.Name : name,
                string.IsNullOrWhiteSpace(contactName) ? existing.ContactName : contactName,
                string.IsNullOrWhiteSpace(phone) ? existing.Phone : phone,
                string.IsNullOrWhiteSpace(email) ? existing.Email : email,
                string.IsNullOrWhiteSpace(address) ? existing.Address : address);

            Console.WriteLine("Supplier updated.");
        }

        // ---------------- Search ----------------
        private void SearchMenu()
        {
            Console.WriteLine("\n-- Search Products --");
            Console.WriteLine("1. By barcode (custom HashTable, O(1) average)");
            Console.WriteLine("2. By exact name (custom Binary Search Tree, O(log n) average)");
            Console.WriteLine("3. By name prefix (Binary Search Tree scan)");
            Console.WriteLine("4. By category (linear search, O(n))");
            Console.WriteLine("5. By supplier (linear search, O(n))");
            Console.WriteLine("6. By exact name (classic binary search on sorted array, O(log n))");
            Console.Write("Select: ");
            switch (Console.ReadLine())
            {
                case "1":
                    Console.Write("Barcode: ");
                    var byBarcode = _products.FindByBarcode(Console.ReadLine() ?? "");
                    Console.WriteLine(byBarcode?.ToString() ?? "Not found.");
                    break;
                case "2":
                    Console.Write("Exact title: ");
                    var exact = _products.FindByExactTitle(Console.ReadLine() ?? "");
                    Console.WriteLine(exact?.ToString() ?? "Not found.");
                    break;
                case "3":
                    Console.Write("Name prefix: ");
                    foreach (var p in _products.SearchByTitlePrefix(Console.ReadLine() ?? ""))
                        Console.WriteLine(p);
                    break;
                case "4":
                    Console.Write("Category ID: ");
                    int catId = int.Parse(Console.ReadLine() ?? "0");
                    foreach (var p in SearchAlgorithms.LinearSearchByCategory(_products.GetAll(), catId))
                        Console.WriteLine(p);
                    break;
                case "5":
                    Console.Write("Supplier ID: ");
                    int supId = int.Parse(Console.ReadLine() ?? "0");
                    foreach (var p in SearchAlgorithms.LinearSearchBySupplier(_products.GetAll(), supId))
                        Console.WriteLine(p);
                    break;
                case "6":
                    Console.Write("Exact title to find: ");
                    var sorted = _products.GetAll();
                    SearchAlgorithms.SortByTitle(sorted);
                    var found = SearchAlgorithms.BinarySearchByTitle(sorted, Console.ReadLine() ?? "");
                    Console.WriteLine(found?.ToString() ?? "Not found.");
                    break;
            }
        }

        // ---------------- Sales ----------------
        private void RecordSaleMenu()
        {
            Console.WriteLine("\n-- Record a Sale --");
            // Note: a List<T> is used here only as a transient UI input buffer while the
            // operator types in line items - it is NOT part of the system's core data
            // structures. The sale itself is stored via DynamicArray + EF/SQL Server.
            var lines = new List<(int productId, int quantity)>();
            while (true)
            {
                Console.Write("Product ID (blank to finish): ");
                var idInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(idInput)) break;
                Console.Write("Quantity: ");
                int qty = int.Parse(Console.ReadLine() ?? "0");
                lines.Add((int.Parse(idInput), qty));
            }

            if (lines.Count == 0) { Console.WriteLine("No items entered."); return; }

            var sale = _sales.RecordSale(lines.ToArray());
            Console.WriteLine($"Sale #{sale.SaleId} recorded. Total: {sale.TotalAmount:C}");
        }

        // ---------------- Reports ----------------
        private void ReportsMenu()
        {
            Console.WriteLine("\n-- Reports --");
            Console.WriteLine("1. Low-stock items");
            Console.WriteLine("2. Products by category");
            Console.WriteLine("3. Supplier stock list");
            Console.WriteLine("4. Sales by product");
            Console.Write("Select: ");
            switch (Console.ReadLine())
            {
                case "1":
                    foreach (var p in _reports.LowStockReport()) Console.WriteLine(p);
                    break;
                case "2":
                    Console.Write("Category ID: ");
                    int catId = int.Parse(Console.ReadLine() ?? "0");
                    foreach (var p in _reports.ProductsByCategory(catId)) Console.WriteLine(p);
                    break;
                case "3":
                    Console.Write("Supplier ID: ");
                    int supId = int.Parse(Console.ReadLine() ?? "0");
                    foreach (var p in _reports.SupplierStockList(supId)) Console.WriteLine(p);
                    break;
                case "4":
                    foreach (var row in _reports.SalesByProduct())
                        Console.WriteLine($"[{row.productId}] {row.title} | Sold: {row.totalQuantitySold} | Revenue: {row.totalRevenue:C}");
                    break;
            }
        }
    }
}
