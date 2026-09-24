using System;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات استجابة حركة كشف حساب العميل (Customer Transaction Response DTO).
/// </summary>
public class CustomerTransactionResponseDto : BaseDto
{
    /// <summary>معرف العميل</summary>
    public Guid CustomerId { get; set; }

    /// <summary>نوع حركة العميل التعدادي</summary>
    public CustomerTransactionType TransactionType { get; set; }

    /// <summary>اسم نوع الحركة بالعربية (فاتورة بيع، سداد، مرتجع، رصيد افتتاحي، تسوية)</summary>
    public string TransactionTypeName { get; set; } = null!;

    /// <summary>المبلغ المدين (يزيد مديونية العميل)</summary>
    public decimal DebitAmount { get; set; }

    /// <summary>المبلغ الدائن (يخفض مديونية العميل)</summary>
    public decimal CreditAmount { get; set; }

    /// <summary>رصيد العميل بعد تنفيذ الحركة</summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>تاريخ ووقت الحركة</summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>نوع المستند المالي المصدر</summary>
    public FinancialDocumentType DocumentType { get; set; }

    /// <summary>اسم نوع المستند المالي بالعربية</summary>
    public string DocumentTypeName { get; set; } = null!;

    /// <summary>معرف المستند المصدر (معرف الفاتورة أو سند القبض)</summary>
    public Guid? DocumentId { get; set; }

    /// <summary>طريقة الدفع (في حال السداد أو القبض)</summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>اسم طريقة الدفع بالعربية</summary>
    public string? PaymentMethodName { get; set; }

    /// <summary>البيان / الشرح التفصيلي للحركة</summary>
    public string? Description { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    public string? Notes { get; set; }
}
