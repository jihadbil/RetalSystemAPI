using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات إضافة أو تعديل هاتف العميل (Customer Phone DTO).
/// يتحقق من صحة نسق رقم الهاتف الليبي.
/// </summary>
public class CustomerPhoneDto
{
    /// <summary>رقم الهاتف المطابق لنسق أرقام الهواتف المعتمدة</summary>
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;

    /// <summary>اسم الشخص المسؤول أو جهة الاتصال التابعة لهذا الرقم</summary>
    [MaxLength(100, ErrorMessage = "اسم جهة الاتصال يجب أن لا يتجاوز 100 حرف")]
    public string? ContactName { get; set; }

    /// <summary>هل هذا الرقم هو الرقم الافتراضي للعميل</summary>
    public bool IsDefault { get; set; } = false;
}
