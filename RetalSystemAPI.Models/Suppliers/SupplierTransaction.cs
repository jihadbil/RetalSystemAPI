using System;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Suppliers;

/// <summary>
/// يمثل حركات كشف حساب المورد في دفتر أستاذ المساعد (فواتير مشتريات، سندات صرف وسداد، مرتجعات، تسويات).
/// </summary>
// كلاس كيان حركة حساب المورد المالية
public class SupplierTransaction : TenantBaseEntity
{
    /// <summary>
    /// معرّف المورد المتأثر حسابه بهذه الحركة المالية.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المورد
    public Guid SupplierId { get; set; }

    /// <summary>
    /// كائن المورد المرتبط بالحركة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المورد
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// نوع حركة المورد (فاتورة شراء، سداد، مرتجع مشتريات، رصيد افتتاحي، تسوية).
    /// </summary>
    // خاصية نوع الحركة من تعداد SupplierTransactionType
    public SupplierTransactionType TransactionType { get; set; }

    /// <summary>
    /// المبلغ المدين (سداد للمورد أو مرتجع مشتريات - يخفض من رصيد التزامات المؤسسة تجاه المورد).
    /// </summary>
    // خاصية المبلغ المدين للحركة
    public decimal DebitAmount { get; set; } = 0;

    /// <summary>
    /// المبلغ الدائن (فاتورة مشتريات - يزيد من رصيد التزامات المؤسسة ومستحقات المورد).
    /// </summary>
    // خاصية المبلغ الدائن للحركة
    public decimal CreditAmount { get; set; } = 0;

    /// <summary>
    /// رصيد المورد الصافي المحسوب بعد تنفيذ وترحيل هذه العملية مباشرة.
    /// </summary>
    // خاصية رصيد المورد اللاحق بعد الحركة
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// تاريخ ووقت تسجيل المعاملة المالية للمورد.
    /// </summary>
    // خاصية تاريخ ووقت الحركة
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// معرّف الفرع الذي تمت فيه المعاملة المالية.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع الذي تمت فيه الحركة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// نوع المستند المالي المرتبط بالحركة في النظام.
    /// </summary>
    // خاصية نوع المستند من تعداد FinancialDocumentType
    public FinancialDocumentType DocumentType { get; set; }

    /// <summary>
    /// المعرّف الفريد للمستند المصدر (معرف فاتورة المشتريات، سند الصرف، إلخ).
    /// </summary>
    // خاصية معرف المستند المصدر المنشئ للحركة
    public Guid? DocumentId { get; set; }

    /// <summary>
    /// طريقة السداد والوسيلة المستخدمة في حال كانت الحركة سداداً نقدياً أو بنكياً.
    /// </summary>
    // خاصية وسيلة الدفع من تعداد PaymentMethod
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// البيان والشرح التفصيلي لسبب وطبيعة الحركة في كشف حساب المورد.
    /// </summary>
    // خاصية بيان وشرح الحركة
    public string? Description { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات إضافية حول الحركة.
    /// </summary>
    // خاصية الملاحظات العامة للحركة
    public string? Notes { get; set; }
}
