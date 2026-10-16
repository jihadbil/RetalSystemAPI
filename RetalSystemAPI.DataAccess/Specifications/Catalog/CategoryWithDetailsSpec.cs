using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب التصنيفات مع كافة تفاصيلها (الأب، الأصناف التابعة، والتصنيفات الفرعية).
/// </summary>
internal sealed class CategoryWithDetailsSpec : BaseSpecification<Category>
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
    /// <param name = "id">معرف التصنيف الفريد</param>
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
