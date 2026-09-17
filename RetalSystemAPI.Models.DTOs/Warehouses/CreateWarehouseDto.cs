using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses;

/// <summary>
/// ناقل بيانات إنشاء مستودع أو مخزن أو صالة عرض جديدة (Create Warehouse DTO).
/// </summary>
public class CreateWarehouseDto
{
    /// <summary>اسم المستودع أو المخزن</summary>
    [Required(ErrorMessage = "اسم المخزن مطلوب")]
    [MaxLength(150, ErrorMessage = "اسم المخزن يجب أن لا يتجاوز 150 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>معرف الفرع التابع له المستودع</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    /// <summary>نوع المستودع (مخزن رئيسي تخزيني / صالة عرض)</summary>
    [Required(ErrorMessage = "نوع المخزن مطلوب")]
    public WarehouseType Type { get; set; }

    /// <summary>حالة نشاط المستودع (افتراضي: مفعل)</summary>
    public bool IsActive { get; set; } = true;
}
