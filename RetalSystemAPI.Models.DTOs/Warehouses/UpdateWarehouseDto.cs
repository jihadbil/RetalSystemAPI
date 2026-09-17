using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses;

/// <summary>
/// ناقل بيانات تعديل مستودع قائم (Update Warehouse DTO).
/// </summary>
public class UpdateWarehouseDto
{
    /// <summary>المعرف الفريد للمستودع المطلوب تعديله</summary>
    [Required(ErrorMessage = "معرف المخزن مطلوب")]
    public Guid Id { get; set; }

    /// <summary>اسم المستودع الجديد</summary>
    [Required(ErrorMessage = "اسم المخزن مطلوب")]
    [MaxLength(150, ErrorMessage = "اسم المخزن يجب أن لا يتجاوز 150 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>حالة نشاط وتشغيل المستودع</summary>
    public bool IsActive { get; set; }
}
