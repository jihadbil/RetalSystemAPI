using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Services.Purchase.Specifications;

/// <summary>
/// تخصيصات استعلامات الطلبيات والمشتريات.
/// </summary>
public class PurchaseOrderWithDetailsSpec : BaseSpecification<PurchaseOrder>
{
    public PurchaseOrderWithDetailsSpec()
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse!);
        AddInclude("Items.ProductBarCode.Product");
        ApplyOrderByDescending(p => p.OrderDate);
    }

    public PurchaseOrderWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse!);
        AddInclude("Items.ProductBarCode.Product");
    }

    public PurchaseOrderWithDetailsSpec(Guid? branchId, Guid? warehouseId, PurchaseOrderStatus? status, string? search = null)
        : base(p => (!branchId.HasValue || p.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) &&
                    (!status.HasValue || p.Status == status.Value) &&
                    (string.IsNullOrWhiteSpace(search) || p.OrderNumber.Contains(search)))
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse!);
        AddInclude("Items.ProductBarCode.Product");
        ApplyOrderByDescending(p => p.OrderDate);
    }
}
