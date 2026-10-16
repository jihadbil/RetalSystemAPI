using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// تخصيص استعلامات رصيد التخزين الأدنى (Low Storge Stock) لتنبيهات نقص الأرصدة ووصولها لحد إعادة الطلب.
/// </summary>
internal sealed class LowStorgeStockSpec : BaseSpecification<StorgeStock>
{
    /// <summary>
    /// تهيئة مواصفة تنبيه نقص مخزون التخزين وتضمين بيانات المنتج والصور.
    /// </summary>
    /// <param name = "warehouseId">معرف المستودع الاختياري</param>
    public LowStorgeStockSpec(Guid? warehouseId = null) : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات الباركود
        AddInclude(s => s.ProductBarcode);
        // تضمين بيانات المنتج التابع للباركود
        AddInclude("ProductBarcode.Product");
        // تضمين صور المنتج
        AddInclude("ProductBarcode.Product.ProductImages");
    }
}
