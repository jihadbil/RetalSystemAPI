using System;
using System.Collections.Generic;
using RetalSystemAPI.Desktop.Models.Sales;

namespace RetalSystemAPI.Desktop.Models.Purchase;

public enum PurchaseReturnReason
{
    Defective = 1,
    WrongSpecification = 2,
    NearExpiryOrExpired = 3,
    ExcessStock = 4,
    Other = 5
}

public class PurchaseReturnSummaryDto
{
    public Guid Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public Guid? PurchaseInvoiceId { get; set; }
    public string? PurchaseInvoiceNumber { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public PurchaseReturnReason Reason { get; set; }
    public string ReasonName => Reason switch
    {
        PurchaseReturnReason.Defective => "بضاعة معيبة أو تالفة",
        PurchaseReturnReason.WrongSpecification => "غير مطابق للمواصفات",
        PurchaseReturnReason.NearExpiryOrExpired => "منتهي أو قريب الصلاحية",
        PurchaseReturnReason.ExcessStock => "فائض مخزون / مرتجع",
        _ => "أخرى"
    };
    public int ItemCount { get; set; }
    public string? Notes { get; set; }
}

public class PurchaseReturnDto
{
    public Guid Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public Guid? PurchaseInvoiceId { get; set; }
    public string? PurchaseInvoiceNumber { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public PurchaseReturnReason Reason { get; set; }
    public string ReasonName => Reason switch
    {
        PurchaseReturnReason.Defective => "بضاعة معيبة أو تالفة",
        PurchaseReturnReason.WrongSpecification => "غير مطابق للمواصفات",
        PurchaseReturnReason.NearExpiryOrExpired => "منتهي أو قريب الصلاحية",
        PurchaseReturnReason.ExcessStock => "فائض مخزون / مرتجع",
        _ => "أخرى"
    };
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PurchaseReturnItemDto> Items { get; set; } = new();
}

public class PurchaseReturnItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid? ProductBarCodeId { get; set; }
    public string? BarcodeTitle { get; set; }
    public string? BarcodeValue { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

public class CreatePurchaseReturnRequest
{
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public Guid? PurchaseInvoiceId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public PurchaseReturnReason Reason { get; set; } = PurchaseReturnReason.Defective;
    public string? Notes { get; set; }
    public List<CreatePurchaseReturnItemRequest> Items { get; set; } = new();
}

public class CreatePurchaseReturnItemRequest
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public string? BarcodeTitle { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public string? Notes { get; set; }
}
