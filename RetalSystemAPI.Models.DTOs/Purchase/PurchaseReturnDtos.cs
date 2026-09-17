using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Purchase;

/// <summary>
/// ناقل بيانات إنشاء مرتجع مشتريات جديد (Create Purchase Return DTO).
/// يتضمن سبب الإرجاع، طريقة استرداد المبلغ، وقائمة البنود المرتجعة للمورد.
/// </summary>
public class CreatePurchaseReturnDto
{
    /// <summary>رقم إشعار الإرجاع</summary>
    [Required(ErrorMessage = "رقم إشعار الإرجاع مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم الإرجاع يجب ألا يتجاوز 50 حرف")]
    public string ReturnNumber { get; set; } = null!;

    /// <summary>تاريخ الإرجاع</summary>
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    /// <summary>معرف فاتورة الشراء الأصلية المراد الإرجاع منها إن وجدت</summary>
    public Guid? PurchaseInvoiceId { get; set; }

    /// <summary>معرف المورد</summary>
    [Required(ErrorMessage = "معرف المورد مطلوب")]
    public Guid SupplierId { get; set; }

    /// <summary>معرف الفرع</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    /// <summary>معرف المستودع المسحوبة منه البضاعة المرتجعة</summary>
    [Required(ErrorMessage = "معرف المستودع الرئيسي مطلوب")]
    public Guid WarehouseId { get; set; }

    /// <summary>طريقة التسوية أو استرداد القيمة</summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>سبب إرجاع المشتريات (تالف، مخالف للمواصفات، منتهي الصلاحية، إلخ)</summary>
    public PurchaseReturnReason Reason { get; set; } = PurchaseReturnReason.Defective;

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    /// <summary>قائمة البنود المطلوب إرجاعها للمورد</summary>
    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل لمرتجع المشتريات")]
    public List<CreatePurchaseReturnItemDto> Items { get; set; } = new();
}

/// <summary>
/// ناقل بيانات بند طلب إنشاء مرتجع المشتريات (Create Purchase Return Item DTO).
/// </summary>
public class CreatePurchaseReturnItemDto
{
    /// <summary>معرف الصنف</summary>
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    /// <summary>معرف الباركود</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>الكمية المرتجعة</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "الكمية المرتجعة يجب أن تكون أكبر من صفر")]
    public decimal Quantity { get; set; }

    /// <summary>سعر وحدة الصنف المرتجع</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سعر الإرجاع للوحدة يجب أن يكون أكبر من أو يساوي صفر")]
    public decimal UnitPrice { get; set; }

    /// <summary>ملاحظات على بند الإرجاع</summary>
    [MaxLength(200, ErrorMessage = "الملاحظات يجب ألا تتجاوز 200 حرف")]
    public string? Notes { get; set; }
}

/// <summary>
/// ناقل بيانات ملخص مرتجع المشتريات (Purchase Return Summary DTO).
/// مخصص لعرض المرتجعات في الجداول وقوائم المتابعة.
/// </summary>
public class PurchaseReturnSummaryDto
{
    /// <summary>المعرف الفريد للمرتجع</summary>
    public Guid Id { get; set; }

    /// <summary>رقم إشعار الإرجاع</summary>
    public string ReturnNumber { get; set; } = string.Empty;

    /// <summary>تاريخ الإرجاع</summary>
    public DateTime ReturnDate { get; set; }

    /// <summary>معرف فاتورة الشراء الأصلية</summary>
    public Guid? PurchaseInvoiceId { get; set; }

    /// <summary>رقم فاتورة الشراء الأصلية</summary>
    public string? PurchaseInvoiceNumber { get; set; }

    /// <summary>معرف المورد</summary>
    public Guid SupplierId { get; set; }

    /// <summary>اسم المورد</summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>معرف المستودع</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم المستودع</summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>طريقة الدفع أو التسوية</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>اسم طريقة الدفع المعرب</summary>
    public string PaymentMethodName { get; set; } = string.Empty;

    /// <summary>المبلغ الإجمالي للمرتجع</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>سبب الإرجاع التعدادي</summary>
    public PurchaseReturnReason Reason { get; set; }

    /// <summary>اسم سبب الإرجاع المعرب</summary>
    public string ReasonName { get; set; } = string.Empty;

    /// <summary>إجمالي عدد البنود المرتجعة</summary>
    public int ItemCount { get; set; }

    /// <summary>ملاحظات</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// ناقل بيانات استجابة تفاصيل مرتجع المشتريات الشاملة (Purchase Return Response DTO).
/// </summary>
public class PurchaseReturnResponseDto
{
    /// <summary>المعرف الفريد للمرتجع</summary>
    public Guid Id { get; set; }

    /// <summary>رقم إشعار الإرجاع</summary>
    public string ReturnNumber { get; set; } = string.Empty;

    /// <summary>تاريخ الإرجاع</summary>
    public DateTime ReturnDate { get; set; }

    /// <summary>معرف فاتورة الشراء الأصلية</summary>
    public Guid? PurchaseInvoiceId { get; set; }

    /// <summary>رقم فاتورة الشراء الأصلية</summary>
    public string? PurchaseInvoiceNumber { get; set; }

    /// <summary>معرف المورد</summary>
    public Guid SupplierId { get; set; }

    /// <summary>اسم المورد</summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>معرف المستودع</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم المستودع</summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>طريقة التسوية المالية</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>اسم طريقة التسوية المعرب</summary>
    public string PaymentMethodName { get; set; } = string.Empty;

    /// <summary>المبلغ الإجمالي المسترد أو المخفض</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>سبب الإرجاع</summary>
    public PurchaseReturnReason Reason { get; set; }

    /// <summary>اسم سبب الإرجاع المعرب</summary>
    public string ReasonName { get; set; } = string.Empty;

    /// <summary>ملاحظات</summary>
    public string? Notes { get; set; }

    /// <summary>تاريخ الإنشاء</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>قائمة بنود المرتجع التفصيلية</summary>
    public List<PurchaseReturnItemResponseDto> Items { get; set; } = new();
}

/// <summary>
/// ناقل بيانات استجابة بند مرتجع المشتريات (Purchase Return Item Response DTO).
/// </summary>
public class PurchaseReturnItemResponseDto
{
    /// <summary>المعرف الفريد لسجل البند</summary>
    public Guid Id { get; set; }

    /// <summary>معرف المنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>معرف الباركود</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>عنوان الباركود</summary>
    public string? BarcodeTitle { get; set; }

    /// <summary>رمز الباركود</summary>
    public string? BarcodeValue { get; set; }

    /// <summary>الكمية المرتجعة</summary>
    public decimal Quantity { get; set; }

    /// <summary>سعر الوحدة المرتجعة</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>إجمالي سطر البند</summary>
    public decimal LineTotal { get; set; }

    /// <summary>ملاحظات</summary>
    public string? Notes { get; set; }
}
