using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.Sales;

/// <summary>
/// يمثل مرتجع المبيعات — رجوع بضاعة من العميل إلى المخزن أو الصالة.
/// </summary>
public class SalesReturn : TenantBaseEntity
{
    /// <summary>
    /// رقم إشعار الإرجاع.
    /// </summary>
    public required string ReturnNumber { get; set; }

    /// <summary>
    /// تاريخ الإرجاع.
    /// </summary>
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// الفاتورة الأصلية المرجع إليها — اختيارية.
    /// </summary>
    public Guid? OriginalInvoiceId { get; set; }
    public SalesInvoice? OriginalInvoice { get; set; }

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// المستودع أو الصالة التي يعود إليها المخزون.
    /// </summary>
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>
    /// إجمالي قيمة المرتجع.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// سبب الإرجاع.
    /// </summary>
    public SalesReturnReason Reason { get; set; } = SalesReturnReason.Other;

    public string? Notes { get; set; }

    public ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
}
