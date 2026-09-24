using System;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Accounting.Expenses;

/// <summary>
/// ناقل بيانات تعديل سند المصروف (Update Expense DTO).
/// </summary>
public class UpdateExpenseDto
{
    /// <summary>معرف تصنيف المصروف</summary>
    [Required(ErrorMessage = "تصنيف المصروف مطلوب")]
    public Guid ExpenseCategoryId { get; set; }

    /// <summary>اسم المستفيد أو الجهة المصروف لها</summary>
    [MaxLength(150, ErrorMessage = "اسم المستفيد لا يمكن أن يتجاوز 150 حرفاً")]
    public string? BeneficiaryName { get; set; }

    /// <summary>البيان / تفاصيل وشرح المصروف</summary>
    [Required(ErrorMessage = "بيان المصروف مطلوب")]
    [MaxLength(300, ErrorMessage = "بيان المصروف لا يمكن أن يتجاوز 300 حرف")]
    public string Description { get; set; } = null!;

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
