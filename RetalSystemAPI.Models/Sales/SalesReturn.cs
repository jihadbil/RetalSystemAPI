using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.Sales;

/// <summary>
/// يمثل سند إشعار مرتجع المبيعات — استرداد بضاعة من العميل وإعادتها للمخزون مع رد المبلغ نقداً أو تقييده في حسابه الدائن.
/// </summary>
// كلاس كيان سند مرتجع المبيعات
public class SalesReturn : TenantBaseEntity
{
    /// <summary>
    /// رقم إشعار سند مرتجع المبيعات الفريد الصادر بالنظام.
    /// </summary>
    // خاصية رقم سند مرتجع المبيعات الإلزامي
    public required string ReturnNumber { get; set; }

    /// <summary>
    /// تاريخ ووقت تحرير وتسجيل عملية إرجاع المبيعات.
    /// </summary>
    // خاصية تاريخ سند مرتجع المبيعات
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// معرّف فاتورة المبيعات الأصلية المرجع إليها البضاعة (اختياري).
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف فاتورة المبيعات الأصلية
    public Guid? OriginalInvoiceId { get; set; }

    /// <summary>
    /// كائن فاتورة المبيعات الأصلية المرتبط بالمرتجع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان فاتورة المبيعات الأصلية
    public SalesInvoice? OriginalInvoice { get; set; }

    /// <summary>
    /// معرّف الفرع الذي تم تسجيل المرتجع فيه.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع الذي تم فيه استلام المرتجع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// معرّف المستودع أو صالة العرض التي تستقبل البضاعة المرتجعة وتُعاد لأرصدتها.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستودع
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// كائن المستودع المستلم للبضاعة المرتجعة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستودع
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// معرّف العميل المرجع للبضاعة إن كان مسجلاً بالنظام.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف العميل
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// كائن العميل المرجع للبضاعة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان العميل
    public Customer? Customer { get; set; }

    /// <summary>
    /// إجمالي القيمة المالية للمرتجع المستحقة للعميل.
    /// </summary>
    // خاصية إجمالي مبلغ المرتجع
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// المبلغ النقدي الذي أُعيد فعلياً للعميل فوراً؛ والباقي يُقيد كرصيد دائن في حسابه.
    /// </summary>
    // خاصية المبلغ المعاد نقداً للعميل
    public decimal RefundedAmount { get; set; }

    /// <summary>
    /// طريقة تسوية ورد المبلغ المالي (نقداً، رصيد دائن، تحويل بنكي).
    /// </summary>
    // خاصية طريقة رد المبلغ من تعداد PaymentMethod
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Credit;

    /// <summary>
    /// معرّف الخزينة أو درج الكاشير الذي صُرف منه المبلغ النقدي المعاد للعميل إن وجد.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف خزينة الصرف
    public Guid? RefundSafeId { get; set; }

    /// <summary>
    /// كائن الخزينة المصروف منها مبلغ الاسترداد (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان خزينة الصرف
    public Safe? RefundSafe { get; set; }

    /// <summary>
    /// سبب إرجاع البضاعة من قائمة الأسباب المعتمدة (تالف، صنف خاطئ، إلخ).
    /// </summary>
    // خاصية سبب الإرجاع من تعداد SalesReturnReason
    public SalesReturnReason Reason { get; set; } = SalesReturnReason.Other;

    /// <summary>
    /// ملاحظات وإيضاحات إضافية حول سند مرتجع المبيعات.
    /// </summary>
    // خاصية ملاحظات سند الإرجاع
    public string? Notes { get; set; }

    /// <summary>
    /// قائمة بنود وأصناف وكميات البضاعة المرتجعة من العميل (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة بنود سند مرتجع المبيعات
    public ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
}
