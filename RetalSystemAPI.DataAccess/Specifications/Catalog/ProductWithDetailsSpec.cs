using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب صنف محدد مع كافة تفاصيله الدقيقة (التصنيف، الوحدات، الباركودات وصورها، وصور المنتج).
/// </summary>
internal sealed class ProductWithDetailsSpec : BaseSpecification<Product>
{
    /// <summary>
    /// تهيئة مواصفة الصنف المفصل بالمعرف.
    /// </summary>
    /// <param name = "id">معرف الصنف الفريد</param>
    public ProductWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        // تضمين بيانات تصنيف الصنف
        AddInclude(p => p.Category!);
        // تضمين وحدات القياس الخاصة بالصنف مع تعريف كل وحدة
        AddInclude("ProductUnits.Unit");
        // تضمين باركودات الصنف وصور كل باركود/نكهة
        AddInclude("ProductBarCodes.ProductImages");
        // تضمين صور المنتج العامة
        AddInclude(p => p.ProductImages);
    }
}
