using System;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات استجابة حركة كشف حساب المورد (Supplier Transaction Response DTO).
/// </summary>
public class SupplierTransactionResponseDto : BaseDto
{
    /// <summary>معرف المورد</summary>
    public Guid SupplierId { get; set; }

    /// <summary>نوع حركة المورد التعدادي</summary>
    public SupplierTransactionType TransactionType { get; set; }

    /// <summary>اسم نوع الحركة بالعربية (فاتورة شراء، سداد، مرتجع، رصيد افتتاحي، تسوية)</summary>
    public string TransactionTypeName { get; set; } = null!;

    /// <summary>المبلغ المدين (سداد للمورد أو مرتجع - يخفض مستحقات المورد)</summary>
    public decimal DebitAmount { get; set; }

    /// <summary>المبلغ الدائن (فاتورة مشتريات - يزيد مستحقات المورد)</summary>
    public decimal CreditAmount { get; set; }

    /// <summary>رصيد المورد بعد تنفيذ العملية</summary>
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

    /// <summary>معرف المستند المصدر (معرف الفاتورة أو سند الصرف)</summary>
    public Guid? DocumentId { get; set; }

    /// <summary>طريقة الدفع (في حالة السداد)</summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>اسم طريقة الدفع بالعربية</summary>
    public string? PaymentMethodName { get; set; }

    /// <summary>البيان / الشرح التفصيلي للحركة</summary>
    public string? Description { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    public string? Notes { get; set; }
}
