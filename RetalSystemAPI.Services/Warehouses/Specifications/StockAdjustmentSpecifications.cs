using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// مواصفات استعلامات التسويات الجردية مع تضمين المستودع وبنود التسوية والأصناف والباركودات.
/// </summary>
public class StockAdjustmentWithDetailsSpec : BaseSpecification<StockAdjustment>
{
    /// <summary>جلب كافة التسويات الجردية مرتبة تنازلياً بتاريخ التسوية</summary>
    public StockAdjustmentWithDetailsSpec()
    {
        AddInclude(s => s.Warehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.AdjustmentDate);
    }

    /// <summary>جلب تسوية جردية محددة بالمعرف مع تفاصيلها</summary>
    /// <param name="id">معرف التسوية الجردية</param>
    public StockAdjustmentWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.Warehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>جلب تسوية جردية بواسطة رقم التسوية</summary>
    /// <param name="adjustmentNumber">رقم التسوية الجردية</param>
    public StockAdjustmentWithDetailsSpec(string adjustmentNumber) : base(s => s.AdjustmentNumber == adjustmentNumber)
    {
        AddInclude(s => s.Warehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>فلترة التسويات الجردية حسب المستودع وسبب التسوية والنطاق الزمني والبحث برقم التسوية</summary>
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
