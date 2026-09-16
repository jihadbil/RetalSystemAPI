using System;
using System.Collections.Generic;
using RetalSystemAPI.Desktop.Models.Sales;

namespace RetalSystemAPI.Desktop.Models.Purchase;

public class PurchaseInvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int ItemsCount { get; set; }
}

public class PurchaseInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseInvoiceItemDto> Items { get; set; } = new();
}

public class PurchaseInvoiceItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public string? BarCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreatePurchaseInvoiceRequest
{
    public string InvoiceNumber { get; set; } = string.Empty;
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
    public List<CreatePurchaseInvoiceItemRequest> Items { get; set; } = new();
}

public class PackagingUnitOption
{
    public string Name { get; set; } = string.Empty;
    public int ConversionFactor { get; set; } = 1;
    public string DisplayText => $"{Name} ({ConversionFactor} قطعة)";
}

public class PurchaseInvoiceItemBreakdownRequest
{
    public Guid ProductBarCodeId { get; set; }
    public string BarCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal PackageQuantity { get; set; } = 0;
    public int UnitsPerPackage { get; set; } = 1;
    public decimal Quantity => PackageQuantity * (UnitsPerPackage > 0 ? UnitsPerPackage : 1);
    public decimal UnitPrice { get; set; }
}

public class CreatePurchaseInvoiceItemRequest
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid? ProductBarCodeId { get; set; }
    public string? BarCode { get; set; }
    public string PackageUnitName { get; set; } = "قطعة";
    public decimal PackageQuantity { get; set; } = 1;
    public int UnitsPerPackage { get; set; } = 1;
    public decimal InvoicePackagePrice { get; set; } = 0;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; } = 0;
    public decimal SalePrice { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal LineTotal => Math.Max(0, (Quantity * UnitPrice) - DiscountAmount);
    public decimal ProfitPerPiece => SalePrice > 0 ? (SalePrice - UnitPrice) : 0;
    public decimal ProfitMarginPercentage => UnitPrice > 0 && SalePrice > 0 ? Math.Round(((SalePrice - UnitPrice) / UnitPrice) * 100, 2) : 0;
    public string PackagingSummary => UnitsPerPackage > 1 ? $"{PackageQuantity:N0} {PackageUnitName} × {UnitsPerPackage} حبة" : $"{Quantity:N0} قطعة";
    public List<PurchaseInvoiceItemBreakdownRequest> Breakdowns { get; set; } = new();
    public string FlavorsSummaryText => Breakdowns != null && Breakdowns.Any(b => b.PackageQuantity > 0)
        ? string.Join(" | ", Breakdowns.Where(b => b.PackageQuantity > 0).Select(b => $"{b.Title} ({b.PackageQuantity:N0})"))
        : (string.IsNullOrWhiteSpace(BarCode) ? "افتراضي" : BarCode);
}

public class UpdatePurchaseInvoiceRequest
{
    public string InvoiceNumber { get; set; } = string.Empty;
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
    public List<CreatePurchaseInvoiceItemRequest>? Items { get; set; }
}
