using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب كافة الباركودات مع البحث بالرقم أو العنوان أو اسم المنتج
/// (تضمّن المنتج وصوره — حقل Images في الاستجابة يعتمد عليهما).
/// </summary>
internal sealed class AllProductBarCodesSpec : BaseSpecification<ProductBarCode>
{
    /// <summary>
    /// تهيئة مواصفة البحث الشامل في الباركودات بالرقم أو العنوان أو اسم المنتج.
    /// </summary>
    /// <param name = "search">نص البحث الاختياري</param>
    public AllProductBarCodesSpec(string? search = null) : base(b => string.IsNullOrWhiteSpace(search) || b.BarCode.Contains(search) || b.Title.Contains(search) || (b.Product != null && b.Product.Name.Contains(search)))
    {
        // تضمين المنتج
        AddInclude(b => b.Product!);
        // تضمين الصور
        AddInclude(b => b.ProductImages);
    }
}
