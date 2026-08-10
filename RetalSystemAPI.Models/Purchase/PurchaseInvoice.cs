using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;


namespace RetalSystemAPI.Models.Purchase;

public class PurchaseInvoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = null!;
    public int SupplierId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public Warehouse.Warehouse Warehouse { get; set; } = null!;
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
    public ICollection<PurchaseReturn> Returns { get; set; } = new List<PurchaseReturn>();
}
