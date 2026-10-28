namespace RetalSystemAPI.Models.DTOs.Accounting.Reconciliation;

/// <summary>
/// نتيجة فحص اتساق المستندات والدفاتر والأرصدة المالية للمستأجر الحالي.
/// </summary>
public sealed class FinancialReconciliationReportDto
{
    /// <summary>تاريخ ووقت توليد التقرير بتوقيت UTC</summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>هامش التفاوت المالي المعتمد في المقارنات (Tolerance)</summary>
    public decimal Tolerance { get; init; }

    /// <summary>إجمالي عدد المشاكل والمخالفات المكتشفة</summary>
    public int TotalIssues { get; init; }

    /// <summary>عدد المشاكل الحرجة التي تتطلب تدخلاً فورياً (مثل اختلال التوازن المحاسبي)</summary>
    public int CriticalIssues { get; init; }

    /// <summary>عدد التنبيهات والتحذيرات غير الحرجة</summary>
    public int WarningIssues { get; init; }

    /// <summary>هل تم اقتصار النتائج على الحد الأقصى المسموح لمنع تضخم الاستجابة</summary>
    public bool IsTruncated { get; init; }

    /// <summary>قائمة المخالفات المالية المكتشفة بالتفصيل</summary>
    public IReadOnlyList<FinancialReconciliationIssueDto> Issues { get; init; } = [];
}

/// <summary>
/// مخالفة مالية واحدة قابلة للتتبع والمعالجة اليدوية.
/// </summary>
public sealed class FinancialReconciliationIssueDto
{
    /// <summary>رمز أو كود نوع المشكلة البرمجي للفلترة والتصنيف</summary>
    public required string Code { get; init; }

    /// <summary>مستوى خطورة المشكلة (Critical, Warning, Info)</summary>
    public required string Severity { get; init; }

    /// <summary>نوع الكيان المالي المتأثر (Invoice, Safe, Customer, etc.)</summary>
    public required string EntityType { get; init; }

    /// <summary>المعرف الفريد للكيان المتأثر</summary>
    public Guid EntityId { get; init; }

    /// <summary>معرف الفرع التابع له الكيان (إن وجد)</summary>
    public Guid? BranchId { get; init; }

    /// <summary>الرقم المرجعي أو كود المستند المرتبط</summary>
    public string? ReferenceNumber { get; init; }

    /// <summary>المبلغ المالي المتوقع دفترياً أو حسابياً</summary>
    public decimal? ExpectedAmount { get; init; }

    /// <summary>المبلغ المالي الفعلي المسجل في الدفاتر</summary>
    public decimal? ActualAmount { get; init; }

    /// <summary>الفارق المالي المكتشف بين المتوقع والفعلي</summary>
    public decimal? Difference { get; init; }

    /// <summary>نص رسالة الخطأ وشرح المشكلة بالعربية</summary>
    public required string Message { get; init; }

    /// <summary>الإجراء التصحيحي المقترح لحل المشكلة</summary>
    public required string SuggestedAction { get; init; }
}
