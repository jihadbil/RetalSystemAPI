using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات إنشاء فاتورة مبيعات جديدة (Create Sales Invoice DTO).
/// يتضمن بيانات الفرع والمستودع، العميل، طريقة السداد، المبالغ والخصم، وقائمة بنود الفاتورة.
/// </summary>
public class CreateSalesInvoiceDto
{
    /// <summary>رقم فاتورة المبيعات الفريد</summary>
    [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم الفاتورة يجب أن لا يتجاوز 50 حرف")]
    public string InvoiceNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت إصدار الفاتورة</summary>
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    /// <summary>معرف الفرع المنفذ للعملية</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    /// <summary>معرف المستودع أو صالة العرض المسحوب منها البضاعة</summary>
    [Required(ErrorMessage = "معرف المستودع أو صالة العرض مطلوب")]
    public Guid WarehouseId { get; set; }

    /// <summary>معرف العميل (اختياري، في حال البيع النقدي المباشر)</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>حالة الفاتورة (افتراضي: مدفوعة ومكتملة)</summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;

    /// <summary>طريقة الدفع المعتمدة (نقدي، بطاقة، صك، آجل)</summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>المجموع الفرعي قبل الخصم</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الإجمالي قبل الخصم يجب أن يكون أكبر من أو يساوي 0")]
    public decimal SubTotal { get; set; }

    /// <summary>قيمة الخصم الممنوح على إجمالي الفاتورة</summary>
    [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم يجب أن تكون أكبر من أو تساوي 0")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>الإجمالي النهائي بعد الخصم</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الإجمالي الصافي يجب أن يكون أكبر من أو يساوي 0")]
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المسدد من قبل العميل</summary>
    [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدفوع يجب أن يكون أكبر من أو يساوي 0")]
    public decimal PaidAmount { get; set; }

    /// <summary>ملاحظات إضافية على الفاتورة</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الأصناف المباعة</summary>
    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل للفاتورة")]
    public List<SalesInvoiceItemDto> Items { get; set; } = new();
}
