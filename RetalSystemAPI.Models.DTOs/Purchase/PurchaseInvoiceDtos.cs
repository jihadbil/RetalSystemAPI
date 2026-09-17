using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Purchase;

/// <summary>
/// ناقل بيانات ملخص فاتورة المشتريات (Purchase Invoice Summary DTO).
/// خفيف ومخصص لعرض الفواتير في الجداول وقوائم البحث السريع.
/// </summary>
public class PurchaseInvoiceSummaryDto : BaseDto
{
    /// <summary>رقم فاتورة الشراء</summary>
    public string InvoiceNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت الفاتورة</summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>معرف المورد</summary>
    public Guid SupplierId { get; set; }

    /// <summary>اسم المورد</summary>
    public string SupplierName { get; set; } = null!;

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>معرف المستودع</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم المستودع</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>حالة الفاتورة</summary>
    public InvoiceStatus Status { get; set; }

    /// <summary>اسم حالة الفاتورة المعرب</summary>
    public string StatusName { get; set; } = null!;

    /// <summary>طريقة الدفع</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>اسم طريقة الدفع المعرب</summary>
    public string PaymentMethodName { get; set; } = null!;

    /// <summary>المجموع الفرعي قبل الخصم والضريبة</summary>
    public decimal SubTotal { get; set; }

    /// <summary>قيمة الخصم الممنوح</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>مبلغ الضريبة</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>المبلغ الإجمالي النهائي للفاتورة</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المدفوع للمورد</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>المبلغ المتبقي لصالح المورد</summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>إجمالي عدد الأصناف في الفاتورة</summary>
    public int ItemsCount { get; set; }
}

/// <summary>
/// ناقل بيانات استجابة تفاصيل فاتورة المشتريات الشاملة (Purchase Invoice Response DTO).
/// يتضمن المبالغ المالية، الملاحظات، وبيانات البنود وتفاصيل تفكيك العبوات.
/// </summary>
public class PurchaseInvoiceResponseDto : BaseDto
{
    /// <summary>رقم فاتورة الشراء</summary>
    public string InvoiceNumber { get; set; } = null!;

    /// <summary>تاريخ إصدار الفاتورة</summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>معرف المورد</summary>
    public Guid SupplierId { get; set; }

    /// <summary>اسم المورد</summary>
    public string SupplierName { get; set; } = null!;

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>معرف المستودع المستلم</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم المستودع</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>حالة الفاتورة التعدادية</summary>
    public InvoiceStatus Status { get; set; }

    /// <summary>اسم الحالة المعرب</summary>
    public string StatusName { get; set; } = null!;

    /// <summary>طريقة الدفع</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>اسم طريقة الدفع المعرب</summary>
    public string PaymentMethodName { get; set; } = null!;

    /// <summary>المجموع الفرعي</summary>
    public decimal SubTotal { get; set; }

    /// <summary>قيمة الخصم</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>مبلغ الضريبة</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>المبلغ الإجمالي الصافي</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المدفوع</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>المبلغ المتبقي</summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>معرف أمر الشراء المرتبط إن وجد</summary>
    public Guid? PurchaseOrderId { get; set; }

    /// <summary>رقم أمر الشراء المرتبط</summary>
    public string? PurchaseOrderNumber { get; set; }

    /// <summary>ملاحظات الفاتورة</summary>
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الفاتورة التفصيلية</summary>
    public List<PurchaseInvoiceItemResponseDto> Items { get; set; } = new();
}

/// <summary>
/// ناقل بيانات استجابة بند فاتورة المشتريات (Purchase Invoice Item Response DTO).
/// </summary>
public class PurchaseInvoiceItemResponseDto : BaseDto
{
    /// <summary>معرف المنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = null!;

    /// <summary>كود المنتج الداخلي</summary>
    public string? ProductCode { get; set; }

    /// <summary>معرف الباركود</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>رمز الباركود</summary>
    public string? BarCode { get; set; }

    /// <summary>الكمية المشتراة</summary>
    public decimal Quantity { get; set; }

    /// <summary>سعر شراء الوحدة</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>مبلغ الخصم على البند</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>إجمالي سطر البند</summary>
    public decimal LineTotal { get; set; }

    /// <summary>تفاصيل تفكيك العبوات والوحدات التابعة للبند</summary>
    public List<PurchaseInvoiceItemBreakdownResponseDto> Breakdowns { get; set; } = new();
}

/// <summary>
/// ناقل بيانات استجابة تفكيك عبوة الشراء (Purchase Invoice Breakdown Response DTO).
/// </summary>
public class PurchaseInvoiceItemBreakdownResponseDto : BaseDto
{
    /// <summary>معرف باركود العبوة أو الوحدة الصغرى</summary>
    public Guid ProductBarCodeId { get; set; }

    /// <summary>رمز الباركود</summary>
    public string? BarCode { get; set; }

    /// <summary>عنوان ومسمى الباركود</summary>
    public string? Title { get; set; }

    /// <summary>كمية العبوات</summary>
    public decimal PackageQuantity { get; set; }

    /// <summary>عدد الوحدات بكل عبوة</summary>
    public int UnitsPerPackage { get; set; }

    /// <summary>الكمية الإجمالية بالوحدات</summary>
    public decimal Quantity { get; set; }

    /// <summary>سعر تكلفة الوحدة</summary>
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// ناقل بيانات إنشاء فاتورة شراء جديدة (Create Purchase Invoice DTO).
/// </summary>
public class CreatePurchaseInvoiceDto
{
    /// <summary>رقم فاتورة الشراء</summary>
    public string InvoiceNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت إصدار الفاتورة</summary>
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    /// <summary>معرف المورد</summary>
    public Guid SupplierId { get; set; }

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>معرف المستودع المستلم</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>حالة الفاتورة (افتراضي: مدفوعة ومغلقة)</summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;

    /// <summary>طريقة الدفع (افتراضي: نقدي)</summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>المجموع الفرعي</summary>
    public decimal SubTotal { get; set; }

    /// <summary>مبلغ الخصم</summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>مبلغ الضريبة</summary>
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>المبلغ الإجمالي الكلي</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المدفوع للمورد</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>معرف أمر الشراء المرتبط إن وجد</summary>
    public Guid? PurchaseOrderId { get; set; }

    /// <summary>ملاحظات إضافية على الفاتورة</summary>
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الأصناف المشتراة</summary>
    public List<CreatePurchaseInvoiceItemDto> Items { get; set; } = new();
}

/// <summary>
/// ناقل بيانات إنشاء بند فاتورة شراء (Create Purchase Invoice Item DTO).
/// </summary>
public class CreatePurchaseInvoiceItemDto
{
    /// <summary>معرف المنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>معرف الباركود</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>الكمية المشتراة</summary>
    public decimal Quantity { get; set; }

    /// <summary>سعر الوحدة</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>مبلغ الخصم</summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>تفاصيل تفكيك العبوات إن وجدت</summary>
    public List<CreatePurchaseInvoiceItemBreakdownDto>? Breakdowns { get; set; }
}

/// <summary>
/// ناقل بيانات تفكيك عبوة الشراء في طلب الإنشاء.
/// </summary>
public class CreatePurchaseInvoiceItemBreakdownDto
{
    /// <summary>معرف الباركود للوحدة المفككة</summary>
    public Guid ProductBarCodeId { get; set; }

    /// <summary>كمية العبوات المشتراة</summary>
    public decimal PackageQuantity { get; set; } = 1;

    /// <summary>عدد القطع أو الوحدات داخل العبوة الواحدة</summary>
    public int UnitsPerPackage { get; set; } = 1;

    /// <summary>الكمية الإجمالية المحتسبة بالقطع</summary>
    public decimal Quantity { get; set; }

    /// <summary>سعر تكلفة القطعة</summary>
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// ناقل بيانات تعديل فاتورة شراء قائمة (Update Purchase Invoice DTO).
/// </summary>
public class UpdatePurchaseInvoiceDto
{
    /// <summary>رقم فاتورة الشراء</summary>
    public string InvoiceNumber { get; set; } = null!;

    /// <summary>تاريخ الفاتورة</summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>معرف المورد</summary>
    public Guid SupplierId { get; set; }

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>معرف المستودع</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>حالة الفاتورة</summary>
    public InvoiceStatus Status { get; set; }

    /// <summary>طريقة الدفع</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>المجموع الفرعي</summary>
    public decimal SubTotal { get; set; }

    /// <summary>مبلغ الخصم</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>مبلغ الضريبة</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>المبلغ الإجمالي</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المدفوع</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>ملاحظات</summary>
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الفاتورة المحدثة</summary>
    public List<CreatePurchaseInvoiceItemDto>? Items { get; set; }
}
