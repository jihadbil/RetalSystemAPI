using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة القوائم والترقيم لمرتجعات المبيعات — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمينات العميقة Items.Product / Items.ProductBarCode التي تحتاجها استعلامات التفاصيل فقط.
/// </summary>
internal sealed class SalesReturnListSpec : BaseSpecification<SalesReturn>
{
    /// <summary>
    /// جلب كافة مرتجعات المبيعات للقوائم مرتبة تنازلياً بتاريخ المرتجع.
    /// </summary>
    public SalesReturnListSpec()
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات الفاتورة الأصلية
        AddInclude(s => s.OriginalInvoice!);
        // تضمين البنود لحساب عددها
        AddInclude(s => s.Items);
        // الترتيب تنازلياً بتاريخ المرتجع
        ApplyOrderByDescending(s => s.ReturnDate);
    }

    /// <summary>
    /// فلترة قوائم مرتجعات المبيعات حسب الفرع والمستودع والعميل وسبب الإرجاع والتاريخ والبحث.
    /// </summary>
    /// <param name = "branchId">معرف الفرع (اختياري)</param>
    /// <param name = "warehouseId">معرف المستودع (اختياري)</param>
    /// <param name = "customerId">معرف العميل (اختياري)</param>
    /// <param name = "reason">سبب الإرجاع (اختياري)</param>
    /// <param name = "fromDate">تاريخ البداية (اختياري)</param>
    /// <param name = "toDate">تاريخ النهاية (اختياري)</param>
    /// <param name = "search">نص البحث (اختياري)</param>
    public SalesReturnListSpec(Guid? branchId, Guid? warehouseId, Guid? customerId, SalesReturnReason? reason = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null) : base(s => (!branchId.HasValue || s.BranchId == branchId.Value) && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value) && (!customerId.HasValue || s.CustomerId == customerId.Value) && (!reason.HasValue || s.Reason == reason.Value) && (!fromDate.HasValue || s.ReturnDate >= fromDate.Value) && (!toDate.HasValue || s.ReturnDate <= toDate.Value) && (string.IsNullOrWhiteSpace(search) || s.ReturnNumber.Contains(search) || (s.Customer != null && s.Customer.Name.Contains(search))))
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات الفاتورة الأصلية
        AddInclude(s => s.OriginalInvoice!);
        // تضمين البنود
        AddInclude(s => s.Items);
        // الترتيب تنازلياً بتاريخ المرتجع
        ApplyOrderByDescending(s => s.ReturnDate);
    }
}
