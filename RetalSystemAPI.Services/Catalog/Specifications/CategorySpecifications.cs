using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.Services.Catalog.Specifications;

/// <summary>
/// مواصفة جلب التصنيفات مع كافة تفاصيلها (الأب، الأصناف التابعة، والتصنيفات الفرعية).
/// </summary>
public class CategoryWithDetailsSpec : BaseSpecification<Category>
{
    /// <summary>جلب كافة التصنيفات مع تفاصيلها مرتبة حسب SortOrder</summary>
    public CategoryWithDetailsSpec()
    {
        AddInclude(c => c.ParentCategory!);
        AddInclude(c => c.Products);
        AddInclude(c => c.SubCategories);
        ApplyOrderBy(c => c.SortOrder);
    }

    /// <summary>جلب تصنيف محدد بالمعرف مع تفاصيله</summary>
    /// <param name="id">معرف التصنيف</param>
    public CategoryWithDetailsSpec(Guid id) : base(c => c.Id == id)
    {
        AddInclude(c => c.ParentCategory!);
        AddInclude(c => c.Products);
        AddInclude(c => c.SubCategories);
    }
}

/// <summary>
/// مواصفة جلب التصنيفات الجذرية الرئيسية (بدون أب) مع تفاصيلها.
/// </summary>
public class RootCategoriesSpec : BaseSpecification<Category>
{
    /// <summary>تهيئة مواصفة التصنيفات الجذرية</summary>
    public RootCategoriesSpec() : base(c => c.ParentCategoryId == null)
    {
        AddInclude(c => c.ParentCategory!);
        AddInclude(c => c.Products);
        AddInclude(c => c.SubCategories);
        ApplyOrderBy(c => c.SortOrder);
    }
}

/// <summary>
/// مواصفة جلب التصنيفات الفرعية التابعة لتصنيف أب محدد.
/// </summary>
public class SubCategoriesSpec : BaseSpecification<Category>
{
    /// <summary>تهيئة مواصفة التصنيفات الفرعية لمعرف الأب</summary>
    /// <param name="parentId">معرف التصنيف الأب</param>
    public SubCategoriesSpec(Guid parentId) : base(c => c.ParentCategoryId == parentId)
    {
        AddInclude(c => c.ParentCategory!);
        AddInclude(c => c.Products);
        AddInclude(c => c.SubCategories);
        ApplyOrderBy(c => c.SortOrder);
    }
}
