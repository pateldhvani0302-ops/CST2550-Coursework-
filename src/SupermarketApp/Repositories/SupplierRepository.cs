using System.Linq;
using SupermarketApp.Data;
using SupermarketApp.DataStructures;
using SupermarketApp.Models;

namespace SupermarketApp.Repositories
{
    /// <summary>
    /// Manages suppliers using a custom HashTable keyed by SupplierId for O(1)
    /// average lookup, backed by SQL Server via EF Core for persistence.
    /// </summary>
    public class SupplierRepository
    {
        private readonly SupermarketContext _context;
        private readonly HashTable<int, Supplier> _byId = new();
        private readonly DynamicArray<Supplier> _all = new();

        public SupplierRepository(SupermarketContext context)
        {
            _context = context;
            foreach (var s in _context.Suppliers.ToList())
            {
                _all.Add(s);
                _byId.Add(s.SupplierId, s);
            }
        }

        public void Add(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            _context.SaveChanges();
            _all.Add(supplier);
            _byId.Add(supplier.SupplierId, supplier);
        }

        /// <summary>O(1) average - HashTable lookup.</summary>
        public Supplier? FindById(int id) => _byId.TryGetValue(id, out var s) ? s : null;

        public Supplier[] GetAll() => _all.ToArray();

        /// <summary>
        /// Updates an existing supplier's contact details. O(1) average (HashTable
        /// lookup already gave us the tracked-equivalent object reference) + O(1) DB write.
        /// </summary>
        public void Update(int id, string name, string contactName, string phone, string email, string address)
        {
            var supplier = FindById(id)
                ?? throw new System.InvalidOperationException("Supplier not found.");

            supplier.Name = name;
            supplier.ContactName = contactName;
            supplier.Phone = phone;
            supplier.Email = email;
            supplier.Address = address;

            _context.Suppliers.Update(supplier);
            _context.SaveChanges();
        }

        public bool Remove(int id)
        {
            var supplier = FindById(id);
            if (supplier == null) return false;

            _context.Suppliers.Remove(supplier);
            _context.SaveChanges();

            _byId.Remove(id);
            int idx = _all.IndexOf(s => s.SupplierId == id);
            if (idx >= 0) _all.RemoveAt(idx);
            return true;
        }
    }
}