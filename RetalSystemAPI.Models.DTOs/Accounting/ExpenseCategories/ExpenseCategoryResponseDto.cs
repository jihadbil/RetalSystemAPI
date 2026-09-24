using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Accounting.ExpenseCategories;

/// <summary>
/// ناقل بيانات استجابة تفاصيل تصنيف المصروفات (Expense Category Response DTO).
/// </summary>
public class ExpenseCategoryResponseDto : BaseDto
{
    /// <summary>اسم تصنيف المصروف</summary>
    public string Name { get; set; } = null!;

    /// <summary>حالة نشاط التصنيف</summary>
    public bool IsActive { get; set; }

    /// <summary>عدد المصروفات المسجلة تحت هذا التصنيف</summary>
    public int ExpensesCount { get; set; }
}
