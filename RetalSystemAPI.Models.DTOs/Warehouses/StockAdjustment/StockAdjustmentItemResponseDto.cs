using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

/// <summary>
/// ناقل بيانات استجابة بند التسوية الجردية (Stock Adjustment Item Response DTO).
/// </summary>
public class StockAdjustmentItemResponseDto : BaseDto
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

    /// <summary>الكمية الدفترية</summary>
    public int SystemQuantity { get; set; }

    /// <summary>الكمية الفعلية</summary>
    public int ActualQuantity { get; set; }

    /// <summary>فارق الجرد المحسوب (الفعلي - الدفتري)</summary>
    public int DifferenceQuantity { get; set; }

    /// <summary>تكلفة الوحدة</summary>
    public decimal UnitCost { get; set; }

    /// <summary>سبب التسوية للبند</summary>
    public StockAdjustmentReason Reason { get; set; }

    /// <summary>اسم سبب التسوية المعرب</summary>
    public string ReasonName { get; set; } = string.Empty;
}
