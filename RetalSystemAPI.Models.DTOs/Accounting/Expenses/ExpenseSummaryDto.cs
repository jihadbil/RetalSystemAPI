using System;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Expenses;

/// <summary>
/// ناقل بيانات ملخص المصروف لشاشات العرض والجداول السريعة (Expense Summary DTO).
/// </summary>
public class ExpenseSummaryDto
{
    /// <summary>المعرف الفريد</summary>
    public Guid Id { get; set; }

    /// <summary>رقم سند المصروف</summary>
    public string ExpenseNumber { get; set; } = null!;

    /// <summary>تاريخ المصروف</summary>
    public DateTime ExpenseDate { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>اسم تصنيف المصروف</summary>
    public string ExpenseCategoryName { get; set; } = null!;

    /// <summary>مبلغ المصروف</summary>
    public decimal Amount { get; set; }

    /// <summary>اسم طريقة الدفع بالعربية</summary>
    public string PaymentMethodName { get; set; } = null!;

    /// <summary>اسم الخزينة المسحوب منها إن وجدت</summary>
    public string? SafeName { get; set; }

    /// <summary>اسم المستفيد</summary>
    public string? BeneficiaryName { get; set; }

    /// <summary>البيان المختصر</summary>
    public string Description { get; set; } = null!;
}
