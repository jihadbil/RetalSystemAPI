using RetalSystemAPI.Models.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات تعديل بيانات مورد قائم (Update Supplier DTO).
/// </summary>
public class UpdateSupplierDto
{
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string? CompanyName { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(150)]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string? Email { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string? TaxNumber { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
    /// <summary>المعرف الفريد للمورد المطلوب تعديله</summary>
    [Required(ErrorMessage = "معرف المورد مطلوب")]
    public Guid Id { get; set; }

    /// <summary>اسم المورد الجديد</summary>
    [Required(ErrorMessage = "اسم المورد مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم المورد يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>عنوان مقر المورد</summary>
    [MaxLength(500, ErrorMessage = "العنوان يجب أن لا يتجاوز 500 حرف")]
    public string? Address { get; set; }

    /// <summary>قائمة أرقام هواتف المورد المحدثة</summary>
    [NoNullItems]
    public List<SupplierPhoneDto> Phones { get; set; } = new();
}
