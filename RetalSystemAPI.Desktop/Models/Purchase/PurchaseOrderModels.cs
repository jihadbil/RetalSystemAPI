using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Purchase;

public enum PurchaseOrderStatus
{
    Draft = 1,
    Submitted = 2,
    Received = 3,
    Canceled = 4
}

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class PurchaseOrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public class PurchaseOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductBarCodeId { get; set; }
    public string BarcodeTitle { get; set; } = null!;
    public string BarcodeValue { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public string OrderNumber { get; set; } = null!;
    public Guid BranchId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemRequest
{
    public Guid ProductBarCodeId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdatePurchaseOrderRequest
{
    public Guid? WarehouseId { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public List<CreatePurchaseOrderItemRequest>? Items { get; set; }
}
