using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات تعديل بيانات مورد قائم (Update Supplier DTO).
/// </summary>
public class UpdateSupplierDto
{
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
    public List<SupplierPhoneDto> Phones { get; set; } = new();
}
