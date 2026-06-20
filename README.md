# Local Supermarket Management System for Small Shops

CST2550 Reset Coursework - C# .NET console application for managing products,
stock, suppliers, sales and reports in a small supermarket.

## 1. Project structure

```
SupermarketManagementSystem/
├── SupermarketManagementSystem.sln
├── src/
│   ├── SupermarketApp/                 # Console application (main project)
│   │   ├── Models/                     # Product, Category, Supplier, StockRecord, Sale, SaleItem
│   │   ├── DataStructures/              # Custom DynamicArray, HashTable, BinarySearchTree
│   │   ├── Search/                      # Classic SearchAlgorithms (binary search, linear search)
│   │   ├── Data/                        # EF Core SupermarketContext + SeedData
│   │   ├── Repositories/                # Product/Supplier/Category/Stock/Sales repositories
│   │   ├── Services/                    # ValidationService, ReportService
│   │   ├── UI/                          # ConsoleMenu
│   │   └── Program.cs                   # Entry point
│   └── SupermarketApp.Tests/           # xUnit test project
├── database/
│   ├── CreateDatabase.sql               # Table creation script
│   └── SeedData.sql                     # Sample data script
└── docs/
    └── TimeComplexityAnalysis.md        # Time complexity notes for the report
```

## 2. Design overview

The system separates **in-memory algorithmic structures** from **persistence**:

- **Custom data structures** (`DataStructures/`) - a hand-written `DynamicArray<T>`,
  `HashTable<TKey, TValue>` (separate chaining) and `BinarySearchTree<TKey, TValue>` -
  are used as the *main* data structures for products, suppliers and stock/sales
  logs. No built-in collection (`List<T>`, `Dictionary<TKey, TValue>`, etc.) is used
  for this core storage, satisfying the "custom data structure" requirement.
  - `HashTable` → O(1) average barcode lookup and supplier-ID lookup.
  - `BinarySearchTree` → O(log n) average exact product-name lookup, plus O(n)
    sorted (alphabetical) traversal for listings/reports.
  - `DynamicArray` → O(1) amortised append, used as the master list and as the
    append-only stock-movement log.
- **SQL Server + Entity Framework Core** (`Data/SupermarketContext.cs`) persists
  every change so data survives between runs. Repositories load all rows into the
  custom structures on startup and write through to the database on every change.
- One small exception: a `List<(int,int)>` is used purely as a *transient UI input
  buffer* while the operator types in sale line items in `ConsoleMenu.RecordSaleMenu`
  - it is not part of the system's core data structures.
- **Search algorithms** (`Search/SearchAlgorithms.cs`) explicitly implement a
  classic **binary search** (O(log n), requires a sorted array) and a **linear
  search** (O(n)), in addition to the HashTable/BST-based lookups in
  `ProductRepository`, so the "at least two different search algorithms"
  requirement is met with several approaches to compare/discuss in the report.

## 3. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express, Developer, LocalDB, or a remote instance)
- (Optional) Visual Studio 2022 / VS Code with the C# extension

> **Note:** This code was written and reviewed manually but could not be compiled
> in the environment it was produced in (no internet access to NuGet). Please run
> a full `dotnet restore` and `dotnet build` yourself before your first run, and
> double check for any small build errors.

## 4. Database setup

1. Open `database/CreateDatabase.sql` in SQL Server Management Studio (or `sqlcmd`)
   and execute it against your SQL Server instance. This creates the `SupermarketDb`
   database and all tables (`Categories`, `Suppliers`, `Products`, `StockRecords`,
   `Sales`, `SaleItems`).
2. Optionally run `database/SeedData.sql` to load sample data via plain SQL.
   *(Alternatively, the app seeds the same sample data automatically via EF Core
   the first time it runs against an empty database - see step 5.)*
3. Update the connection string in
   `src/SupermarketApp/Data/SupermarketContext.cs` (`OnConfiguring`) to match your
   SQL Server instance, e.g.:
   ```csharp
   optionsBuilder.UseSqlServer(
       "Server=localhost\\SQLEXPRESS;Database=SupermarketDb;Trusted_Connection=True;TrustServerCertificate=True;");
   ```
   - For LocalDB: `Server=(localdb)\\MSSQLLocalDB;Database=SupermarketDb;Trusted_Connection=True;`
   - For SQL auth: `Server=YOUR_SERVER;Database=SupermarketDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;`

## 5. Build and run

```bash
cd SupermarketManagementSystem
dotnet restore
dotnet build
dotnet run --project src/SupermarketApp/SupermarketApp.csproj
```

On first run against an empty database, `SeedData.EnsureSeeded` will create the
tables (if they don't already exist) and insert sample categories, suppliers and
products automatically, so you can also skip step 4 entirely and just `dotnet run`.

You'll see a console menu:

```
===== Local Supermarket Management System =====
1. Product management
2. Stock management
3. Supplier management
4. Search products
5. Record a sale
6. Reports
0. Exit
```

## 6. Run the unit tests

```bash
dotnet test src/SupermarketApp.Tests/SupermarketApp.Tests.csproj
```

The test project uses the EF Core **InMemory** provider, so tests do not require
a real SQL Server connection. Tests cover:
- Custom data structures (`DynamicArray`, `HashTable`, `BinarySearchTree`)
- Classic search algorithms (binary search, linear search)
- Product repository operations (add, duplicate-barcode validation, low-stock
  detection, name lookup)
- Sales repository operations (recording a sale, insufficient-stock handling,
  stock-movement logging)
- Validation rules (price, barcode uniqueness, supplier contact details)

## 7. Suggested manual test walkthrough (for your video demo)

1. Add a new product (Product management → Add product).
2. Search for it by barcode, then by exact name (Search products → options 1 and 2).
3. Restock it, then view its stock history (Stock management → options 1 and 4).
4. Record a sale for it and watch the stock quantity drop.
5. Reduce its quantity below the low-stock threshold (Stock management → Adjust
   stock) and confirm it appears in the low-stock report (Reports → option 1).
6. Run the sales-by-product report to see totals update after your sale.

## 8. Known limitations / possible future improvements

- No GUI - this is a console app per the technical requirements (WinForms/Web API
  are valid alternatives if you'd prefer a different interface).
- `ProductRepository.FindById` is O(n) (no dedicated ID index); fine for a small
  shop's catalogue, but could be upgraded with a second `HashTable<int, Product>`.
- The `BinarySearchTree` is unbalanced, so a pathological insert order (e.g.
  alphabetically sorted product names) degrades it to O(n) per operation; an
  AVL/Red-Black tree would guarantee O(log n) but adds complexity.
- No receipt printing, barcode-scanner hardware integration, or customer
  accounts - listed as suggested future work in the report.

## 9. Where to put the rest of your deliverables

This repository structure leaves room for:
- `report.pdf` (or `.docx`) - your 5-page report (see coursework brief for sections)
- `video-demo.mp4` (or a link to it) - your max 6-minute demo video
