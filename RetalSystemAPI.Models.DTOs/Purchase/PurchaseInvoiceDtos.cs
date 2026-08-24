using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Purchase;

public class PurchaseInvoiceSummaryDto : BaseDto
{
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public InvoiceStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int ItemsCount { get; set; }
}

public class PurchaseInvoiceResponseDto : BaseDto
{
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public InvoiceStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseInvoiceItemResponseDto> Items { get; set; } = new();
}

public class PurchaseInvoiceItemResponseDto : BaseDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductCode { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public string? BarCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreatePurchaseInvoiceDto
{
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public Guid SupplierId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseInvoiceItemDto> Items { get; set; } = new();
}

public class CreatePurchaseInvoiceItemDto
{
    public Guid ProductId { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
}

public class UpdatePurchaseInvoiceDto
{
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public Guid SupplierId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public InvoiceStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseInvoiceItemDto>? Items { get; set; }
}
