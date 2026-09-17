using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.Unit;

/// <summary>
/// ناقل بيانات إنشاء وحدة قياس جديدة (Create Unit DTO).
/// </summary>
public class CreateUnitDto
{
    /// <summary>اسم وحدة القياس (مثل: حبة، درزن، كرتونة)</summary>
    [Required(ErrorMessage = "اسم الوحدة مطلوب")]
    [MaxLength(100, ErrorMessage = "اسم الوحدة يجب أن لا يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>وصف الوحدة</summary>
    public string? Description { get; set; }

    /// <summary>عدد القطع التي تحتويها هذه العبوة أو الوحدة</summary>
    [Range(1, int.MaxValue, ErrorMessage = "عدد القطع يجب أن يكون 1 على الأقل")]
    public int UnitPackage { get; set; } = 1;
}
