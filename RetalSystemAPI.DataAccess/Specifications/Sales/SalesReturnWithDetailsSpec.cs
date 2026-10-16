using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفات استعلامات مرتجعات المبيعات مع تضمين الفاتورة الأصلية والفرع والمستودع والعميل وبنود المرتجع.
/// </summary>
internal sealed class SalesReturnWithDetailsSpec : BaseSpecification<SalesReturn>
{
    /// <summary>
    /// جلب كافة مرتجعات المبيعات مرتبة تنازلياً بتاريخ الإرجاع مع كامل التفاصيل والبنود.
    /// </summary>
    public SalesReturnWithDetailsSpec()
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع أو الصالة
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات الفاتورة الأصلية
        AddInclude(s => s.OriginalInvoice!);
        // تضمين منتجات البنود
        AddInclude("Items.Product");
        // تضمين باركودات البنود
        AddInclude("Items.ProductBarCode");
        // الترتيب تنازلياً بتاريخ الإرجاع
        ApplyOrderByDescending(s => s.ReturnDate);
    }

    /// <summary>
    /// جلب سجل مرتجع مبيعات محدد بالمعرف مع تفاصيله الكاملة.
    /// </summary>
    /// <param name = "id">معرف المرتجع</param>
    public SalesReturnWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات الفاتورة الأصلية
        AddInclude(s => s.OriginalInvoice!);
        // تضمين منتجات البنود
        AddInclude("Items.Product");
        // تضمين باركودات البنود
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// جلب سجل مرتجع مبيعات بواسطة رقم المرتجع.
    /// </summary>
    /// <param name = "returnNumber">رقم المرتجع</param>
    public SalesReturnWithDetailsSpec(string returnNumber) : base(s => s.ReturnNumber == returnNumber)
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات الفاتورة الأصلية
        AddInclude(s => s.OriginalInvoice!);
        // تضمين منتجات البنود
        AddInclude("Items.Product");
        // تضمين باركودات البنود
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// فلترة مرتجعات المبيعات حسب الفرع والمستودع والعميل وسبب الإرجاع والتاريخ والبحث.
    /// </summary>
    /// <param name = "branchId">معرف الفرع (اختياري)</param>
    /// <param name = "warehouseId">معرف المستودع (اختياري)</param>
    /// <param name = "customerId">معرف العميل (اختياري)</param>
    /// <param name = "reason">سبب الإرجاع (اختياري)</param>
    /// <param name = "fromDate">تاريخ البداية (اختياري)</param>
    /// <param name = "toDate">تاريخ النهاية (اختياري)</param>
    /// <param name = "search">نص البحث (اختياري)</param>
    public SalesReturnWithDetailsSpec(Guid? branchId, Guid? warehouseId, Guid? customerId, SalesReturnReason? reason = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null) : base(s => (!branchId.HasValue || s.BranchId == branchId.Value) && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value) && (!customerId.HasValue || s.CustomerId == customerId.Value) && (!reason.HasValue || s.Reason == reason.Value) && (!fromDate.HasValue || s.ReturnDate >= fromDate.Value) && (!toDate.HasValue || s.ReturnDate <= toDate.Value) && (string.IsNullOrWhiteSpace(search) || s.ReturnNumber.Contains(search) || (s.Customer != null && s.Customer.Name.Contains(search))))
    {
        // تضمين بيانات الفرع
        AddInclude(s => s.Branch);
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);
        // تضمين بيانات العميل
        AddInclude(s => s.Customer!);
        // تضمين بيانات الفاتورة الأصلية
        AddInclude(s => s.OriginalInvoice!);
        // تضمين منتجات البنود
        AddInclude("Items.Product");
        // تضمين باركودات البنود
        AddInclude("Items.ProductBarCode");
        // الترتيب تنازلياً بتاريخ الإرجاع
        ApplyOrderByDescending(s => s.ReturnDate);
    }
}
