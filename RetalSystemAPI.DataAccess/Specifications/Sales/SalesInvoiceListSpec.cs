using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة القوائم والترقيم لفواتير المبيعات — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمينات العميقة Items.Product / Items.ProductBarCode التي تحتاجها استعلامات التفاصيل فقط.
/// </summary>
internal sealed class SalesInvoiceListSpec : BaseSpecification<SalesInvoice>
{
    /// <summary>
    /// جلب كافة فواتير المبيعات للقوائم مرتبة تنازلياً بتاريخ الفاتورة.
    /// </summary>
    public SalesInvoiceListSpec()
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين قائمة البنود لحساب عددها
        AddInclude(s => s.Items);
        // الترتيب تنازلياً بتاريخ الفاتورة
        ApplyOrderByDescending(s => s.InvoiceDate);
    }

    /// <summary>
    /// فلترة قوائم فواتير المبيعات بالفرع والمستودع والعميل والحالة وطريقة الدفع والتاريخ والبحث.
    /// </summary>
    /// <param name = "branchId">معرف الفرع (اختياري)</param>
    /// <param name = "warehouseId">معرف المستودع (اختياري)</param>
    /// <param name = "customerId">معرف العميل (اختياري)</param>
    /// <param name = "status">حالة الفاتورة (اختياري)</param>
    /// <param name = "paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name = "fromDate">تاريخ البداية (اختياري)</param>
    /// <param name = "toDate">تاريخ النهاية (اختياري)</param>
    /// <param name = "search">نص البحث (اختياري)</param>
    public SalesInvoiceListSpec(Guid? branchId, Guid? warehouseId, Guid? customerId, InvoiceStatus? status, PaymentMethod? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null) : base(s => (!branchId.HasValue || s.BranchId == branchId.Value) && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value) && (!customerId.HasValue || s.CustomerId == customerId.Value) && (!status.HasValue || s.Status == status.Value) && (!paymentMethod.HasValue || s.PaymentMethod == paymentMethod.Value) && (!fromDate.HasValue || s.InvoiceDate >= fromDate.Value) && (!toDate.HasValue || s.InvoiceDate <= toDate.Value) && (string.IsNullOrWhiteSpace(search) || s.InvoiceNumber.Contains(search) || (s.Customer != null && s.Customer.Name.Contains(search))))
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بنود الفاتورة
        AddInclude(s => s.Items);
        // الترتيب تنازلياً بتاريخ الفاتورة
        ApplyOrderByDescending(s => s.InvoiceDate);
    }
}
