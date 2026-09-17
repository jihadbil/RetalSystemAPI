using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات إنشاء مورد جديد (Create Supplier DTO).
/// </summary>
public class CreateSupplierDto
{
    /// <summary>اسم المورد أو الشركة الموردة</summary>
    [Required(ErrorMessage = "اسم المورد مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم المورد يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>عنوان مقر المورد</summary>
    [MaxLength(500, ErrorMessage = "العنوان يجب أن لا يتجاوز 500 حرف")]
    public string? Address { get; set; }

    /// <summary>الرصيد المالي الافتتاحي المستحق للمورد</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الرصيد الافتتاحي يجب أن يكون أكبر من أو يساوي 0")]
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>قائمة أرقام هواتف المورد</summary>
    public List<SupplierPhoneDto> Phones { get; set; } = new();
}
