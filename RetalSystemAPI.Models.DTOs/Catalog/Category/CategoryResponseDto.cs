using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.Category;

/// <summary>
/// ناقل بيانات استجابة تصنيف الأصناف (Category Response DTO).
/// يعرض تفاصيل الفئة مع الفئة الأب، عدد الأصناف، والتصنيفات الفرعية المتداخلة شجرياً.
/// </summary>
public class CategoryResponseDto : BaseDto
{
    /// <summary>اسم التصنيف</summary>
    public string Name { get; set; } = null!;

    /// <summary>معرف التصنيف الأب إن وجد</summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>اسم التصنيف الأب</summary>
    public string? ParentCategoryName { get; set; }

    /// <summary>حالة نشاط التصنيف</summary>
    public bool IsActive { get; set; }

    /// <summary>ترتيب العرض</summary>
    public int SortOrder { get; set; }

    /// <summary>إجمالي عدد الأصناف المندرجة تحت هذا التصنيف</summary>
    public int ProductCount { get; set; }

    /// <summary>قائمة التصنيفات الفرعية المتفرعة من هذا التصنيف</summary>
    public List<CategoryResponseDto> SubCategories { get; set; } = new();
}
