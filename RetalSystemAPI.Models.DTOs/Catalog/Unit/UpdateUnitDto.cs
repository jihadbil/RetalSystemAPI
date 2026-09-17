using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.Unit;

/// <summary>
/// ناقل بيانات تعديل وحدة قياس قائمة (Update Unit DTO).
/// </summary>
public class UpdateUnitDto
{
    /// <summary>المعرف الفريد للوحدة المطلوب تعديلها</summary>
    [Required(ErrorMessage = "معرف الوحدة مطلوب")]
    public Guid Id { get; set; }

    /// <summary>اسم الوحدة الجديد</summary>
    [Required(ErrorMessage = "اسم الوحدة مطلوب")]
    [MaxLength(100, ErrorMessage = "اسم الوحدة يجب أن لا يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>وصف الوحدة</summary>
    public string? Description { get; set; }

    /// <summary>عدد القطع التي تحتويها هذه العبوة</summary>
    [Range(1, int.MaxValue, ErrorMessage = "عدد القطع يجب أن يكون 1 على الأقل")]
    public int UnitPackage { get; set; } = 1;
}
