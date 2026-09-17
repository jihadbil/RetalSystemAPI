using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.Category;

/// <summary>
/// ناقل بيانات إنشاء تصنيف جديد للأصناف (Create Category DTO).
/// </summary>
public class CreateCategoryDto
{
    /// <summary>اسم التصنيف الجديد</summary>
    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم التصنيف يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>معرف التصنيف الأب في حال إنشاء تصنيف فرعي</summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>حالة نشاط التصنيف (افتراضي: مفعل)</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>ترتيب ظهور التصنيف في القوائم</summary>
    public int SortOrder { get; set; } = 0;
}
