-- =====================================================================
-- Sample seed data for the Local Supermarket Management System
-- Run after CreateDatabase.sql
-- =====================================================================

USE SupermarketDb;
GO

INSERT INTO dbo.Categories (Name, Description) VALUES
('Dairy', 'Milk, cheese, yoghurt'),
('Bakery', 'Bread and baked goods'),
('Drinks', 'Soft drinks and juices');
GO

INSERT INTO dbo.Suppliers (Name, ContactName, Phone, Email, Address) VALUES
('Fresh Farms Ltd', 'Lisa Grant', '020 7946 0958', 'sales@freshfarms.co.uk', '12 Market St, London'),
('Golden Bakery Co', 'Tom Reid', '020 7946 0123', 'orders@goldenbakery.co.uk', '5 Mill Lane, London');
GO

INSERT INTO dbo.Products (Title, Barcode, CategoryId, SupplierId, Price, QuantityInStock, LowStockThreshold, ExpiryOrRestockDate) VALUES
('Whole Milk 1L', '5000112637922', 1, 1, 1.20, 40, 10, DATEADD(DAY, 10, GETDATE())),
('Cheddar Cheese 200g', '5000112637939', 1, 1, 2.50, 25, 8, DATEADD(DAY, 20, GETDATE())),
('White Bread Loaf', '5000112637946', 2, 2, 1.10, 4, 10, DATEADD(DAY, 3, GETDATE())),
('Orange Juice 1L', '5000112637953', 3, 1, 1.80, 0, 5, DATEADD(DAY, 15, GETDATE()));
GO

-- Sample sales transaction for demonstration/testing
INSERT INTO dbo.Sales (SaleDate, TotalAmount) VALUES (GETDATE(), 3.70);
GO

INSERT INTO dbo.SaleItems (SaleId, ProductId, Quantity, UnitPrice) VALUES
(1, 1, 1, 1.20),
(1, 2, 1, 2.50);
GO
