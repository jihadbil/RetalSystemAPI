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
    /// <summary>جلب كافة مرتجعات المشتريات مرتبة تنازلياً بتاريخ الإرجاع</summary>
    public PurchaseReturnWithDetailsSpec()
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse);
        AddInclude(p => p.Supplier);
        AddInclude(p => p.PurchaseInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(p => p.ReturnDate);
    }

    /// <summary>جلب سجل مرتجع مشتريات محدد بالمعرف مع تفاصيله الكاملة</summary>
    public PurchaseReturnWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse);
        AddInclude(p => p.Supplier);
        AddInclude(p => p.PurchaseInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>جلب سجل مرتجع مشتريات بواسطة رقم المرتجع</summary>
    public PurchaseReturnWithDetailsSpec(string returnNumber) : base(p => p.ReturnNumber == returnNumber)
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse);
        AddInclude(p => p.Supplier);
        AddInclude(p => p.PurchaseInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>فلترة مرتجعات المشتريات حسب المورد والفرع والمستودع وسبب الإرجاع وطريقة الدفع والتواريخ والبحث</summary>
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
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse);
        AddInclude(p => p.Supplier);
        AddInclude(p => p.PurchaseInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(p => p.ReturnDate);
    }
}

/// <summary>
/// مواصفة القوائم والترقيم لمرتجعات المشتريات — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمينات العميقة Items.Product / Items.ProductBarCode التي تحتاجها استعلامات التفاصيل فقط.
/// </summary>
public class PurchaseReturnListSpec : BaseSpecification<PurchaseReturn>
{
    /// <summary>جلب كافة مرتجعات المشتريات للقوائم مرتبة تنازلياً بتاريخ الإرجاع</summary>
    public PurchaseReturnListSpec()
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse);
        AddInclude(p => p.Supplier);
        AddInclude(p => p.PurchaseInvoice!);
        AddInclude(p => p.Items);
        ApplyOrderByDescending(p => p.ReturnDate);
    }

    /// <summary>فلترة قوائم مرتجعات المشتريات حسب المورد والفرع والمستودع وسبب الإرجاع وطريقة الدفع والتواريخ والبحث</summary>
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
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse);
        AddInclude(p => p.Supplier);
        AddInclude(p => p.PurchaseInvoice!);
        AddInclude(p => p.Items);
        ApplyOrderByDescending(p => p.ReturnDate);
    }
}
