using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب كافة وحدات القياس المرتبطة بمنتج محدد مع تضمين بيانات الوحدة الأساسية.
/// </summary>
internal sealed class ProductUnitsByProductSpec : BaseSpecification<ProductUnit>
{
    /// <summary>
    /// تهيئة مواصفة وحدات المنتج بالمعرف وتضمين الوحدة المرتبطة.
    /// </summary>
    /// <param name = "productId">معرف المنتج</param>
    public ProductUnitsByProductSpec(Guid productId) : base(pu => pu.ProductId == productId)
    {
        // تضمين بيانات وحدة القياس التابعة
        AddInclude(pu => pu.Unit!);
    }
}
