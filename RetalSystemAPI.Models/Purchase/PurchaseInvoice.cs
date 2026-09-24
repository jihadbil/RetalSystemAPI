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
/// يمثل فاتورة مشتريات بضاعة مباشرة من المورد مع زيادة أرصدة المخزون تلقائياً وإثبات الذمة المالية.
/// </summary>
// كلاس كيان فاتورة المشتريات
public class PurchaseInvoice : TenantBaseEntity
{
    /// <summary>
    /// رقم الفاتورة التسلسلي في النظام أو رقم فاتورة الشراء الصادرة من المورد.
    /// </summary>
    // خاصية رقم فاتورة المشتريات الإلزامي
    public required string InvoiceNumber { get; set; }

    /// <summary>
    /// تاريخ ووقت إصدار وتسجيل فاتورة المشتريات.
    /// </summary>
    // خاصية تاريخ إصدار الفاتورة
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// معرّف المورد الصادرة منه فاتورة الشراء.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المورد
    public Guid SupplierId { get; set; }

    /// <summary>
    /// كائن المورد المرتبط بفاتورة الشراء (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المورد
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// معرّف الفرع الذي تم تسجيل واستلام فاتورة المشتريات لصالحه.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع المستلم للمشتريات (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// معرّف المستودع أو صالة العرض التي تم توريد واستلام البضاعة فيها.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستودع
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// كائن المستودع المستلم للبضاعة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستودع
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// حالة الفاتورة الحالية (مسودة، معلقة، مسددة، مسددة جزئياً، ملغاة).
    /// </summary>
    // خاصية حالة الفاتورة من تعداد InvoiceStatus
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;

    /// <summary>
    /// طريقة السداد والوسيلة المعتمدة لدفع قيمة المشتريات (نقدي، تحويل، آجل، إلخ).
    /// </summary>
    // خاصية طريقة الدفع من تعداد PaymentMethod
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    // ── الحقول المالية ─────────────────────────────────────────

    /// <summary>
    /// إجمالي الفاتورة قبل احتساب الخصومات والضرائب.
    /// </summary>
    // خاصية الإجمالي قبل الخصم والضريبة
    public decimal SubTotal { get; set; }

    /// <summary>
    /// قيمة الخصم الممنوح على إجمالي الفاتورة من المورد.
    /// </summary>
    // خاصية مبلغ الخصم
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// قيمة الضريبة المحتسبة على إجمالي مشتريات الفاتورة.
    /// </summary>
    // خاصية مبلغ الضريبة
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>
    /// المبلغ الإجمالي الصافي النهائي للفاتورة الواجب سداده للمورد.
    /// </summary>
    // خاصية المبلغ الإجمالي النهائي للفاتورة
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// المبلغ المدفوع والمسدد فعلياً للمورد عند استلام الفاتورة.
    /// </summary>
    // خاصية المبلغ المدفوع للمورد
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// المبلغ المتبقي المستحق كدين والتزام لصالح المورد.
    /// </summary>
    // خاصية المبلغ المتبقي الآجل
    public decimal RemainingAmount { get; set; }

    /// <summary>
    /// معرّف الخزينة أو الحساب البنكي الذي صُرف منه المبلغ المدفوع للمورد (اختياري).
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف خزينة السداد
    public Guid? PaymentSafeId { get; set; }

    /// <summary>
    /// كائن الخزينة المسحوب منها الدفع النقدي (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان خزينة السداد
    public Safe? PaymentSafe { get; set; }

    /// <summary>
    /// معرّف أمر وطلبية الشراء الأصلية التي انبثقت عنها هذه الفاتورة إن وجدت.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف أمر الشراء
    public Guid? PurchaseOrderId { get; set; }

    /// <summary>
    /// كائن أمر الشراء الأصلي المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان أمر الشراء
    public PurchaseOrder? PurchaseOrder { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات إضافية حول فاتورة الشراء وبنود التوريد.
    /// </summary>
    // خاصية ملاحظات فاتورة المشتريات
    public string? Notes { get; set; }

    /// <summary>
    /// قائمة بنود وأصناف فاتورة الشراء وأسعار التكلفة لكل بند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة بنود فاتورة الشراء
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();

    /// <summary>
    /// قائمة مردودات المشتريات الصادرة على هذه الفاتورة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة مرتجعات الفاتورة
    public ICollection<PurchaseReturn> Returns { get; set; } = new List<PurchaseReturn>();
}
