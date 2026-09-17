using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.Services.Catalog.Specifications;

/// <summary>
/// مواصفة جلب التصنيفات مع كافة تفاصيلها (الأب، الأصناف التابعة، والتصنيفات الفرعية).
/// </summary>
public class CategoryWithDetailsSpec : BaseSpecification<Category>
{
    /// <summary>
    /// جلب كافة التصنيفات مع تفاصيلها مرتبة حسب ترتيب العرض.
    /// </summary>
    public CategoryWithDetailsSpec()
    {
        // تضمين بيانات التصنيف الأب المباشر
        AddInclude(c => c.ParentCategory!);
        // تضمين قائمة الأصناف التابعة لهذا التصنيف
        AddInclude(c => c.Products);
        // تضمين قائمة التصنيفات الفرعية التابعة
        AddInclude(c => c.SubCategories);
        // ترتيب التصنيفات تصاعدياً حسب حقل SortOrder
        ApplyOrderBy(c => c.SortOrder);
    }

    /// <summary>
    /// جلب تصنيف محدد بالمعرف مع تفاصيله.
    /// </summary>
    /// <param name="id">معرف التصنيف الفريد</param>
    public CategoryWithDetailsSpec(Guid id) : base(c => c.Id == id)
    {
        // تضمين بيانات التصنيف الأب
        AddInclude(c => c.ParentCategory!);
        // تضمين الأصناف التابعة للتصنيف
        AddInclude(c => c.Products);
        // تضمين التصنيفات الفرعية
        AddInclude(c => c.SubCategories);
    }
}

/// <summary>
/// مواصفة جلب التصنيفات الجذرية الرئيسية (بدون أب) مع تفاصيلها.
/// </summary>
public class RootCategoriesSpec : BaseSpecification<Category>
{
    /// <summary>
    /// تهيئة مواصفة التصنيفات الجذرية مع ترتيب العرض وتضمين التفرعات.
    /// </summary>
    public RootCategoriesSpec() : base(c => c.ParentCategoryId == null)
    {
        // تضمين بيانات التصنيف الأب (فارغ للجذري)
        AddInclude(c => c.ParentCategory!);
        // تضمين الأصناف
        AddInclude(c => c.Products);
        // تضمين الفروع التابعة للتصنيف الجذري
        AddInclude(c => c.SubCategories);
        // تطبيق الترتيب التصاعدي حسب ترتيب العرض
        ApplyOrderBy(c => c.SortOrder);
    }
}

/// <summary>
/// مواصفة جلب التصنيفات الفرعية التابعة لتصنيف أب محدد.
/// </summary>
public class SubCategoriesSpec : BaseSpecification<Category>
{
    /// <summary>
    /// تهيئة مواصفة التصنيفات الفرعية لمعرف الأب.
    /// </summary>
    /// <param name="parentId">معرف التصنيف الأب</param>
    public SubCategoriesSpec(Guid parentId) : base(c => c.ParentCategoryId == parentId)
    {
        // تضمين التصنيف الأب
        AddInclude(c => c.ParentCategory!);
        // تضمين الأصناف
        AddInclude(c => c.Products);
        // تضمين التصنيفات الفرعية الأدنى
        AddInclude(c => c.SubCategories);
        // الترتيب حسب حقل العرض
        ApplyOrderBy(c => c.SortOrder);
    }
}
