using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Branch;

/// <summary>
/// ناقل بيانات استجابة هاتف الفرع (Branch Phone Response DTO).
/// </summary>
public class BranchPhoneResponseDto : BaseDto
{
    /// <summary>اسم جهة الاتصال أو المسمى الوظيفي للهاتف</summary>
    public string? Name { get; set; }

    /// <summary>رقم الهاتف المسجل</summary>
    public string PhoneNumber { get; set; } = null!;
}
