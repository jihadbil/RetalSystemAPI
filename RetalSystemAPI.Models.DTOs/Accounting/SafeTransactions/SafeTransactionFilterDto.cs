using System;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.SafeTransactions;

/// <summary>
/// ناقل بيانات تصفية وتجزئة حركات الخزينة (Safe Transaction Filter DTO).
/// </summary>
public class SafeTransactionFilterDto
{
    /// <summary>معرف الخزينة للفلترة (اختياري)</summary>
    public Guid? SafeId { get; set; }

    /// <summary>اتجاه الحركة للفلترة (وارد أو صادر)</summary>
    public TransactionDirection? Direction { get; set; }

    /// <summary>نوع المستند المالي للفلترة</summary>
    public FinancialDocumentType? DocumentType { get; set; }

    /// <summary>تاريخ بداية النطاق الزمني</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>تاريخ نهاية النطاق الزمني</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>رقم الصفحة المطلوبة (افتراضي: 1)</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>حجم الصفحة (افتراضي: 20)</summary>
    public int PageSize { get; set; } = 20;
}
