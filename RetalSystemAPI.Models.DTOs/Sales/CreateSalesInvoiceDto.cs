using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class CreateSalesInvoiceDto
{
    [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم الفاتورة يجب أن لا يتجاوز 50 حرف")]
    public string InvoiceNumber { get; set; } = null!;

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    [Required(ErrorMessage = "معرف المستودع أو صالة العرض مطلوب")]
    public Guid WarehouseId { get; set; }

    public Guid? CustomerId { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [Range(0, double.MaxValue, ErrorMessage = "الإجمالي قبل الخصم يجب أن يكون أكبر من أو يساوي 0")]
    public decimal SubTotal { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم يجب أن تكون أكبر من أو تساوي 0")]
    public decimal DiscountAmount { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "الإجمالي الصافي يجب أن يكون أكبر من أو يساوي 0")]
    public decimal TotalAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدفوع يجب أن يكون أكبر من أو يساوي 0")]
    public decimal PaidAmount { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل للفاتورة")]
    public List<SalesInvoiceItemDto> Items { get; set; } = new();
}
