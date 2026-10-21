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
/// يمثل فاتورة المبيعات الصادرة للعملاء في نقاط البيع أو صالات العرض (نقدية، آجلة، أو جزئية السداد)، وتخصم من المخزون تلقائياً.
/// </summary>
// كلاس كيان فاتورة المبيعات
public class SalesInvoice : TenantBaseEntity
{
    /// <summary>
    /// رقم الفاتورة الفريد التسلسلي الصادر بالنظام (مثال: INV-2026-0001).
    /// </summary>
    // خاصية رقم فاتورة المبيعات الإلزامي
    public required string InvoiceNumber { get; set; }

    /// <summary>بصمة طلب نقطة البيع للتحقق من إعادة المحاولة بنفس البيانات.</summary>
    public string? PosRequestHash { get; set; }

    /// <summary>
    /// تاريخ ووقت إصدار الفاتورة.
    /// </summary>
    // خاصية تاريخ إصدار الفاتورة
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// معرّف الفرع الذي تم إصدار فاتورة المبيعات منه.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع الذي تمت فيه عملية البيع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// معرّف صالة العرض أو المستودع الذي تم بيع وخصم البضاعة منه.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستودع
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// كائن صالة العرض أو المستودع المباع منه البضاعة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستودع
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// معرّف العميل المرتبط بالفاتورة (اختياري للزبائن العابرين).
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف العميل
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// كائن العميل المشتري (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان العميل
    public Customer? Customer { get; set; }

    /// <summary>
    /// حالة الفاتورة الحالية (مسودة، معلقة، مسددة بالكامل، مسددة جزئياً، ملغاة).
    /// </summary>
    // خاصية حالة الفاتورة من تعداد InvoiceStatus
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;

    /// <summary>
    /// طريقة الدفع والسداد المعتمدة للفاتورة (نقدي، بطاقة مصرفية، آجل، تحويل).
    /// </summary>
    // خاصية طريقة الدفع من تعداد PaymentMethod
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    // ── الحقول المالية ─────────────────────────────────────────

    /// <summary>
    /// إجمالي الفاتورة قبل احتساب الخصومات والضرائب.
    /// </summary>
    // خاصية الإجمالي قبل الخصم
    public decimal SubTotal { get; set; }

    /// <summary>
    /// قيمة الخصم المالي الإجمالي الممنوح على الفاتورة.
    /// </summary>
    // خاصية مبلغ الخصم الإجمالي
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// قيمة الضريبة المحتسبة على إجمالي مبيعات الفاتورة.
    /// </summary>
    // خاصية مبلغ الضريبة المحتسبة
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>
    /// الإجمالي الصافي النهائي للفاتورة بعد تطبيق الخصم والضريبة.
    /// </summary>
    // خاصية المبلغ الإجمالي النهائي للفاتورة
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// المبلغ المدفوع والمقبوض فعلياً من العميل عند البيع.
    /// </summary>
    // خاصية المبلغ المدفوع من العميل
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// المبلغ المتبقي الآجل المستحق كدين على حساب العميل (0 = مسددة بالكامل).
    /// </summary>
    // خاصية المبلغ المتبقي المستحق على العميل
    public decimal RemainingAmount { get; set; }

    /// <summary>
    /// معرّف الخزينة أو درج الكاشير الذي استلم وقبض المبلغ المدفوع.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف خزينة التحصيل
    public Guid? PaymentSafeId { get; set; }

    /// <summary>
    /// كائن الخزينة أو درج النقدية المستلم للمبلغ (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان خزينة التحصيل
    public Safe? PaymentSafe { get; set; }

    /// <summary>
    /// معرّف طرفية وجهاز نقطة البيع (POS Terminal) التي تم إصدار الفاتورة من خلالها إن وجدت.
    /// </summary>
    // خاصية المفتاح الأجنبي لطرفية نقطة البيع
    public Guid? PosTerminalId { get; set; }

    /// <summary>
    /// كائن طرفية نقطة البيع الصادرة منها الفاتورة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان طرفية نقطة البيع
    public PosTerminal? PosTerminal { get; set; }

    /// <summary>
    /// معرّف جلسة ووردية الكاشير المرتبطة بعملية البيع.
    /// </summary>
    // خاصية المفتاح الأجنبي لجلسة الكاشير
    public Guid? CashSessionId { get; set; }

    /// <summary>
    /// كائن جلسة ووردية الكاشير المرتبطة بالفاتورة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان جلسة الكاشير
    public CashSession? CashSession { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات إضافية حول فاتورة المبيعات.
    /// </summary>
    // خاصية ملاحظات فاتورة المبيعات
    public string? Notes { get; set; }

    /// <summary>
    /// قائمة بنود وأصناف فاتورة المبيعات والكميات والأسعار (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة بنود فاتورة المبيعات
    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();

    /// <summary>
    /// قائمة مردودات المبيعات المحررة على هذه الفاتورة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة مرتجعات الفاتورة
    public ICollection<SalesReturn> Returns { get; set; } = new List<SalesReturn>();
}
