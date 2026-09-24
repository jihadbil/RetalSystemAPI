using System;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Customers;

/// <summary>
/// يمثل حركات كشف حساب العميل والزبون في دفتر الأستاذ (فواتير مبيعات، سندات قبض وسداد، مرتجعات، تسويات).
/// </summary>
// كلاس كيان حركة حساب العميل المالية
public class CustomerTransaction : TenantBaseEntity
{
    /// <summary>
    /// معرّف العميل المتأثر حسابه بهذه الحركة المالية.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف العميل
    public Guid CustomerId { get; set; }

    /// <summary>
    /// كائن العميل المرتبط بالحركة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان العميل
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// نوع حركة حساب العميل (فاتورة بيع، سداد، مرتجع مبيعات، رصيد افتتاحي، تسوية).
    /// </summary>
    // خاصية نوع الحركة من تعداد CustomerTransactionType
    public CustomerTransactionType TransactionType { get; set; }

    /// <summary>
    /// المبلغ المدين (فاتورة بيع - يزيد من رصيد المديونية المستحقة على العميل).
    /// </summary>
    // خاصية المبلغ المدين للحركة
    public decimal DebitAmount { get; set; } = 0;

    /// <summary>
    /// المبلغ الدائن (سداد أو مرتجع مبيعات - يخفض من رصيد مديونية العميل).
    /// </summary>
    // خاصية المبلغ الدائن للحركة
    public decimal CreditAmount { get; set; } = 0;

    /// <summary>
    /// رصيد العميل الصافي المحسوب بعد تنفيذ وترحيل هذه العملية مباشرة.
    /// </summary>
    // خاصية رصيد العميل اللاحق بعد الحركة
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// تاريخ ووقت تسجيل المعاملة المالية.
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
    /// المعرّف الفريد للمستند المصدر (معرف فاتورة المبيعات، سند القبض، إلخ).
    /// </summary>
    // خاصية معرف المستند المصدر المنشئ للحركة
    public Guid? DocumentId { get; set; }

    /// <summary>
    /// طريقة السداد والوسيلة المستخدمة في حال كانت الحركة سداداً أو قبضاً.
    /// </summary>
    // خاصية وسيلة الدفع من تعداد PaymentMethod
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// البيان والشرح التفصيلي لسبب وطبيعة الحركة في كشف الحساب.
    /// </summary>
    // خاصية بيان وشرح الحركة
    public string? Description { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات إضافية حول الحركة.
    /// </summary>
    // خاصية الملاحظات العامة للحركة
    public string? Notes { get; set; }
}
