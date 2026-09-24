using System;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات تسجيل سند قبض / سداد من عميل (Customer Payment DTO).
/// </summary>
public class CreateCustomerPaymentDto
{
    /// <summary>معرف العميل</summary>
    [Required(ErrorMessage = "معرف العميل مطلوب")]
    public Guid CustomerId { get; set; }

    /// <summary>المبلغ المسدد / المقبوض</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ السداد يجب أن يكون أكبر من الصفر")]
    public decimal Amount { get; set; }

    /// <summary>طريقة الدفع</summary>
    [Required(ErrorMessage = "طريقة الدفع مطلوبة")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>معرف الخزينة أو الدرج المودع به المبلغ</summary>
    public Guid? SafeId { get; set; }

    /// <summary>معرف الفرع</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    /// <summary>تاريخ السداد</summary>
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    /// <summary>البيان / الشرح</summary>
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string? Description { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
