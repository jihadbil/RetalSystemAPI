using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class SalesReturnItemDto
{
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    public Guid? ProductBarCodeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "الكمية المرتجعة يجب أن تكون 1 على الأقل")]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "سعر الإرجاع للوحدة يجب أن يكون أكبر من أو يساوي 0")]
    public decimal UnitPrice { get; set; }

    [MaxLength(200, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 200 حرف")]
    public string? Notes { get; set; }
}
