using RetalSystemAPI.Models.DTOs.Common;
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
    [NotEmptyGuid]
    public Guid BranchId { get; set; }

    /// <summary>معرف المستودع أو صالة العرض المسحوب منها البضاعة</summary>
    [Required(ErrorMessage = "معرف المستودع أو صالة العرض مطلوب")]
    [NotEmptyGuid]
    public Guid WarehouseId { get; set; }

    /// <summary>معرف العميل (اختياري، في حال البيع النقدي المباشر)</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>حالة الفاتورة (افتراضي: مدفوعة ومكتملة)</summary>
    [EnumDataType(typeof(InvoiceStatus))]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;

    /// <summary>طريقة الدفع المعتمدة (نقدي، بطاقة، صك، آجل)</summary>
    [EnumDataType(typeof(PaymentMethod))]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>المجموع الفرعي قبل الخصم</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الإجمالي قبل الخصم يجب أن يكون أكبر من أو يساوي 0")]
    [MoneyAmount]
    public decimal SubTotal { get; set; }

    /// <summary>قيمة الخصم الممنوح على إجمالي الفاتورة</summary>
    [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم يجب أن تكون أكبر من أو تساوي 0")]
    [MoneyAmount]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>قيمة الضريبة على الفاتورة.</summary>
    [Range(0, double.MaxValue, ErrorMessage = "قيمة الضريبة يجب أن تكون أكبر من أو تساوي 0")]
    [MoneyAmount]
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>الإجمالي النهائي بعد الخصم</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الإجمالي الصافي يجب أن يكون أكبر من أو يساوي 0")]
    [MoneyAmount]
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المسدد من قبل العميل</summary>
    [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدفوع يجب أن يكون أكبر من أو يساوي 0")]
    [MoneyAmount]
    public decimal PaidAmount { get; set; }

    /// <summary>الخزينة المستلمة للمبلغ المدفوع. يمكن تركها فارغة عند عدم وجود دفعة.</summary>
    public Guid? PaymentSafeId { get; set; }

    /// <summary>وردية درج النقود المطلوبة للمبيعات النقدية من شاشة نقطة البيع.</summary>
    public Guid? CashSessionId { get; set; }

    /// <summary>ملاحظات إضافية على الفاتورة</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الأصناف المباعة</summary>
    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل للفاتورة")]
    [NoNullItems]
    public List<SalesInvoiceItemDto> Items { get; set; } = new();
}
