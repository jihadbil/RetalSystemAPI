using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Warehouses;

public enum StockTransferStatus
{
    Draft = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4
}

public class StockTransferDto
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = null!;
    public Guid FromWarehouseId { get; set; }
    public string? FromWarehouseName { get; set; }
    public Guid ToWarehouseId { get; set; }
    public string? ToWarehouseName { get; set; }
    public DateTime TransferDate { get; set; }
    public StockTransferStatus Status { get; set; }
    public string StatusName => Status switch
    {
        StockTransferStatus.Draft => "مسودة",
        StockTransferStatus.Confirmed => "مؤكد",
        StockTransferStatus.Completed => "مرحل ومكتمل",
        StockTransferStatus.Cancelled => "ملغي",
        _ => "غير محدد"
    };
    public string? Notes { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();
}

public class StockTransferSummaryDto
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = null!;
    public string? FromWarehouseName { get; set; }
    public string? ToWarehouseName { get; set; }
    public DateTime TransferDate { get; set; }
    public StockTransferStatus Status { get; set; }
    public string StatusName => Status switch
    {
        StockTransferStatus.Draft => "مسودة",
        StockTransferStatus.Confirmed => "مؤكد",
        StockTransferStatus.Completed => "مرحل ومكتمل",
        StockTransferStatus.Cancelled => "ملغي",
        _ => "غير محدد"
    };
    public int ItemCount { get; set; }
}

public class StockTransferItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public string? BarCode { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateStockTransferRequest
{
    public string TransferNumber { get; set; } = null!;
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public List<CreateStockTransferItemRequest> Items { get; set; } = new();
}

public class CreateStockTransferItemRequest
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public string? BarcodeTitle { get; set; }
    public string? BarcodeValue { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}

public class UpdateStockTransferRequest
{
    public StockTransferStatus Status { get; set; }
    public string? Notes { get; set; }
}
