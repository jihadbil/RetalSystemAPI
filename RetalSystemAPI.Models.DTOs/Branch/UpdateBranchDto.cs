using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Branch;

/// <summary>
/// ناقل بيانات تعديل بيانات فرع قائم (Update Branch DTO).
/// </summary>
public class UpdateBranchDto
{
    /// <summary>المعرف الفريد للفرع المطلوب تعديله</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid Id { get; set; }

    /// <summary>اسم الفرع الجديد</summary>
    [Required(ErrorMessage = "اسم الفرع مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم الفرع يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>العنوان الجديد</summary>
    public string? Address { get; set; }

    /// <summary>حالة نشاط الفرع</summary>
    public bool IsActive { get; set; }

    /// <summary>قائمة أرقام هواتف الفرع المحدثة</summary>
    public List<BranchPhoneDto> Phones { get; set; } = new();
}
