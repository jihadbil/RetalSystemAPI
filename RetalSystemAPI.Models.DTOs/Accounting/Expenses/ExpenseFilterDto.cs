using System;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Expenses;

/// <summary>
/// ناقل بيانات تصفية وفلترة سندات المصروفات (Expense Filter DTO).
/// </summary>
public class ExpenseFilterDto
{
    /// <summary>معرف الفرع للفلترة</summary>
    public Guid? BranchId { get; set; }

    /// <summary>معرف تصنيف المصروف للفلترة</summary>
    public Guid? ExpenseCategoryId { get; set; }

    /// <summary>معرف الخزينة للفلترة</summary>
    public Guid? SafeId { get; set; }

    /// <summary>طريقة الدفع للفلترة</summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>تاريخ بداية النطاق</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>تاريخ نهاية النطاق</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>كلمة البحث (في البيان أو رقم السند أو اسم المستفيد)</summary>
    public string? SearchTerm { get; set; }

    /// <summary>رقم الصفحة (افتراضي: 1)</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>حجم الصفحة (افتراضي: 20)</summary>
    public int PageSize { get; set; } = 20;
}
