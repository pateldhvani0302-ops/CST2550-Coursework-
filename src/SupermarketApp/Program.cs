using SupermarketApp.Data;
using SupermarketApp.Repositories;
using SupermarketApp.Services;
using SupermarketApp.UI;

var context = new SupermarketContext();
SeedData.EnsureSeeded(context);

var productRepository = new ProductRepository(context);
var supplierRepository = new SupplierRepository(context);
var categoryRepository = new CategoryRepository(context);
var stockRepository = new StockRepository(context, productRepository);
var salesRepository = new SalesRepository(context, productRepository, stockRepository);
var reportService = new ReportService(productRepository, supplierRepository, categoryRepository, salesRepository);

var menu = new ConsoleMenu(productRepository, supplierRepository, categoryRepository,
    stockRepository, salesRepository, reportService);
menu.Run();
