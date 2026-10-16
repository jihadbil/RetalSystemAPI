using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// تخصيص استعلامات رصيد صالة العرض الأدنى (Low Showroom Stock) لتنبيهات نقص الكميات ووصولها لحد إعادة الطلب.
/// </summary>
internal sealed class LowShowroomStockSpec : BaseSpecification<ShowroomStock>
{
    /// <summary>
    /// تهيئة مواصفة تنبيه نقص مخزون صالة العرض وتضمين بيانات المستودع والمنتج.
    /// </summary>
    /// <param name = "warehouseId">معرف الصالة الاختياري</param>
    public LowShowroomStockSpec(Guid? warehouseId = null) : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات المنتج
        AddInclude(s => s.Product);
    }
}
