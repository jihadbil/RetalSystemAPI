using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Branch;

/// <summary>
/// ناقل بيانات إنشاء فرع جديد (Create Branch DTO).
/// </summary>
public class CreateBranchDto
{
    /// <summary>اسم الفرع</summary>
    [Required(ErrorMessage = "اسم الفرع مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم الفرع يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>عنوان وموقع الفرع</summary>
    public string? Address { get; set; }

    /// <summary>حالة نشاط الفرع (افتراضي: مفعل)</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>قائمة أرقام هواتف الفرع</summary>
    public List<BranchPhoneDto> Phones { get; set; } = new();
}
