using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Purchase;

public class PurchaseOrderItemDto
{
    [Required(ErrorMessage = "معرف الباركود مطلوب")]
    public Guid ProductBarCodeId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من 0")]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "سعر الوحدة لا يمكن أن يكون سالبًا")]
    public decimal UnitPrice { get; set; }
}
