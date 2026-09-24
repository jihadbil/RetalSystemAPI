using System;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Expenses;

/// <summary>
/// ناقل بيانات استجابة تفاصيل سند المصروف (Expense Response DTO).
/// </summary>
public class ExpenseResponseDto : BaseDto
{
    /// <summary>رقم سند المصروف الفريد</summary>
    public string ExpenseNumber { get; set; } = null!;

    /// <summary>تاريخ المصروف</summary>
    public DateTime ExpenseDate { get; set; }

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>معرف تصنيف المصروف</summary>
    public Guid ExpenseCategoryId { get; set; }

    /// <summary>اسم تصنيف المصروف</summary>
    public string ExpenseCategoryName { get; set; } = null!;

    /// <summary>مبلغ المصروف</summary>
    public decimal Amount { get; set; }

    /// <summary>طريقة الدفع التعدادية</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>اسم طريقة الدفع بالعربية</summary>
    public string PaymentMethodName { get; set; } = null!;

    /// <summary>معرف الخزينة أو الدرج المسحوب منه</summary>
    public Guid? SafeId { get; set; }

    /// <summary>اسم الخزينة أو الدرج المسحوب منه</summary>
    public string? SafeName { get; set; }

    /// <summary>اسم المستفيد أو الجهة المصروف لها</summary>
    public string? BeneficiaryName { get; set; }

    /// <summary>البيان / تفاصيل المصروف</summary>
    public string Description { get; set; } = null!;

    /// <summary>ملاحظات إضافية</summary>
    public string? Notes { get; set; }
}
