using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل مرتجع المشتريات — خروج بضاعة من المخزن وإعادتها للمورد مع خصمها من المخزون.
/// </summary>
public class PurchaseReturn : TenantBaseEntity
{
    /// <summary>
    /// رقم إشعار مرتجع المشتريات.
    /// </summary>
    public required string ReturnNumber { get; set; }

    /// <summary>
    /// تاريخ الإرجاع.
    /// </summary>
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// فاتورة الشراء الأصلية المرجع إليها (اختياري).
    /// </summary>
    public Guid? PurchaseInvoiceId { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    /// <summary>
    /// المورد المرجع إليه البضاعة.
    /// </summary>
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// الفرع الذي تم منه الإرجاع.
    /// </summary>
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// المستودع الرئيسي (المخزن) الذي تخرج منه البضاعة المرتجعة.
    /// </summary>
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// طريقة تسوية المرتجع (نقداً أو آجل بتخفيض حساب المورد).
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>
    /// إجمالي قيمة المرتجع.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// سبب الإرجاع للمورد.
    /// </summary>
    public PurchaseReturnReason Reason { get; set; } = PurchaseReturnReason.Defective;

    /// <summary>
    /// ملاحظات إضافية على المرتجع.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// بنود الأصناف المرتجعة.
    /// </summary>
    public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}
