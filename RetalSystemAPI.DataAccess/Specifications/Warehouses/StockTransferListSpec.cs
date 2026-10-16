using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة القوائم والترقيم لأوامر التحويل المخزني — المستودعان وبنود الجذر فقط (لعدد البنود)
/// دون التضمينات العميقة Items.Product / Items.ProductBarCode التي تحتاجها استعلامات التفاصيل فقط.
/// </summary>
internal sealed class StockTransferListSpec : BaseSpecification<StockTransfer>
{
    /// <summary>
    /// جلب كافة أوامر التحويل للقوائم مرتبة تنازلياً بتاريخ التحويل.
    /// </summary>
    public StockTransferListSpec()
    {
        // تضمين بيانات المستودع المصدر
        AddInclude(s => s.FromWarehouse);
        // تضمين بيانات المستودع الوجهة
        AddInclude(s => s.ToWarehouse);
        // تضمين البنود لحساب عددها دون تفاصيل الأصناف
        AddInclude(s => s.Items);
        // الترتيب تنازلياً بتاريخ التحويل
        ApplyOrderByDescending(s => s.TransferDate);
    }

    /// <summary>
    /// فلترة قوائم أوامر التحويل بالمستودع المصدر والمستودع الهدف والحالة والتاريخ والبحث.
    /// </summary>
    /// <param name = "fromWarehouseId">معرف المستودع المصدر (اختياري)</param>
    /// <param name = "toWarehouseId">معرف المستودع الوجهة (اختياري)</param>
    /// <param name = "status">حالة أمر التحويل (اختياري)</param>
    /// <param name = "fromDate">تاريخ البداية (اختياري)</param>
    /// <param name = "toDate">تاريخ النهاية (اختياري)</param>
    /// <param name = "search">نص البحث (اختياري)</param>
    public StockTransferListSpec(Guid? fromWarehouseId, Guid? toWarehouseId, StockTransferStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null) : base(s => (!fromWarehouseId.HasValue || s.FromWarehouseId == fromWarehouseId.Value) && (!toWarehouseId.HasValue || s.ToWarehouseId == toWarehouseId.Value) && (!status.HasValue || s.Status == status.Value) && (!fromDate.HasValue || s.TransferDate >= fromDate.Value) && (!toDate.HasValue || s.TransferDate <= toDate.Value) && (string.IsNullOrWhiteSpace(search) || s.TransferNumber.Contains(search)))
    {
        // تضمين بيانات المستودع المصدر
        AddInclude(s => s.FromWarehouse);
        // تضمين بيانات المستودع الوجهة
        AddInclude(s => s.ToWarehouse);
        // تضمين البنود لحساب عددها
        AddInclude(s => s.Items);
        // الترتيب تنازلياً بتاريخ التحويل
        ApplyOrderByDescending(s => s.TransferDate);
    }
}
