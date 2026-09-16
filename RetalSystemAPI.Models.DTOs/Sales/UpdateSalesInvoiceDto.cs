using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class UpdateSalesInvoiceDto
{
    [Required(ErrorMessage = "معرف الفاتورة مطلوب")]
    public Guid Id { get; set; }

    public InvoiceStatus Status { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدفوع يجب أن يكون أكبر من أو يساوي 0")]
    public decimal PaidAmount { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    public List<SalesInvoiceItemDto>? Items { get; set; }
}
