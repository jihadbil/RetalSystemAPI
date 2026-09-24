namespace RetalSystemAPI.Models.DTOs.Accounting.Reconciliation;

/// <summary>نتيجة فحص اتساق المستندات والدفاتر والأرصدة المالية للمستأجر الحالي.</summary>
public sealed class FinancialReconciliationReportDto
{
    public DateTime GeneratedAtUtc { get; init; }
    public decimal Tolerance { get; init; }
    public int TotalIssues { get; init; }
    public int CriticalIssues { get; init; }
    public int WarningIssues { get; init; }
    public bool IsTruncated { get; init; }
    public IReadOnlyList<FinancialReconciliationIssueDto> Issues { get; init; } = [];
}

/// <summary>مخالفة مالية واحدة قابلة للتتبع والمعالجة اليدوية.</summary>
public sealed class FinancialReconciliationIssueDto
{
    public required string Code { get; init; }
    public required string Severity { get; init; }
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public Guid? BranchId { get; init; }
    public string? ReferenceNumber { get; init; }
    public decimal? ExpectedAmount { get; init; }
    public decimal? ActualAmount { get; init; }
    public decimal? Difference { get; init; }
    public required string Message { get; init; }
    public required string SuggestedAction { get; init; }
}
