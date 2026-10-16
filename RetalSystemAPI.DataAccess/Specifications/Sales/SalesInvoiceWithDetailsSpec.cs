using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفات استعلامات فواتير المبيعات مع تضمين بيانات الفروع والمستودعات والعملاء والأصناف والباركودات.
/// </summary>
internal sealed class SalesInvoiceWithDetailsSpec : BaseSpecification<SalesInvoice>
{
    /// <summary>
    /// جلب كافة فواتير المبيعات مرتبة تنازلياً بتاريخ الفاتورة مع التفاصيل الكاملة.
    /// </summary>
    public SalesInvoiceWithDetailsSpec()
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع أو صالة العرض
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات المنتجات التابعة لبنود الفاتورة
        AddInclude("Items.Product");
        // تضمين بيانات الباركودات للبنود
        AddInclude("Items.ProductBarCode");
        // ترتيب النتائج تنازلياً بتاريخ الفاتورة
        ApplyOrderByDescending(s => s.InvoiceDate);
    }

    /// <summary>
    /// جلب فاتورة مبيعات محددة بالمعرف مع تفاصيلها.
    /// </summary>
    /// <param name = "id">معرف الفاتورة</param>
    public SalesInvoiceWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات المنتجات
        AddInclude("Items.Product");
        // تضمين بيانات الباركودات
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// جلب فاتورة مبيعات بواسطة رقم الفاتورة.
    /// </summary>
    /// <param name = "invoiceNumber">رقم الفاتورة</param>
    public SalesInvoiceWithDetailsSpec(string invoiceNumber) : base(s => s.InvoiceNumber == invoiceNumber)
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات المنتجات
        AddInclude("Items.Product");
        // تضمين بيانات الباركودات
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// فلترة فواتير المبيعات بالفرع والمستودع والعميل والحالة وطريقة الدفع والتاريخ والبحث.
    /// </summary>
    /// <param name = "branchId">معرف الفرع (اختياري)</param>
    /// <param name = "warehouseId">معرف المستودع (اختياري)</param>
    /// <param name = "customerId">معرف العميل (اختياري)</param>
    /// <param name = "status">حالة الفاتورة (اختياري)</param>
    /// <param name = "paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name = "fromDate">تاريخ البداية (اختياري)</param>
    /// <param name = "toDate">تاريخ النهاية (اختياري)</param>
    /// <param name = "search">نص البحث (اختياري)</param>
    public SalesInvoiceWithDetailsSpec(Guid? branchId, Guid? warehouseId, Guid? customerId, InvoiceStatus? status, PaymentMethod? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null) : base(s => (!branchId.HasValue || s.BranchId == branchId.Value) && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value) && (!customerId.HasValue || s.CustomerId == customerId.Value) && (!status.HasValue || s.Status == status.Value) && (!paymentMethod.HasValue || s.PaymentMethod == paymentMethod.Value) && (!fromDate.HasValue || s.InvoiceDate >= fromDate.Value) && (!toDate.HasValue || s.InvoiceDate <= toDate.Value) && (string.IsNullOrWhiteSpace(search) || s.InvoiceNumber.Contains(search) || (s.Customer != null && s.Customer.Name.Contains(search))))
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات المنتجات
        AddInclude("Items.Product");
        // تضمين بيانات الباركودات
        AddInclude("Items.ProductBarCode");
        // الترتيب تنازلياً بتاريخ الفاتورة
        ApplyOrderByDescending(s => s.InvoiceDate);
    }
}
