using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Purchase;

/// <summary>
/// ناقل بيانات استجابة بند أمر الشراء (Purchase Order Item Response DTO).
/// </summary>
public class PurchaseOrderItemResponseDto : BaseDto
{
    /// <summary>معرف الباركود</summary>
    public Guid ProductBarCodeId { get; set; }

    /// <summary>عنوان ومسمى الباركود</summary>
    public string BarcodeTitle { get; set; } = null!;

    /// <summary>رمز الباركود</summary>
    public string BarcodeValue { get; set; } = null!;

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = null!;

    /// <summary>الكمية المطلوبة</summary>
    public decimal Quantity { get; set; }

    /// <summary>سعر شراء الوحدة</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>إجمالي سطر البند (الكمية × السعر)</summary>
    public decimal LineTotal { get; set; }
}
