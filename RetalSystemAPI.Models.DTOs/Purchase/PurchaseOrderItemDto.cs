using RetalSystemAPI.Models.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Purchase;

/// <summary>
/// ناقل بيانات بند أمر الشراء (Purchase Order Item DTO).
/// </summary>
public class PurchaseOrderItemDto
{
    /// <summary>معرف باركود الصنف المطلوب شراؤه</summary>
    [Required(ErrorMessage = "معرف الباركود مطلوب")]
    [NotEmptyGuid]
    public Guid ProductBarCodeId { get; set; }

    /// <summary>الكمية المطلوبة</summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من 0")]
    [WholeQuantity]
    public decimal Quantity { get; set; }

    /// <summary>سعر شراء الوحدة المتفق عليه</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سعر الوحدة لا يمكن أن يكون سالبًا")]
    [MoneyAmount]
    public decimal UnitPrice { get; set; }
}
