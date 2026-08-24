using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// تخصيصات استعلامات التسويات الجردية مع تفاصيل المستودع والبنود.
/// </summary>
public class StockAdjustmentWithDetailsSpec : BaseSpecification<StockAdjustment>
{
    public StockAdjustmentWithDetailsSpec()
    {
        AddInclude(s => s.Warehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.AdjustmentDate);
    }

    public StockAdjustmentWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.Warehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    public StockAdjustmentWithDetailsSpec(string adjustmentNumber) : base(s => s.AdjustmentNumber == adjustmentNumber)
    {
        AddInclude(s => s.Warehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    public StockAdjustmentWithDetailsSpec(
        Guid? warehouseId,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null)
        : base(s => (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value) &&
                    (!reason.HasValue || s.Reason == reason.Value) &&
                    (!fromDate.HasValue || s.AdjustmentDate >= fromDate.Value) &&
                    (!toDate.HasValue || s.AdjustmentDate <= toDate.Value) &&
                    (string.IsNullOrWhiteSpace(search) || s.AdjustmentNumber.Contains(search)))
    {
        AddInclude(s => s.Warehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.AdjustmentDate);
    }
}
