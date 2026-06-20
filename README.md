Database setup


Open database/CreateDatabase.sql in SQL Server Management Studio (or run it via sqlcmd) against your SQL Server instance. This creates the SupermarketDb database and all tables (Categories, Suppliers, Products, StockRecords, Sales, SaleItems).
Optionally, run database/SeedData.sql to load sample data via SQL. (Not required — the app seeds the same sample data automatically through EF Core the first time it runs against an empty database; see the Build and run section below.)
Update the connection string in src/SupermarketApp/Data/SupermarketContext.cs (inside OnConfiguring) to match your SQL Server instance, e.g.:


csharp   optionsBuilder.UseSqlServer(
       "Server=localhost\\SQLEXPRESS;Database=SupermarketDb;Trusted_Connection=True;TrustServerCertificate=True;");


For LocalDB: Server=(localdb)\\MSSQLLocalDB;Database=SupermarketDb;Trusted_Connection=True;
For SQL auth: Server=YOUR_SERVER;Database=SupermarketDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;


Build and run

bashcd SupermarketManagementSystem
dotnet restore
dotnet build
dotnet run --project src/SupermarketApp/SupermarketApp.csproj

On first run against an empty database, the app will automatically create the tables (if they don't already exist) and insert sample categories, suppliers and products — so you can skip the database setup step above entirely and just run it directly.

Usage

Once running, you'll see a console menu:

===== Local Supermarket Management System =====
1. Product management
2. Stock management
3. Supplier management
4. Search products
5. Record a sale
6. Reports
0. Exit


Product management — add, edit, or remove products.
Stock management — restock items, adjust quantities, and view stock history.
Supplier management — add and manage supplier records.
Search products — search by barcode or by exact product name.
Record a sale — log a sale and automatically update stock quantities.
Reports — view low-stock items and sales totals by product
