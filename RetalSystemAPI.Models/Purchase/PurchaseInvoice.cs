using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل فاتورة مشتريات بضاعة مباشرة من المورد مع زيادة أرصدة المخزون تلقائياً.
/// </summary>
public class PurchaseInvoice : TenantBaseEntity
{
    /// <summary>
    /// رقم الفاتورة في النظام أو رقم فاتورة المورد.
    /// </summary>
    public required string InvoiceNumber { get; set; }

    /// <summary>
    /// تاريخ إصدار الفاتورة.
    /// </summary>
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// المستودع أو صالة العرض التي تم استلام البضاعة فيها.
    /// </summary>
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    // ── الحقول المالية ─────────────────────────────────────────
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    /// <summary>
    /// معرف طلبية الشراء الأصلية إن وجدت (اختياري).
    /// </summary>
    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    public string? Notes { get; set; }

    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
}
