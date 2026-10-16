using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.QueryResults;
public sealed class PurchaseInvoiceReadResult
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public Guid? PaymentSafeId { get; set; }
    public decimal RemainingAmount { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseInvoiceItemReadResult> Items { get; set; } = new();
}

public sealed class PurchaseInvoiceItemReadResult
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public string? BarCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public List<PurchaseInvoiceBreakdownReadResult> Breakdowns { get; set; } = new();
}

public sealed class PurchaseInvoiceBreakdownReadResult
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid ProductBarCodeId { get; set; }
    public string? BarCode { get; set; }
    public string? Title { get; set; }
    public decimal PackageQuantity { get; set; }
    public int UnitsPerPackage { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
