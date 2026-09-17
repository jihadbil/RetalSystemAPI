using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات استجابة بند فاتورة المبيعات (Sales Invoice Item Response DTO).
/// </summary>
public class SalesInvoiceItemResponseDto : BaseDto
{
    /// <summary>معرف المنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = null!;

    /// <summary>معرف الباركود</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>عنوان الباركود</summary>
    public string? BarcodeTitle { get; set; }

    /// <summary>رمز الباركود</summary>
    public string? BarcodeValue { get; set; }

    /// <summary>الكمية المباعة</summary>
    public int Quantity { get; set; }

    /// <summary>سعر بيع الوحدة</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>تكلفة الوحدة</summary>
    public decimal UnitCost { get; set; }

    /// <summary>مبلغ الخصم</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>إجمالي سطر البند بعد الخصم</summary>
    public decimal LineTotal { get; set; }
}
