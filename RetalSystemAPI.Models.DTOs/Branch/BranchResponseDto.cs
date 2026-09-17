using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Branch;

/// <summary>
/// ناقل بيانات استجابة تفاصيل الفرع (Branch Response DTO).
/// يتضمن الاسم، العنوان، حالة التفعيل، وقائمة أرقام هواتف الفرع.
/// </summary>
public class BranchResponseDto : BaseDto
{
    /// <summary>اسم الفرع</summary>
    public string Name { get; set; } = null!;

    /// <summary>عنوان وموقع الفرع</summary>
    public string? Address { get; set; }

    /// <summary>حالة نشاط وتشغيل الفرع</summary>
    public bool IsActive { get; set; }

    /// <summary>قائمة أرقام هواتف الفرع</summary>
    public List<BranchPhoneResponseDto> Phones { get; set; } = new();
}
