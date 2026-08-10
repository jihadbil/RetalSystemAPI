using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.Models.Suppliers;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = null!;
        public string? Address { get; set; }

       public decimal OpeningBalance { get; set; }

    public ICollection<SupplierPhone> SupplierPhones { get; set; } = new List<SupplierPhone>();

    public ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();


}
