using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة القوائم والترقيم لمرتجعات المشتريات — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمينات العميقة Items.Product / Items.ProductBarCode التي تحتاجها استعلامات التفاصيل فقط.
/// </summary>
internal sealed class PurchaseReturnListSpec : BaseSpecification<PurchaseReturn>
{
    /// <summary>
    /// جلب كافة مرتجعات المشتريات للقوائم مرتبة تنازلياً بتاريخ الإرجاع.
    /// </summary>
    public PurchaseReturnListSpec()
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier);
        // تضمين بيانات فاتورة الشراء
        AddInclude(p => p.PurchaseInvoice!);
        // تضمين البنود السطحية لحساب العدد
        AddInclude(p => p.Items);
        // ترتيب النتائج تنازلياً حسب تاريخ المرتجع
        ApplyOrderByDescending(p => p.ReturnDate);
    }

    /// <summary>
    /// فلترة قوائم مرتجعات المشتريات حسب المورد والفرع والمستودع وسبب الإرجاع وطريقة الدفع والتواريخ والبحث.
    /// </summary>
    /// <param name = "branchId">معرف الفرع (اختياري)</param>
    /// <param name = "warehouseId">معرف المستودع (اختياري)</param>
    /// <param name = "supplierId">معرف المورد (اختياري)</param>
    /// <param name = "reason">سبب الإرجاع (اختياري)</param>
    /// <param name = "paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name = "fromDate">تاريخ البداية (اختياري)</param>
    /// <param name = "toDate">تاريخ النهاية (اختياري)</param>
    /// <param name = "search">نص البحث (اختياري)</param>
    public PurchaseReturnListSpec(Guid? branchId = null, Guid? warehouseId = null, Guid? supplierId = null, PurchaseReturnReason? reason = null, PaymentMethod? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null) : base(p => (!branchId.HasValue || p.BranchId == branchId.Value) && (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) && (!supplierId.HasValue || p.SupplierId == supplierId.Value) && (!reason.HasValue || p.Reason == reason.Value) && (!paymentMethod.HasValue || p.PaymentMethod == paymentMethod.Value) && (!fromDate.HasValue || p.ReturnDate >= fromDate.Value) && (!toDate.HasValue || p.ReturnDate <= toDate.Value) && (string.IsNullOrWhiteSpace(search) || p.ReturnNumber.Contains(search) || (p.Supplier != null && p.Supplier.Name.Contains(search)) || (p.Notes != null && p.Notes.Contains(search))))
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier);
        // تضمين بيانات فاتورة الشراء
        AddInclude(p => p.PurchaseInvoice!);
        // تضمين البنود لحساب العدد
        AddInclude(p => p.Items);
        // ترتيب النتائج تنازلياً حسب تاريخ المرتجع
        ApplyOrderByDescending(p => p.ReturnDate);
    }
}
