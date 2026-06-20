# Supermarket Management System

Here's a walkthrough of how this project is organised, why it's built the way it is, and how to get it up and running on your own machine.

## Project structure

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

## Design overview

The basic idea was to keep the **algorithmic, in-memory side of things** separate from **how everything gets saved**, rather than mixing the two together.

- All the core storage — products, suppliers, stock and sales logs — runs on data structures I wrote myself in `DataStructures/`: a `DynamicArray<T>`, a `HashTable<TKey, TValue>` (using separate chaining), and a `BinarySearchTree<TKey, TValue>`. None of this relies on built-in collections like `List<T>` or `Dictionary<TKey, TValue>` — that was a deliberate choice so the coursework's "custom data structure" requirement is actually met, not just gestured at.
  - The `HashTable` gives O(1) average-case lookups for barcodes and supplier IDs.
  - The `BinarySearchTree` gives O(log n) average lookups by product name, and you also get a free alphabetically-sorted traversal out of it for listings and reports.
  - The `DynamicArray` is the master list and doubles as the append-only log for stock movements — O(1) amortised appends.
- On top of that sits **SQL Server with EF Core** (`Data/SupermarketContext.cs`), so nothing is lost between runs. The repositories pull everything into the custom structures when the app starts, then write through to the database whenever something changes.
- One small cheat: there's a `List<(int,int)>` used purely as a scratch buffer while the operator is typing in sale line items, in `ConsoleMenu.RecordSaleMenu`. It's just UI input handling, not part of the actual data model, so I didn't see the point in reinventing it there.
- For search, `Search/SearchAlgorithms.cs` has a proper textbook **binary search** (O(log n), needs a sorted array) and a **linear search** (O(n)) sitting alongside the HashTable/BST lookups already used in `ProductRepository`. That covers the "at least two search algorithms" requirement and gives a few different approaches worth comparing in the report.

## Database setup

1. Open `database/CreateDatabase.sql` in SQL Server Management Studio (or run it via `sqlcmd`) against your SQL Server instance. This sets up the `SupermarketDb` database and all the tables (`Categories`, `Suppliers`, `Products`, `StockRecords`, `Sales`, `SaleItems`).
2. If you want, run `database/SeedData.sql` too, to load some sample data directly via SQL. *(You don't have to — the app will seed the same sample data itself through EF Core the first time it runs against an empty database; see the next step.)*
3. Update the connection string in `src/SupermarketApp/Data/SupermarketContext.cs` (inside `OnConfiguring`) so it points at your SQL Server instance, e.g.:
   ```csharp
   optionsBuilder.UseSqlServer(
       "Server=localhost\\SQLEXPRESS;Database=SupermarketDb;Trusted_Connection=True;TrustServerCertificate=True;");
   ```
   - For LocalDB: `Server=(localdb)\\MSSQLLocalDB;Database=SupermarketDb;Trusted_Connection=True;`
   - For SQL auth: `Server=YOUR_SERVER;Database=SupermarketDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;`

## Build and run

```bash
cd SupermarketManagementSystem
dotnet restore
dotnet build
dotnet run --project src/SupermarketApp/SupermarketApp.csproj
```

The first time you run it against an empty database, `SeedData.EnsureSeeded` will create the tables (if they're not there yet) and drop in some sample categories, suppliers and products automatically — so honestly, you can skip the database setup step above entirely and just run it straight away.

Once it's running, you'll land on a console menu:

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

## Running the tests

```bash
dotnet test src/SupermarketApp.Tests/SupermarketApp.Tests.csproj
```

These run against EF Core's **InMemory** provider, so you don't need a live SQL Server connection just to test things. They cover:
- The custom data structures (`DynamicArray`, `HashTable`, `BinarySearchTree`)
- The classic search algorithms (binary search, linear search)
- Product repository behaviour — adding products, catching duplicate barcodes, flagging low stock, looking up by name
- Sales repository behaviour — recording a sale, handling insufficient stock, logging stock movements
- Validation rules — pricing, barcode uniqueness, supplier contact details

## A suggested walkthrough (handy for the demo video)

1. Add a new product (Product management → Add product).
2. Look it up by barcode, then by exact name (Search products → options 1 and 2).
3. Restock it, then check its stock history (Stock management → options 1 and 4).
4. Record a sale for it and watch the quantity drop.
5. Drop its quantity below the low-stock threshold (Stock management → Adjust stock) and confirm it shows up in the low-stock report (Reports → option 1).
6. Run the sales-by-product report and check the totals reflect your sale.

## Known limitations / things I'd improve given more time

- There's no GUI — it's a console app, as the brief asks for, though WinForms or a Web API would both be reasonable alternatives if you'd rather go a different route.
- `ProductRepository.FindById` is O(n) since there's no dedicated ID index. That's fine for a small shop's catalogue, but a second `HashTable<int, Product>` would fix it if it ever needed to scale.
- The `BinarySearchTree` isn't self-balancing, so a bad insertion order (say, product names added in alphabetical order) would degrade it toward O(n) per operation. An AVL or Red-Black tree would guarantee O(log n), but felt like more complexity than this project needed.
- No receipt printing, no barcode-scanner integration, no customer accounts — these are flagged as future work in the report rather than built here.

## Where the rest of your deliverables go

There's room left in the repo structure for:
- `report.pdf` (or `.docx`) — the 5-page report (see the coursework brief for what sections it needs)
- `video-demo.mp4` (or a link to it) — the demo video, max 6 minutes
