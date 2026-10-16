using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفات استعلامات أوامر التحويل المخزني مع تضمين المستودع المصدر والمستودع الهدف وبنود التحويل وتفاصيل الأصناف والباركودات.
/// </summary>
internal sealed class StockTransferWithDetailsSpec : BaseSpecification<StockTransfer>
{
    /// <summary>
    /// جلب كافة أوامر التحويل مرتبة تنازلياً بتاريخ التحويل مع كامل التفاصيل والبنود.
    /// </summary>
    public StockTransferWithDetailsSpec()
    {
        // تضمين بيانات المستودع المصدر
        AddInclude(s => s.FromWarehouse);
        // تضمين بيانات المستودع الوجهة
        AddInclude(s => s.ToWarehouse);
        // تضمين بيانات منتجات البنود
        AddInclude("Items.Product");
        // تضمين بيانات باركودات البنود
        AddInclude("Items.ProductBarCode");
        // ترتيب النتائج تنازلياً بتاريخ التحويل
        ApplyOrderByDescending(s => s.TransferDate);
    }

    /// <summary>
    /// جلب أمر تحويل محدد بواسطة المعرف مع تفاصيله وبنوده.
    /// </summary>
    /// <param name = "id">معرف أمر التحويل</param>
    public StockTransferWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        // تضمين بيانات المستودع المصدر
        AddInclude(s => s.FromWarehouse);
        // تضمين بيانات المستودع الوجهة
        AddInclude(s => s.ToWarehouse);
        // تضمين بيانات المنتج التابع للبند
        AddInclude("Items.Product");
        // تضمين بيانات باركود البند
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// جلب أمر تحويل بواسطة رقم أمر التحويل مع التفاصيل.
    /// </summary>
    /// <param name = "transferNumber">رقم أمر التحويل</param>
    public StockTransferWithDetailsSpec(string transferNumber) : base(s => s.TransferNumber == transferNumber)
    {
        // تضمين بيانات المستودع المصدر
        AddInclude(s => s.FromWarehouse);
        // تضمين بيانات المستودع الوجهة
        AddInclude(s => s.ToWarehouse);
        // تضمين بيانات المنتج التابع للبند
        AddInclude("Items.Product");
        // تضمين بيانات باركود البند
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// فلترة أوامر التحويل بالمستودع المصدر والمستودع الهدف والحالة والتاريخ والبحث برقم التحويل.
    /// </summary>
    /// <param name = "fromWarehouseId">معرف المستودع المصدر (اختياري)</param>
    /// <param name = "toWarehouseId">معرف المستودع الوجهة (اختياري)</param>
    /// <param name = "status">حالة أمر التحويل (اختياري)</param>
    /// <param name = "fromDate">تاريخ البداية (اختياري)</param>
    /// <param name = "toDate">تاريخ النهاية (اختياري)</param>
    /// <param name = "search">نص البحث برقم التحويل (اختياري)</param>
    public StockTransferWithDetailsSpec(Guid? fromWarehouseId, Guid? toWarehouseId, StockTransferStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null) : base(s => (!fromWarehouseId.HasValue || s.FromWarehouseId == fromWarehouseId.Value) && (!toWarehouseId.HasValue || s.ToWarehouseId == toWarehouseId.Value) && (!status.HasValue || s.Status == status.Value) && (!fromDate.HasValue || s.TransferDate >= fromDate.Value) && (!toDate.HasValue || s.TransferDate <= toDate.Value) && (string.IsNullOrWhiteSpace(search) || s.TransferNumber.Contains(search)))
    {
        // تضمين بيانات المستودع المصدر
        AddInclude(s => s.FromWarehouse);
        // تضمين بيانات المستودع الوجهة
        AddInclude(s => s.ToWarehouse);
        // تضمين بيانات منتجات البنود
        AddInclude("Items.Product");
        // تضمين بيانات باركودات البنود
        AddInclude("Items.ProductBarCode");
        // ترتيب النتائج تنازلياً بتاريخ التحويل
        ApplyOrderByDescending(s => s.TransferDate);
    }
}
