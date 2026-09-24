using System;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Accounting;

/// <summary>
/// يمثل الحركات المالية في الخزائن النقدية والبنوك وأدراج الكاشير (إيداع، سحب، تحويل، وتغذية).
/// </summary>
// كلاس كيان حركة الخزينة المالية
public class SafeTransaction : TenantBaseEntity
{
    /// <summary>
    /// رقم الحركة المالي الفريد التسلسلي الصادر بالنظام.
    /// </summary>
    // خاصية رقم الحركة المالي
    public required string TransactionNumber { get; set; }

    /// <summary>
    /// معرّف الخزينة أو الحساب المصرفي أو الدرج المتأثر بالحركة.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الخزينة
    public Guid SafeId { get; set; }

    /// <summary>
    /// كائن الخزينة المتأثرة بالحركة المالية (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الخزينة
    public Safe Safe { get; set; } = null!;

    /// <summary>
    /// اتجاه التدفق النقدي للحركة (وارد / صادر).
    /// </summary>
    // خاصية اتجاه الحركة من تعداد TransactionDirection
    public TransactionDirection Direction { get; set; }

    /// <summary>
    /// القيمة النقدية أو المبلغ المالي للحركة.
    /// </summary>
    // خاصية مبلغ الحركة المالي
    public decimal Amount { get; set; }

    /// <summary>
    /// رصيد الخزينة المحسوب بعد تنفيذ وترحيل هذه الحركة مباشرة.
    /// </summary>
    // خاصية الرصيد اللاحق بعد الحركة
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// تاريخ ووقت وقوع العملية المالية الفعلي.
    /// </summary>
    // خاصية تاريخ الحركة الفعلي
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// نوع المستند المالي المرتبط بالحركة (فاتورة مبيعات، مشتريات، سند صرف، إلخ).
    /// </summary>
    // خاصية نوع المستند المالي من تعداد FinancialDocumentType
    public FinancialDocumentType DocumentType { get; set; }

    /// <summary>
    /// المعرّف الفريد للمستند المصدر المنشئ للحركة (فاتورة، سند مصروف، تحويل).
    /// </summary>
    // خاصية معرف المستند المصدر المرتبط بالحركة
    public Guid? DocumentId { get; set; }

    /// <summary>
    /// البيان أو الشرح المحاسبي التفصيلي لسبب وطبيعة الحركة.
    /// </summary>
    // خاصية بيان وشرح الحركة
    public string? Description { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات إضافية على الحركة المالية.
    /// </summary>
    // خاصية الملاحظات الإضافية للحركة
    public string? Notes { get; set; }

    /// <summary>
    /// معرّف جلسة الكاشير المرتبطة بالحركة في حال تمت ضمن وردية نقطة بيع.
    /// </summary>
    // خاصية المفتاح الأجنبي لجلسة الكاشير
    public Guid? CashSessionId { get; set; }

    /// <summary>
    /// كائن جلسة الكاشير المرتبطة بالحركة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان جلسة الكاشير
    public CashSession? CashSession { get; set; }

    /// <summary>
    /// معرّف المستخدم الذي قام بتنفيذ وإدخال هذه الحركة في النظام.
    /// </summary>
    // خاصية معرف المستخدم منفذ الحركة
    public string? PerformedByUserId { get; set; }

    /// <summary>
    /// مفتاح منع التكرار المالي (Idempotency Key) لضمان عدم تكرار ترحيل الحركة بالخطأ.
    /// </summary>
    // خاصية مفتاح التحقق من عدم تكرار العملية
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// تاريخ وتوقيت الترحيل النهائي وتأكيد الحركة في السجلات المالية.
    /// </summary>
    // خاصية تاريخ وتوقيت الترحيل المالي
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
}
