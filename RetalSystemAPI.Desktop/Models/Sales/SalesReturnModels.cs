using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Sales;

public enum SalesReturnReason
{
    Damaged = 0,
    Defective = 1,
    Expired = 2,
    WrongItem = 3,
    CustomerChangedMind = 4,
    Other = 5
}

public class SalesReturnDto
{
    public Guid Id { get; set; }
    public string ReturnNumber { get; set; } = null!;
    public Guid? OriginalInvoiceId { get; set; }
    public string? OriginalInvoiceNumber { get; set; }
    public Guid BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime ReturnDate { get; set; }
    public SalesReturnReason Reason { get; set; }
    public string ReasonName => Reason switch
    {
        SalesReturnReason.Damaged => "تالف / مكسور",
        SalesReturnReason.Defective => "معيب / غير صالح",
        SalesReturnReason.Expired => "منتهي الصلاحية",
        SalesReturnReason.WrongItem => "صنف خاطئ",
        SalesReturnReason.CustomerChangedMind => "تراجع العميل",
        SalesReturnReason.Other => "أسباب أخرى",
        _ => "أخرى"
    };
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<SalesReturnItemDto> Items { get; set; } = new();
}

public class SalesReturnSummaryDto
{
    public Guid Id { get; set; }
    public string ReturnNumber { get; set; } = null!;
    public string? OriginalInvoiceNumber { get; set; }
    public string? BranchName { get; set; }
    public string? WarehouseName { get; set; }
    public string? CustomerName { get; set; }
    public DateTime ReturnDate { get; set; }
    public SalesReturnReason Reason { get; set; }
    public string ReasonName => Reason switch
    {
        SalesReturnReason.Damaged => "تالف",
        SalesReturnReason.Defective => "معيب",
        SalesReturnReason.Expired => "منتهي الصلاحية",
        SalesReturnReason.WrongItem => "صنف خاطئ",
        SalesReturnReason.CustomerChangedMind => "تراجع العميل",
        SalesReturnReason.Other => "أخرى",
        _ => "أخرى"
    };
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public class SalesReturnItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

public class CreateSalesReturnRequest
{
    public string ReturnNumber { get; set; } = null!;
    public Guid? OriginalInvoiceId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public SalesReturnReason Reason { get; set; } = SalesReturnReason.CustomerChangedMind;
    public string? Notes { get; set; }
    public List<CreateSalesReturnItemRequest> Items { get; set; } = new();
}

public class CreateSalesReturnItemRequest
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public string? Notes { get; set; }
}
