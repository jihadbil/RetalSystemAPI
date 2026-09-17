using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات استجابة بند مرتجع المبيعات (Sales Return Item Response DTO).
/// </summary>
public class SalesReturnItemResponseDto : BaseDto
{
    /// <summary>معرف المنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = null!;

    /// <summary>معرف الباركود</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>عنوان ومسمى الباركود</summary>
    public string? BarcodeTitle { get; set; }

    /// <summary>رمز الباركود</summary>
    public string? BarcodeValue { get; set; }

    /// <summary>الكمية المرتجعة</summary>
    public int Quantity { get; set; }

    /// <summary>سعر الإرجاع للوحدة</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>إجمالي سطر البند</summary>
    public decimal LineTotal { get; set; }

    /// <summary>ملاحظات</summary>
    public string? Notes { get; set; }
}
