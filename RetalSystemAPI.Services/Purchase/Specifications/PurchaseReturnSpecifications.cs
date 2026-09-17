using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Services.Purchase.Specifications;

/// <summary>
/// مواصفات استعلامات مرتجعات المشتريات مع تضمين المورد والفرع والمستودع والفاتورة وبنود المرتجع.
/// </summary>
public class PurchaseReturnWithDetailsSpec : BaseSpecification<PurchaseReturn>
{
    /// <summary>
    /// جلب كافة مرتجعات المشتريات مرتبة تنازلياً بتاريخ الإرجاع مع تضمين العلاقات والتفاصيل.
    /// </summary>
    public PurchaseReturnWithDetailsSpec()
    {
        // تضمين بيانات الفرع التابع له المرتجع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع المعني بالإرجاع
        AddInclude(p => p.Warehouse);
        // تضمين بيانات المورد المرتجع إليه
        AddInclude(p => p.Supplier);
        // تضمين بيانات فاتورة الشراء الأصلية المرتبطة إن وجدت
        AddInclude(p => p.PurchaseInvoice!);
        // تضمين بيانات الصنف لكل بند مرتجع
        AddInclude("Items.Product");
        // تضمين بيانات الباركود والنكهة لكل بند مرتجع
        AddInclude("Items.ProductBarCode");
        // ترتيب المرتجعات تنازلياً حسب تاريخ المرتجع
        ApplyOrderByDescending(p => p.ReturnDate);
    }

    /// <summary>
    /// جلب سجل مرتجع مشتريات محدد بالمعرف مع تفاصيله الكاملة.
    /// </summary>
    /// <param name="id">معرف سجل المرتجع</param>
    public PurchaseReturnWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier);
        // تضمين بيانات فاتورة الشراء الأصلية
        AddInclude(p => p.PurchaseInvoice!);
        // تضمين الصنف لكل بند مرتجع
        AddInclude("Items.Product");
        // تضمين الباركود لكل بند مرتجع
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// جلب سجل مرتجع مشتريات بواسطة رقم المرتجع مع التفاصيل الكاملة.
    /// </summary>
    /// <param name="returnNumber">رقم إشعار المرتجع</param>
    public PurchaseReturnWithDetailsSpec(string returnNumber) : base(p => p.ReturnNumber == returnNumber)
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier);
        // تضمين فاتورة الشراء
        AddInclude(p => p.PurchaseInvoice!);
        // تضمين الصنف
        AddInclude("Items.Product");
        // تضمين الباركود
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// فلترة مرتجعات المشتريات حسب المورد والفرع والمستودع وسبب الإرجاع وطريقة الدفع والتواريخ والبحث.
    /// </summary>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع (اختياري)</param>
    /// <param name="supplierId">معرف المورد (اختياري)</param>
    /// <param name="reason">سبب الإرجاع (اختياري)</param>
    /// <param name="paymentMethod">طريقة الدفع/الاسترداد (اختياري)</param>
    /// <param name="fromDate">تاريخ بداية الفترة (اختياري)</param>
    /// <param name="toDate">تاريخ نهاية الفترة (اختياري)</param>
    /// <param name="search">نص البحث برقم المرتجع أو اسم المورد أو الملاحظات (اختياري)</param>
    public PurchaseReturnWithDetailsSpec(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null)
        : base(p => (!branchId.HasValue || p.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) &&
                    (!supplierId.HasValue || p.SupplierId == supplierId.Value) &&
                    (!reason.HasValue || p.Reason == reason.Value) &&
                    (!paymentMethod.HasValue || p.PaymentMethod == paymentMethod.Value) &&
                    (!fromDate.HasValue || p.ReturnDate >= fromDate.Value) &&
                    (!toDate.HasValue || p.ReturnDate <= toDate.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     p.ReturnNumber.Contains(search) ||
                     (p.Supplier != null && p.Supplier.Name.Contains(search)) ||
                     (p.Notes != null && p.Notes.Contains(search))))
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier);
        // تضمين فاتورة المشتريات
        AddInclude(p => p.PurchaseInvoice!);
        // تضمين الأصناف
        AddInclude("Items.Product");
        // تضمين الباركودات
        AddInclude("Items.ProductBarCode");
        // ترتيب النتائج تنازلياً حسب تاريخ الإرجاع
        ApplyOrderByDescending(p => p.ReturnDate);
    }
}

/// <summary>
/// مواصفة القوائم والترقيم لمرتجعات المشتريات — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمينات العميقة Items.Product / Items.ProductBarCode التي تحتاجها استعلامات التفاصيل فقط.
/// </summary>
public class PurchaseReturnListSpec : BaseSpecification<PurchaseReturn>
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
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع (اختياري)</param>
    /// <param name="supplierId">معرف المورد (اختياري)</param>
    /// <param name="reason">سبب الإرجاع (اختياري)</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    public PurchaseReturnListSpec(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null)
        : base(p => (!branchId.HasValue || p.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) &&
                    (!supplierId.HasValue || p.SupplierId == supplierId.Value) &&
                    (!reason.HasValue || p.Reason == reason.Value) &&
                    (!paymentMethod.HasValue || p.PaymentMethod == paymentMethod.Value) &&
                    (!fromDate.HasValue || p.ReturnDate >= fromDate.Value) &&
                    (!toDate.HasValue || p.ReturnDate <= toDate.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     p.ReturnNumber.Contains(search) ||
                     (p.Supplier != null && p.Supplier.Name.Contains(search)) ||
                     (p.Notes != null && p.Notes.Contains(search))))
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
