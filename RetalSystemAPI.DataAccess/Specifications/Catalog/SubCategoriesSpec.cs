using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب التصنيفات الفرعية التابعة لتصنيف أب محدد.
/// </summary>
internal sealed class SubCategoriesSpec : BaseSpecification<Category>
{
    /// <summary>
    /// تهيئة مواصفة التصنيفات الفرعية لمعرف الأب.
    /// </summary>
    /// <param name = "parentId">معرف التصنيف الأب</param>
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
