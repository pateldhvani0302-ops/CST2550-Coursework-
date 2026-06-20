using System;
using SupermarketApp.Models;
using SupermarketApp.Repositories;

namespace SupermarketApp.Services
{
    /// <summary>Validation rules for supermarket data, applied before persistence.</summary>
    public static class ValidationService
    {
        public static void ValidateProduct(Product product, ProductRepository repository, bool isNew)
        {
            if (string.IsNullOrWhiteSpace(product.Title))
                throw new ArgumentException("Product title is required.");

            if (string.IsNullOrWhiteSpace(product.Barcode))
                throw new ArgumentException("Barcode is required.");

            if (isNew && repository.FindByBarcode(product.Barcode) != null)
                throw new ArgumentException($"Barcode '{product.Barcode}' is already in use.");

            if (product.Price <= 0)
                throw new ArgumentException("Price must be a positive value.");

            if (product.QuantityInStock < 0)
                throw new ArgumentException("Quantity in stock cannot be negative.");

            if (product.ExpiryOrRestockDate < DateTime.Today.AddYears(-1))
                throw new ArgumentException("Expiry/restock date does not look valid.");
        }

        public static void ValidateSupplier(Supplier supplier)
        {
            if (string.IsNullOrWhiteSpace(supplier.Name))
                throw new ArgumentException("Supplier name is required.");

            if (string.IsNullOrWhiteSpace(supplier.ContactName))
                throw new ArgumentException("Supplier contact name is required.");

            if (string.IsNullOrWhiteSpace(supplier.Phone) && string.IsNullOrWhiteSpace(supplier.Email))
                throw new ArgumentException("At least one contact method (phone or email) is required.");
        }
    }
}
