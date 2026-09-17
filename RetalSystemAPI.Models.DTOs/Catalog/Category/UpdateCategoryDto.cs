using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.Category;

/// <summary>
/// ناقل بيانات تعديل تصنيف أصناف حالي (Update Category DTO).
/// </summary>
public class UpdateCategoryDto
{
    /// <summary>المعرف الفريد للتصنيف المطلوب تعديله</summary>
    [Required(ErrorMessage = "معرف التصنيف مطلوب")]
    public Guid Id { get; set; }

    /// <summary>اسم التصنيف الجديد</summary>
    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم التصنيف يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>معرف التصنيف الأب</summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>حالة نشاط التصنيف</summary>
    public bool IsActive { get; set; }

    /// <summary>ترتيب العرض</summary>
    public int SortOrder { get; set; }
}
