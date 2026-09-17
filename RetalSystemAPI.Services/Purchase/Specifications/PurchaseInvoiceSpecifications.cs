using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Services.Purchase.Specifications;

/// <summary>
/// مواصفة جلب تفاصيل فاتورة مشتريات محددة مع المورد، الفرع، المستودع، البنود المجمعة، وتفصيلات النكهات (Breakdowns).
/// </summary>
public class PurchaseInvoiceWithDetailsSpec : BaseSpecification<PurchaseInvoice>
{
    /// <summary>
    /// تهيئة مواصفة فاتورة المشتريات بالمعرف وتضمين كافة العلاقات التفصيلية وتفاصيل النكهات.
    /// </summary>
    /// <param name="id">معرف فاتورة المشتريات</param>
    public PurchaseInvoiceWithDetailsSpec(Guid id)
        : base(p => p.Id == id)
    {
        // تضمين بيانات المورد المصدر للفاتورة
        AddInclude(p => p.Supplier);
        // تضمين بيانات الفرع التابع له الفاتورة
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع المستلم للبضاعة
        AddInclude(p => p.Warehouse);
        // تضمين أمر الشراء المرتبط في حال وجوده
        AddInclude(p => p.PurchaseOrder!);
        // تضمين بنود الفاتورة
        AddInclude(p => p.Items);
        // تضمين بيانات الصنف لكل بند
        AddInclude("Items.Product");
        // تضمين بيانات الباركود الرئيسي لكل بند
        AddInclude("Items.ProductBarCode");
        // تضمين تفريعات ونكهات البنود
        AddInclude("Items.Breakdowns");
        // تضمين باركود كل نكهة أو تفريعة للبند
        AddInclude("Items.Breakdowns.ProductBarCode");
    }
}

/// <summary>
/// مواصفة فلترة فواتير المشتريات وتصفحها بالترقيم مع فلاتر التاريخ والمورد والفرع والمستودع والحالة وطريقة الدفع.
/// </summary>
public class PurchaseInvoiceFilterSpec : BaseSpecification<PurchaseInvoice>
{
    /// <summary>
    /// تهيئة مواصفة فلترة فواتير المشتريات مع دعم مختلف محددات التصفية والبحث بالنص.
    /// </summary>
    /// <param name="supplierId">معرف المورد للفلترة (اختياري)</param>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع للفلترة (اختياري)</param>
    /// <param name="status">حالة الفاتورة (اختياري)</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="searchTerm">مصطلح البحث برقم الفاتورة أو اسم المورد (اختياري)</param>
    public PurchaseInvoiceFilterSpec(
        Guid? supplierId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
        : base(p =>
            (!supplierId.HasValue || p.SupplierId == supplierId.Value) &&
            (!branchId.HasValue || p.BranchId == branchId.Value) &&
            (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) &&
            (!status.HasValue || p.Status == status.Value) &&
            (!paymentMethod.HasValue || p.PaymentMethod == paymentMethod.Value) &&
            (!fromDate.HasValue || p.InvoiceDate >= fromDate.Value) &&
            (!toDate.HasValue || p.InvoiceDate <= toDate.Value) &&
            (string.IsNullOrWhiteSpace(searchTerm) ||
             p.InvoiceNumber.Contains(searchTerm) ||
             (p.Supplier != null && p.Supplier.Name.Contains(searchTerm))))
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
