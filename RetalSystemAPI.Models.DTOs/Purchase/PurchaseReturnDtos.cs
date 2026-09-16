using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Purchase;

/// <summary>
/// بيانات إنشاء مرتجع مشتريات جديد.
/// </summary>
public class CreatePurchaseReturnDto
{
    [Required(ErrorMessage = "رقم إشعار الإرجاع مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم الإرجاع يجب ألا يتجاوز 50 حرف")]
    public string ReturnNumber { get; set; } = null!;

    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    public Guid? PurchaseInvoiceId { get; set; }

    [Required(ErrorMessage = "معرف المورد مطلوب")]
    public Guid SupplierId { get; set; }

    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    [Required(ErrorMessage = "معرف المستودع الرئيسي مطلوب")]
    public Guid WarehouseId { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public PurchaseReturnReason Reason { get; set; } = PurchaseReturnReason.Defective;

    [MaxLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل لمرتجع المشتريات")]
    public List<CreatePurchaseReturnItemDto> Items { get; set; } = new();
}

/// <summary>
/// بند في طلب إنشاء مرتجع المشتريات.
/// </summary>
public class CreatePurchaseReturnItemDto
{
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    public Guid? ProductBarCodeId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "الكمية المرتجعة يجب أن تكون أكبر من صفر")]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "سعر الإرجاع للوحدة يجب أن يكون أكبر من أو يساوي صفر")]
    public decimal UnitPrice { get; set; }

    [MaxLength(200, ErrorMessage = "الملاحظات يجب ألا تتجاوز 200 حرف")]
    public string? Notes { get; set; }
}

/// <summary>
/// ملخص بيانات مرتجع المشتريات لعرض القوائم والجداول والبحث.
/// </summary>
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
    public string ReasonName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// بيانات تفصيلية متكاملة لمرتجع المشتريات وبنوده.
/// </summary>
public class PurchaseReturnResponseDto
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
    public string ReasonName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PurchaseReturnItemResponseDto> Items { get; set; } = new();
}

/// <summary>
/// بند في الاستجابة التفصيلية لمرتجع المشتريات.
/// </summary>
public class PurchaseReturnItemResponseDto
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
