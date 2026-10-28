using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Accounting.Expenses;

/// <summary>
/// ناقل بيانات إلغاء سند مصروف مسجل مع بيان السبب الإجباري.
/// </summary>
public class CancelExpenseDto
{
    /// <summary>سبب ومبرر إلغاء المصروف</summary>
    [Required(ErrorMessage = "سبب الإلغاء مطلوب")]
    [MaxLength(250, ErrorMessage = "سبب الإلغاء لا يمكن أن يتجاوز 250 حرفاً")]
    public string Reason { get; set; } = null!;
}
