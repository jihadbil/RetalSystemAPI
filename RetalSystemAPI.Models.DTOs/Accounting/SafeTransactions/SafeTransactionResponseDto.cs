using System;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.SafeTransactions;

/// <summary>
/// ناقل بيانات استجابة تفاصيل حركة الخزينة (Safe Transaction Response DTO).
/// </summary>
public class SafeTransactionResponseDto : BaseDto
{
    public string? ReferenceNumber { get; set; }

    /// <summary>رقم الحركة المالي الفريد</summary>
    public string TransactionNumber { get; set; } = null!;

    /// <summary>معرف الخزينة</summary>
    public Guid SafeId { get; set; }

    /// <summary>اسم الخزينة</summary>
    public string SafeName { get; set; } = null!;

    /// <summary>اتجاه الحركة التعدادي (وارد / صادر)</summary>
    public TransactionDirection Direction { get; set; }

    /// <summary>اسم اتجاه الحركة بالعربية (وارد / صادر)</summary>
    public string DirectionName { get; set; } = null!;

    /// <summary>مبلغ الحركة</summary>
    public decimal Amount { get; set; }

    /// <summary>رصيد الخزينة بعد تنفيذ الحركة</summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>تاريخ ووقت الحركة</summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>نوع المستند المالي المصدر</summary>
    public FinancialDocumentType DocumentType { get; set; }

    /// <summary>اسم نوع المستند المالي بالعربية</summary>
    public string DocumentTypeName { get; set; } = null!;

    /// <summary>معرف المستند المصدر (فاتورة، سند، إلخ)</summary>
    public Guid? DocumentId { get; set; }

    /// <summary>البيان / الشرح التفصيلي</summary>
    public string? Description { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    public string? Notes { get; set; }
}
