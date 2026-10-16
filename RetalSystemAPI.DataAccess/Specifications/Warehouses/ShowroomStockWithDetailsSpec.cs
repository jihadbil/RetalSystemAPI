using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// تخصيصات استعلامات مخزون صالات العرض (ShowroomStock) مع تضمين المستودع وبيانات المنتج وصوره.
/// </summary>
internal sealed class ShowroomStockWithDetailsSpec : BaseSpecification<ShowroomStock>
{
    /// <summary>
    /// جلب كافة أرصدة صالات العرض مع المستودع والمنتج وصور المنتج.
    /// </summary>
    public ShowroomStockWithDetailsSpec()
    {
        // تضمين بيانات المستودع أو الصالة
        AddInclude(s => s.Warehouse);
        // تضمين بيانات المنتج
        AddInclude(s => s.Product);
        // تضمين صور المنتج
        AddInclude("Product.ProductImages");
    }

    /// <summary>
    /// جلب كافة أرصدة الأصناف لصالة عرض محددة بالمعرف.
    /// </summary>
    /// <param name = "warehouseId">معرف صالة العرض</param>
    public ShowroomStockWithDetailsSpec(Guid warehouseId) : base(s => s.WarehouseId == warehouseId)
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات المنتج
        AddInclude(s => s.Product);
        // تضمين صور المنتج
        AddInclude("Product.ProductImages");
    }

    /// <summary>
    /// جلب رصيد صنف محدد في صالة عرض معينة.
    /// </summary>
    /// <param name = "warehouseId">معرف الصالة</param>
    /// <param name = "productId">معرف المنتج</param>
    public ShowroomStockWithDetailsSpec(Guid warehouseId, Guid productId) : base(s => s.WarehouseId == warehouseId && s.ProductId == productId)
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات المنتج
        AddInclude(s => s.Product);
        // تضمين صور المنتج
        AddInclude("Product.ProductImages");
    }

    /// <summary>
    /// جلب صفحة بيانات مجزأة من أرصدة صالة العرض مع البحث باسم أو وصف الصنف (أو مطابقة باركوداته تماماً).
    /// </summary>
    /// <param name = "warehouseId">معرف الصالة</param>
    /// <param name = "searchTerm">نص البحث</param>
    /// <param name = "exactBarcode">تفعيل المطابقة التامة للباركود</param>
    public ShowroomStockWithDetailsSpec(Guid warehouseId, string? searchTerm = null, bool exactBarcode = false) : base(s => s.WarehouseId == warehouseId && (string.IsNullOrWhiteSpace(searchTerm) || (exactBarcode ? (s.Product != null && s.Product.ProductBarCodes.Any(b => b.BarCode == searchTerm)) : ((s.Product != null && s.Product.Name != null && s.Product.Name.Contains(searchTerm)) || (s.Product != null && s.Product.Description != null && s.Product.Description.Contains(searchTerm))))))
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات المنتج
        AddInclude(s => s.Product);
        // تضمين صور المنتج
        AddInclude("Product.ProductImages");
    }
}
