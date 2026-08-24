using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class SalesInvoiceItemDto
{
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    public Guid? ProductBarCodeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 1 على الأقل")]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "سعر الوحدة يجب أن يكون أكبر من أو يساوي 0")]
    public decimal UnitPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "تكلفة الوحدة يجب أن تكون أكبر من أو يساوي 0")]
    public decimal UnitCost { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم يجب أن تكون أكبر من أو تساوي 0")]
    public decimal DiscountAmount { get; set; } = 0;
}
