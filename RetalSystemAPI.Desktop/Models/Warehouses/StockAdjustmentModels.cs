using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Warehouses;

public enum StockAdjustmentReason
{
    InventoryCount = 0,
    DamagedGoods = 1,
    ExpiredGoods = 2,
    TheftOrLoss = 3,
    FoundStock = 4,
    DataEntryCorrection = 5,
    Other = 6
}

public class StockAdjustmentDto
{
    public Guid Id { get; set; }
    public string AdjustmentNumber { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public StockAdjustmentReason Reason { get; set; }
    public string ReasonName => Reason switch
    {
        StockAdjustmentReason.InventoryCount => "جرد دوري / سنوي",
        StockAdjustmentReason.DamagedGoods => "بضاعة تالفة",
        StockAdjustmentReason.ExpiredGoods => "بضاعة منتهية الصلاحية",
        StockAdjustmentReason.TheftOrLoss => "فقدان أو سرقة",
        StockAdjustmentReason.FoundStock => "بضاعة معثور عليها زائفة",
        StockAdjustmentReason.DataEntryCorrection => "تصحيح خطأ إدخال",
        StockAdjustmentReason.Other => "أسباب أخرى",
        _ => "أخرى"
    };
    public string? Notes { get; set; }
    public List<StockAdjustmentItemDto> Items { get; set; } = new();
}

public class StockAdjustmentSummaryDto
{
    public Guid Id { get; set; }
    public string AdjustmentNumber { get; set; } = null!;
    public string? WarehouseName { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public StockAdjustmentReason Reason { get; set; }
    public string ReasonName => Reason switch
    {
        StockAdjustmentReason.InventoryCount => "جرد دوري",
        StockAdjustmentReason.DamagedGoods => "بضاعة تالفة",
        StockAdjustmentReason.ExpiredGoods => "منتهي الصلاحية",
        StockAdjustmentReason.TheftOrLoss => "عجز / فقدان",
        StockAdjustmentReason.FoundStock => "فائض معثور عليه",
        StockAdjustmentReason.DataEntryCorrection => "تصحيح خطأ إدخال",
        StockAdjustmentReason.Other => "أخرى",
        _ => "أخرى"
    };
    public int ItemCount { get; set; }
}

public class StockAdjustmentItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int DifferenceQuantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class CreateStockAdjustmentRequest
{
    public string AdjustmentNumber { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;
    public StockAdjustmentReason Reason { get; set; } = StockAdjustmentReason.InventoryCount;
    public string? Notes { get; set; }
    public List<CreateStockAdjustmentItemRequest> Items { get; set; } = new();
}

public class CreateStockAdjustmentItemRequest
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; } = 1;
    public int DifferenceQuantity => ActualQuantity - SystemQuantity;
    public decimal UnitCost { get; set; }
}
