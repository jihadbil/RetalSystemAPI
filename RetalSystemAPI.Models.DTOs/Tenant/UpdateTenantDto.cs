using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

public class UpdateTenantDto
{
    [Required(ErrorMessage = "معرف المستأجر مطلوب")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم المستأجر مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم المستأجر يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string? ContactEmail { get; set; }

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;

    public string Address { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
