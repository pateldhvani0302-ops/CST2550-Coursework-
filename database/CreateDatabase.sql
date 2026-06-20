-- =====================================================================
-- Local Supermarket Management System - Database Creation Script
-- Target: Microsoft SQL Server
-- =====================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SupermarketDb')
BEGIN
    CREATE DATABASE SupermarketDb;
END
GO

USE SupermarketDb;
GO

-- Drop tables in dependency order if re-running this script
IF OBJECT_ID('dbo.SaleItems', 'U') IS NOT NULL DROP TABLE dbo.SaleItems;
IF OBJECT_ID('dbo.Sales', 'U') IS NOT NULL DROP TABLE dbo.Sales;
IF OBJECT_ID('dbo.StockRecords', 'U') IS NOT NULL DROP TABLE dbo.StockRecords;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Suppliers', 'U') IS NOT NULL DROP TABLE dbo.Suppliers;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
GO

CREATE TABLE dbo.Categories (
    CategoryId   INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(255) NULL
);
GO

CREATE TABLE dbo.Suppliers (
    SupplierId   INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(150) NOT NULL,
    ContactName  NVARCHAR(150) NOT NULL,
    Phone        NVARCHAR(30)  NULL,
    Email        NVARCHAR(150) NULL,
    Address      NVARCHAR(255) NULL
);
GO

CREATE TABLE dbo.Products (
    ProductId            INT IDENTITY(1,1) PRIMARY KEY,
    Title                NVARCHAR(150) NOT NULL,
    Barcode              NVARCHAR(64) NOT NULL,
    CategoryId           INT NOT NULL,
    SupplierId           INT NOT NULL,
    Price                DECIMAL(10,2) NOT NULL,
    QuantityInStock      INT NOT NULL DEFAULT 0,
    LowStockThreshold    INT NOT NULL DEFAULT 5,
    ExpiryOrRestockDate  DATE NOT NULL,
    CONSTRAINT UQ_Products_Barcode UNIQUE (Barcode),
    CONSTRAINT CK_Products_Price CHECK (Price > 0),
    CONSTRAINT CK_Products_QuantityInStock CHECK (QuantityInStock >= 0),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT FK_Products_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(SupplierId)
);
GO

CREATE TABLE dbo.StockRecords (
    StockRecordId  INT IDENTITY(1,1) PRIMARY KEY,
    ProductId      INT NOT NULL,
    MovementType   NVARCHAR(20) NOT NULL,   -- Restock | Sale | Adjustment
    QuantityChange INT NOT NULL,
    Timestamp      DATETIME NOT NULL DEFAULT GETDATE(),
    Notes          NVARCHAR(255) NULL,
    CONSTRAINT FK_StockRecords_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE TABLE dbo.Sales (
    SaleId       INT IDENTITY(1,1) PRIMARY KEY,
    SaleDate     DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount  DECIMAL(10,2) NOT NULL DEFAULT 0
);
GO

CREATE TABLE dbo.SaleItems (
    SaleItemId  INT IDENTITY(1,1) PRIMARY KEY,
    SaleId      INT NOT NULL,
    ProductId   INT NOT NULL,
    Quantity    INT NOT NULL,
    UnitPrice   DECIMAL(10,2) NOT NULL,
    CONSTRAINT CK_SaleItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT FK_SaleItems_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(SaleId),
    CONSTRAINT FK_SaleItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);
CREATE INDEX IX_Products_SupplierId ON dbo.Products(SupplierId);
CREATE INDEX IX_StockRecords_ProductId ON dbo.StockRecords(ProductId);
CREATE INDEX IX_SaleItems_SaleId ON dbo.SaleItems(SaleId);
CREATE INDEX IX_SaleItems_ProductId ON dbo.SaleItems(ProductId);
GO
