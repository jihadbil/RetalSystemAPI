using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.Services.Catalog.Specifications;

/// <summary>
/// تخصيص استعلامات التصنيفات (Category Specifications).
/// </summary>
public class CategoryWithDetailsSpec : BaseSpecification<Category>
{
    public CategoryWithDetailsSpec()
    {
        AddInclude(c => c.ParentCategory!);
        AddInclude(c => c.Products);
        AddInclude(c => c.SubCategories);
        ApplyOrderBy(c => c.SortOrder);
    }

    public CategoryWithDetailsSpec(Guid id) : base(c => c.Id == id)
    {
        AddInclude(c => c.ParentCategory!);
        AddInclude(c => c.Products);
        AddInclude(c => c.SubCategories);
    }
}

public class RootCategoriesSpec : BaseSpecification<Category>
{
    public RootCategoriesSpec() : base(c => c.ParentCategoryId == null)
    {
        AddInclude(c => c.ParentCategory!);
        AddInclude(c => c.Products);
        AddInclude(c => c.SubCategories);
        ApplyOrderBy(c => c.SortOrder);
    }
}

public class SubCategoriesSpec : BaseSpecification<Category>
{
    public SubCategoriesSpec(Guid parentId) : base(c => c.ParentCategoryId == parentId)
    {
        AddInclude(c => c.ParentCategory!);
        AddInclude(c => c.Products);
        AddInclude(c => c.SubCategories);
        ApplyOrderBy(c => c.SortOrder);
    }
}
