using Microsoft.EntityFrameworkCore;
using SupermarketApp.Models;

namespace SupermarketApp.Data
{
    public class SupermarketContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<StockRecord> StockRecords => Set<StockRecord>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();

        // Parameterless constructor used by the console app (falls back to OnConfiguring below).
        public SupermarketContext() { }

        // Used by tests / dependency injection to supply a different provider (e.g. InMemory).
        public SupermarketContext(DbContextOptions<SupermarketContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // EDIT THIS connection string to match your local SQL Server instance.
                // SQL Server Express example shown below - see README.md for alternatives
                // (LocalDB, full SQL Server, or a remote server).
                optionsBuilder.UseSqlServer(
    "Server=localhost;Database=SupermarketDb;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(e =>
            {
                e.HasKey(p => p.ProductId);
                e.HasIndex(p => p.Barcode).IsUnique();
                e.Property(p => p.Title).IsRequired().HasMaxLength(150);
                e.Property(p => p.Barcode).IsRequired().HasMaxLength(64);
                e.Property(p => p.Price).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.HasKey(c => c.CategoryId);
                e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Supplier>(e =>
            {
                e.HasKey(s => s.SupplierId);
                e.Property(s => s.Name).IsRequired().HasMaxLength(150);
            });

          modelBuilder.Entity<StockRecord>(e =>
   {
       e.HasKey(s => s.StockRecordId);
       // Store the enum as its text name ("Restock"/"Sale"/"Adjustment") so it
       // matches the NVARCHAR(20) column created by database/CreateDatabase.sql.
       // Without this, EF Core defaults to storing enums as integers, which
       // mismatches the text column and throws a cast error when reading rows back.
       e.Property(s => s.MovementType)
           .HasConversion<string>()
           .HasMaxLength(20);
   });

            modelBuilder.Entity<Sale>(e =>
            {
                e.HasKey(s => s.SaleId);
                e.Property(s => s.TotalAmount).HasColumnType("decimal(10,2)");
                e.Ignore(s => s.Items); // populated manually in-memory, not a mapped column
            });

            modelBuilder.Entity<SaleItem>(e =>
            {
                e.HasKey(si => si.SaleItemId);
                e.Property(si => si.UnitPrice).HasColumnType("decimal(10,2)");
            });
        }
    }
}
