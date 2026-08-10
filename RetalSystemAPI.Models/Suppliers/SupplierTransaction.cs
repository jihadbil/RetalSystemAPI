using System;
using RetalSystemAPI.Models.Common;    
using RetalSystemAPI.Models.Enums;


namespace RetalSystemAPI.Models.Suppliers;

public class SupplierTransaction:BaseEntity
{
 
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } 
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public SupplierTransactionType Type { get; set; }
    public decimal Amount { get; set; } // Positive = debt (due to supplier), Negative = payment (paid to supplier)

    //public long? ReferenceId { get; set; }
    //public string? ReferenceType { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
 
 
}
