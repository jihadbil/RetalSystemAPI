using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Branch;

/// <summary>
/// ناقل بيانات إضافة هاتف الفرع (Branch Phone DTO).
/// </summary>
public class BranchPhoneDto
{
    /// <summary>اسم جهة الاتصال أو مسمى الهاتف (مثال: الإدارة، الكاشير)</summary>
    public string? Name { get; set; }

    /// <summary>رقم الهاتف المطابق لنسق أرقام الهواتف المعتمدة</summary>
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;
}
