using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

/// <summary>
/// ناقل بيانات استجابة بند أمر التحويل المخزني (Stock Transfer Item Response DTO).
/// </summary>
public class StockTransferItemResponseDto : BaseDto
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

    /// <summary>الكمية المحولة</summary>
    public int Quantity { get; set; }

    /// <summary>ملاحظات</summary>
    public string? Notes { get; set; }
}
