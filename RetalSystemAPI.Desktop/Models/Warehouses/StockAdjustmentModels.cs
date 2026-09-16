using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Warehouses;

public enum StockAdjustmentReason
{
    InventoryCount = 1,
    Damaged = 2,
    Expired = 3,
    InitialSetup = 4,
    Other = 5
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
        StockAdjustmentReason.Damaged => "بضاعة تالفة",
        StockAdjustmentReason.Expired => "بضاعة منتهية الصلاحية",
        StockAdjustmentReason.InitialSetup => "إعداد رصيد افتتاحي",
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
        StockAdjustmentReason.Damaged => "بضاعة تالفة",
        StockAdjustmentReason.Expired => "منتهي الصلاحية",
        StockAdjustmentReason.InitialSetup => "رصيد افتتاحي",
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
    public Guid? ProductBarCodeId { get; set; }
    public string? BarcodeTitle { get; set; }
    public string? BarcodeValue { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int DifferenceQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public StockAdjustmentReason Reason { get; set; } = StockAdjustmentReason.InventoryCount;
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
    public string? BarcodeTitle { get; set; }
    public string? BarcodeValue { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; } = 1;
    public int DifferenceQuantity => ActualQuantity - SystemQuantity;
    public decimal UnitCost { get; set; }
    public StockAdjustmentReason Reason { get; set; } = StockAdjustmentReason.InventoryCount;
    public string ReasonDisplay => Reason switch
    {
        StockAdjustmentReason.InventoryCount => "جرد دوري",
        StockAdjustmentReason.Damaged => "بضاعة تالفة",
        StockAdjustmentReason.Expired => "منتهي الصلاحية",
        StockAdjustmentReason.InitialSetup => "رصيد افتتاحي",
        _ => "أخرى"
    };
}
