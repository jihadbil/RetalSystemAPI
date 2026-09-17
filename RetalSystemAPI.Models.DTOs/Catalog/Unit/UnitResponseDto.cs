using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.Unit;

/// <summary>
/// ناقل بيانات استجابة وحدة القياس (Unit Response DTO).
/// </summary>
public class UnitResponseDto : BaseDto
{
    /// <summary>اسم وحدة القياس</summary>
    public string Name { get; set; } = null!;

    /// <summary>وصف الوحدة</summary>
    public string? Description { get; set; }

    /// <summary>عدد القطع المتضمنة</summary>
    public int UnitPackage { get; set; }
}
