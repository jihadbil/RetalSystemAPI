using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.Sales;

/// <summary>
/// يمثل فاتورة المبيعات سواء كانت نقدية أو آجلة أو جزئية السداد.
/// </summary>
public class SalesInvoice : TenantBaseEntity
{
    /// <summary>
    /// رقم الفاتورة الفريد.
    /// </summary>
    public required string InvoiceNumber { get; set; }

    /// <summary>
    /// تاريخ إصدار الفاتورة.
    /// </summary>
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// صالة العرض أو المستودع الذي تم البيع منه.
    /// </summary>
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// العميل المرتبط بالفاتورة (اختياري للزبائن العابرين).
    /// </summary>
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    // ── الحقول المالية ─────────────────────────────────────────
    /// <summary>
    /// الإجمالي قبل الخصم.
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// قيمة الخصم الإجمالي على الفاتورة.
    /// </summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// الإجمالي الصافي بعد الخصم.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// المبلغ المدفوع فعلياً.
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// المبلغ المتبقي (0 = مسدد بالكامل، أكبر من 0 = دين/آجل).
    /// </summary>
    public decimal RemainingAmount { get; set; }

    public string? Notes { get; set; }

    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    public ICollection<SalesReturn> Returns { get; set; } = new List<SalesReturn>();
}
