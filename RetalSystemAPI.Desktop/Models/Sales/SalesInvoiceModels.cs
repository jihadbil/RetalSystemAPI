using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Sales;

public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Cancelled = 4,
    Voided = 5
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    BankTransfer = 2,
    Credit = 3,
    Multiple = 4
}

public class SalesInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusName => Status switch
    {
        InvoiceStatus.Draft => "مسودة",
        InvoiceStatus.Issued => "مصدرة",
        InvoiceStatus.Paid => "مدفوعة بالكامل",
        InvoiceStatus.PartiallyPaid => "مدفوعة جزئياً",
        InvoiceStatus.Cancelled => "ملغية",
        InvoiceStatus.Voided => "باطلة / مسترجعة",
        _ => "غير محدد"
    };
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod switch
    {
        PaymentMethod.Cash => "نقداً",
        PaymentMethod.Card => "بطاقة مصرفية",
        PaymentMethod.BankTransfer => "تحويل مصرفي",
        PaymentMethod.Credit => "آجل / على الحساب",
        PaymentMethod.Multiple => "دفعات متعددة",
        _ => "نقداً"
    };
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string? Notes { get; set; }
    public List<SalesInvoiceItemDto> Items { get; set; } = new();
}

public class SalesInvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusName => Status switch
    {
        InvoiceStatus.Draft => "مسودة",
        InvoiceStatus.Issued => "مصدرة",
        InvoiceStatus.Paid => "مدفوعة",
        InvoiceStatus.PartiallyPaid => "مدفوعة جزئياً",
        InvoiceStatus.Cancelled => "ملغية",
        InvoiceStatus.Voided => "باطلة",
        _ => "غير محدد"
    };
    public PaymentMethod PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int ItemCount { get; set; }
}

public class SalesInvoiceItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public string? BarCode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreateSalesInvoiceRequest
{
    public string InvoiceNumber { get; set; } = null!;
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Issued;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public List<CreateSalesInvoiceItemRequest> Items { get; set; } = new();
}

public class CreateSalesInvoiceItemRequest
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal => (Quantity * UnitPrice) - DiscountAmount;
}

public class UpdateSalesInvoiceRequest
{
    public InvoiceStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
}
