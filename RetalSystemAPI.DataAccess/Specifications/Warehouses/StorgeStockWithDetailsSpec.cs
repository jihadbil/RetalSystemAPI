using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// تخصيصات استعلامات أرصدة مخازن التخزين (StorgeStock) مع تضمين المستودع والباركود وبيانات المنتج وصوره.
/// </summary>
internal sealed class StorgeStockWithDetailsSpec : BaseSpecification<StorgeStock>
{
    /// <summary>
    /// جلب كافة أرصدة مخازن التخزين مع تضمين المستودع والباركود والمنتج والصور التابعة.
    /// </summary>
    public StorgeStockWithDetailsSpec()
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

    /// <summary>
    /// جلب أرصدة مخزن تخزين محدد بالمعرف مع التفاصيل الكاملة.
    /// </summary>
    /// <param name = "warehouseId">المعرف الفريد للمستودع</param>
    public StorgeStockWithDetailsSpec(Guid warehouseId) : base(s => s.WarehouseId == warehouseId)
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

    /// <summary>
    /// جلب رصيد باركود محدد في مخزن تخزين معين.
    /// </summary>
    /// <param name = "warehouseId">المعرف الفريد للمستودع</param>
    /// <param name = "productBarcodeId">المعرف الفريد لباركود المنتج</param>
    public StorgeStockWithDetailsSpec(Guid warehouseId, Guid productBarcodeId) : base(s => s.WarehouseId == warehouseId && s.ProductBarcodeId == productBarcodeId)
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات الباركود
        AddInclude(s => s.ProductBarcode);
        // تضمين بيانات المنتج
        AddInclude("ProductBarcode.Product");
        // تضمين صور المنتج
        AddInclude("ProductBarcode.Product.ProductImages");
    }

    /// <summary>
    /// جلب صفحة بيانات مجزأة من أرصدة مخزن تخزين مع الترقيم والبحث بالباركود أو اسم الصنف (أو المطابقة التامة).
    /// </summary>
    /// <param name = "warehouseId">معرف المستودع</param>
    /// <param name = "searchTerm">نص البحث</param>
    /// <param name = "exactBarcode">تفعيل المطابقة التامة للباركود</param>
    public StorgeStockWithDetailsSpec(Guid warehouseId, string? searchTerm = null, bool exactBarcode = false) : base(s => s.WarehouseId == warehouseId && (string.IsNullOrWhiteSpace(searchTerm) || (exactBarcode ? (s.ProductBarcode != null && s.ProductBarcode.BarCode == searchTerm) : ((s.ProductBarcode != null && s.ProductBarcode.BarCode != null && s.ProductBarcode.BarCode.Contains(searchTerm)) || (s.ProductBarcode != null && s.ProductBarcode.Title != null && s.ProductBarcode.Title.Contains(searchTerm)) || (s.ProductBarcode != null && s.ProductBarcode.Product != null && s.ProductBarcode.Product.Name != null && s.ProductBarcode.Product.Name.Contains(searchTerm))))))
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات الباركود
        AddInclude(s => s.ProductBarcode);
        // تضمين بيانات المنتج التابع
        AddInclude("ProductBarcode.Product");
        // تضمين صور المنتج
        AddInclude("ProductBarcode.Product.ProductImages");
    }
}
