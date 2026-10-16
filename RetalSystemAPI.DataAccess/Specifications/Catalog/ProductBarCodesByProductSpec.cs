using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب جميع باركودات صنف محدد مع المنتج وصوره.
/// </summary>
internal sealed class ProductBarCodesByProductSpec : BaseSpecification<ProductBarCode>
{
    /// <summary>
    /// تهيئة مواصفة باركودات الصنف بالمعرف وتضمين المنتج والصور.
    /// </summary>
    /// <param name = "productId">معرف المنتج</param>
    public ProductBarCodesByProductSpec(Guid productId) : base(b => b.ProductId == productId)
    {
        // تضمين بيانات المنتج التابع له الباركود
        AddInclude(b => b.Product!);
        // تضمين الصور المخصصة لهذا الباركود أو النكهة
        AddInclude(b => b.ProductImages);
    }
}
