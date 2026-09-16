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
    /// <summary>تهيئة مواصفة فاتورة المشتريات بالمعرف وتضمين تفاصيل النكهات</summary>
    /// <param name="id">معرف فاتورة المشتريات</param>
    public PurchaseInvoiceWithDetailsSpec(Guid id)
        : base(p => p.Id == id)
    {
        AddInclude(p => p.Supplier);
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse);
        AddInclude(p => p.PurchaseOrder!);
        AddInclude(p => p.Items);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        AddInclude("Items.Breakdowns");
        AddInclude("Items.Breakdowns.ProductBarCode");
    }
}

/// <summary>
/// مواصفة فلترة فواتير المشتريات وتصفحها بالترقيم مع فلاتر التاريخ والمورد والفرع والمستودع والحالة وطريقة الدفع.
/// </summary>
public class PurchaseInvoiceFilterSpec : BaseSpecification<PurchaseInvoice>
{
    /// <summary>تهيئة مواصفة فلترة فواتير المشتريات</summary>
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
        AddInclude(p => p.Supplier);
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse);
        AddInclude(p => p.Items);
        ApplyOrderByDescending(p => p.InvoiceDate);
    }
}
