using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Accounting.ExpenseCategories;

/// <summary>
/// ناقل بيانات تعديل تصنيف مصروفات (Update Expense Category DTO).
/// </summary>
public class UpdateExpenseCategoryDto
{
    /// <summary>اسم تصنيف المصروف</summary>
    [Required(ErrorMessage = "اسم تصنيف المصروف مطلوب")]
    [MaxLength(100, ErrorMessage = "اسم تصنيف المصروف لا يمكن أن يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>حالة نشاط التصنيف</summary>
    public bool IsActive { get; set; }
}
