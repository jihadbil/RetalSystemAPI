using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب التصنيفات الجذرية الرئيسية (بدون أب) مع تفاصيلها.
/// </summary>
internal sealed class RootCategoriesSpec : BaseSpecification<Category>
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
