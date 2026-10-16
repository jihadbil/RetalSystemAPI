using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة القوائم والترقيم لأوامر الشراء — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمين العميق Items.ProductBarCode.Product الذي تحتاجه استعلامات التفاصيل فقط.
/// </summary>
internal sealed class PurchaseOrderListSpec : BaseSpecification<PurchaseOrder>
{
    /// <summary>
    /// جلب كافة أوامر الشراء للقوائم مرتبة تنازلياً بتاريخ الطلبية.
    /// </summary>
    public PurchaseOrderListSpec()
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse!);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier!);
        // تضمين قائمة البنود السطحية لحساب العدد والإجماليات الخفيفة
        AddInclude(p => p.Items);
        // ترتيب النتائج تنازلياً حسب تاريخ الطلبية
        ApplyOrderByDescending(p => p.OrderDate);
    }

    /// <summary>
    /// فلترة قوائم أوامر الشراء حسب الفرع والمستودع والحالة والبحث برقم الطلبية.
    /// </summary>
    /// <param name = "branchId">معرف الفرع (اختياري)</param>
    /// <param name = "warehouseId">معرف المستودع (اختياري)</param>
    /// <param name = "status">حالة أمر الشراء (اختياري)</param>
    /// <param name = "search">نص البحث في رقم الطلبية (اختياري)</param>
    public PurchaseOrderListSpec(Guid? branchId, Guid? warehouseId, PurchaseOrderStatus? status, string? search = null) : base(p => (!branchId.HasValue || p.BranchId == branchId.Value) && (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) && (!status.HasValue || p.Status == status.Value) && (string.IsNullOrWhiteSpace(search) || p.OrderNumber.Contains(search)))
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse!);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier!);
        // تضمين البنود لحساب الأعداد
        AddInclude(p => p.Items);
        // ترتيب النتائج تنازلياً حسب تاريخ الطلبية
        ApplyOrderByDescending(p => p.OrderDate);
    }
}
