using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل سند إشعار مرتجع المشتريات — خروج بضاعة من المخزن وإعادتها للمورد مع خصمها من المخزون وتخفيض المديونية أو استرداد النقدية.
/// </summary>
// كلاس كيان سند مرتجع المشتريات
public class PurchaseReturn : TenantBaseEntity
{
    /// <summary>
    /// رقم إشعار سند مرتجع المشتريات الفريد الصادر بالنظام.
    /// </summary>
    // خاصية رقم سند مرتجع المشتريات الإلزامي
    public required string ReturnNumber { get; set; }

    /// <summary>
    /// تاريخ ووقت إجراء وتسجيل عملية الإرجاع للمورد.
    /// </summary>
    // خاصية تاريخ سند الإرجاع
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// معرّف فاتورة الشراء الأصلية المرجع إليها البضاعة (اختياري).
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف فاتورة الشراء الأصلية
    public Guid? PurchaseInvoiceId { get; set; }

    /// <summary>
    /// كائن فاتورة الشراء الأصلية المرتبط بالمرتجع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان فاتورة الشراء الأصلية
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    /// <summary>
    /// معرّف المورد المرتجع إليه البضاعة.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المورد
    public Guid SupplierId { get; set; }

    /// <summary>
    /// كائن المورد المرتجع إليه البضاعة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المورد
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// معرّف الفرع الذي تم منه إرجاع المشتريات.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع الذي تم منه الإرجاع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// معرّف المستودع الرئيسي أو صالة العرض التي تخرج منها البضاعة المرتجعة.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستودع
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// كائن المستودع المنقولة منه البضاعة المرتجعة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستودع
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// طريقة تسوية المرتجع المالي (نقداً واسترداد المبلغ أو آجل بتخفيض مديونية المورد).
    /// </summary>
    // خاصية وسيلة تسوية المرتجع من تعداد PaymentMethod
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>
    /// إجمالي القيمة المالية للمرتجع المستحقة من المورد.
    /// </summary>
    // خاصية إجمالي مبلغ مرتجع المشتريات
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// معرّف الخزينة أو الحساب البنكي الذي تم إيداع وقبض قيمة المرتجع النقدي فيه.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف خزينة الاسترداد
    public Guid? RefundSafeId { get; set; }

    /// <summary>
    /// كائن الخزينة المستلمة لمبلغ المرتجع المسترد (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان خزينة الاسترداد
    public Safe? RefundSafe { get; set; }

    /// <summary>
    /// سبب إرجاع البضاعة للمورد من قائمة الأسباب المعتمدة.
    /// </summary>
    // خاصية سبب الإرجاع من تعداد PurchaseReturnReason
    public PurchaseReturnReason Reason { get; set; } = PurchaseReturnReason.Defective;

    /// <summary>
    /// ملاحظات وتفاصيل إضافية حول سند مرتجع المشتريات.
    /// </summary>
    // خاصية ملاحظات سند الإرجاع
    public string? Notes { get; set; }

    /// <summary>
    /// قائمة بنود وأصناف وكميات البضاعة المرتجعة للمورد (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة بنود سند مرتجع المشتريات
    public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}
