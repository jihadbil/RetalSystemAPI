using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات تعديل فاتورة مبيعات قائمة (Update Sales Invoice DTO).
/// </summary>
public class UpdateSalesInvoiceDto
{
    /// <summary>المعرف الفريد للفاتورة المطلوب تعديلها</summary>
    [Required(ErrorMessage = "معرف الفاتورة مطلوب")]
    public Guid Id { get; set; }

    /// <summary>حالة الفاتورة الجديدة</summary>
    public InvoiceStatus Status { get; set; }

    /// <summary>طريقة الدفع</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>المبلغ المسدد المحدث</summary>
    [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدفوع يجب أن يكون أكبر من أو يساوي 0")]
    public decimal PaidAmount { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الفاتورة المحدثة إن وجدت</summary>
    public List<SalesInvoiceItemDto>? Items { get; set; }
}
