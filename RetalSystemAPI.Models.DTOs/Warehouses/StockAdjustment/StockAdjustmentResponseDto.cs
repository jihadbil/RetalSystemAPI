using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

/// <summary>
/// ناقل بيانات استجابة تفاصيل التسوية الجردية الشاملة (Stock Adjustment Response DTO).
/// </summary>
public class StockAdjustmentResponseDto : BaseDto
{
    /// <summary>رقم إشعار التسوية الجردية</summary>
    public string AdjustmentNumber { get; set; } = null!;

    /// <summary>تاريخ التسوية</summary>
    public DateTime AdjustmentDate { get; set; }

    /// <summary>معرف المستودع أو الصالة</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم المستودع</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>سبب التسوية العام</summary>
    public StockAdjustmentReason Reason { get; set; }

    /// <summary>اسم سبب التسوية المعرب</summary>
    public string ReasonName { get; set; } = null!;

    /// <summary>ملاحظات إضافية</summary>
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الأصناف المسواة</summary>
    public List<StockAdjustmentItemResponseDto> Items { get; set; } = new();
}
