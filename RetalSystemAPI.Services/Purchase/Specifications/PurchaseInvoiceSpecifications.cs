using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Services.Purchase.Specifications;

public class PurchaseInvoiceWithDetailsSpec : BaseSpecification<PurchaseInvoice>
{
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
    }
}

public class PurchaseInvoiceFilterSpec : BaseSpecification<PurchaseInvoice>
{
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
