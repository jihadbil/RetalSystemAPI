using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Accounting.Expenses;

public class CancelExpenseDto
{
    [Required(ErrorMessage = "سبب الإلغاء مطلوب")]
    [MaxLength(250, ErrorMessage = "سبب الإلغاء لا يمكن أن يتجاوز 250 حرفاً")]
    public string Reason { get; set; } = null!;
}
