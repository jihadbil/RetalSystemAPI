using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات إضافة هاتف المورد (Supplier Phone DTO).
/// </summary>
public class SupplierPhoneDto
{
    /// <summary>اسم الشخص المسؤول أو مسمى جهة الاتصال</summary>
    public string? Name { get; set; }

    /// <summary>رقم هاتف المورد المطابق للنسق الليبي المعتمد</summary>
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;
}
