using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة فلترة فواتير المشتريات وتصفحها بالترقيم مع فلاتر التاريخ والمورد والفرع والمستودع والحالة وطريقة الدفع.
/// </summary>
internal sealed class PurchaseInvoiceFilterSpec : BaseSpecification<PurchaseInvoice>
{
    /// <summary>
    /// تهيئة مواصفة فلترة فواتير المشتريات مع دعم مختلف محددات التصفية والبحث بالنص.
    /// </summary>
    /// <param name = "supplierId">معرف المورد للفلترة (اختياري)</param>
    /// <param name = "branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name = "warehouseId">معرف المستودع للفلترة (اختياري)</param>
    /// <param name = "status">حالة الفاتورة (اختياري)</param>
    /// <param name = "paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name = "fromDate">تاريخ البداية (اختياري)</param>
    /// <param name = "toDate">تاريخ النهاية (اختياري)</param>
    /// <param name = "searchTerm">مصطلح البحث برقم الفاتورة أو اسم المورد (اختياري)</param>
    public PurchaseInvoiceFilterSpec(Guid? supplierId = null, Guid? branchId = null, Guid? warehouseId = null, InvoiceStatus? status = null, PaymentMethod? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, string? searchTerm = null) : base(p => (!supplierId.HasValue || p.SupplierId == supplierId.Value) && (!branchId.HasValue || p.BranchId == branchId.Value) && (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) && (!status.HasValue || p.Status == status.Value) && (!paymentMethod.HasValue || p.PaymentMethod == paymentMethod.Value) && (!fromDate.HasValue || p.InvoiceDate >= fromDate.Value) && (!toDate.HasValue || p.InvoiceDate <= toDate.Value) && (string.IsNullOrWhiteSpace(searchTerm) || p.InvoiceNumber.Contains(searchTerm) || (p.Supplier != null && p.Supplier.Name.Contains(searchTerm))))
    {
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier);
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse);
        // تضمين قائمة البنود السطحية لحساب الإجماليات
        AddInclude(p => p.Items);
        // ترتيب الفواتير تنازلياً حسب تاريخ الفاتورة
        ApplyOrderByDescending(p => p.InvoiceDate);
    }
}
