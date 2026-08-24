using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Customers;

public class CustomerPhoneDto
{
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;

    [MaxLength(100, ErrorMessage = "اسم جهة الاتصال يجب أن لا يتجاوز 100 حرف")]
    public string? ContactName { get; set; }

    public bool IsDefault { get; set; } = false;
}
