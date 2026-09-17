using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

/// <summary>
/// ناقل بيانات ملخص التسوية الجردية السريع (Stock Adjustment Summary DTO).
/// خفيف ومخصص لعرض التسويات في الجداول وقوائم البحث والمتابعة.
/// </summary>
public class StockAdjustmentSummaryDto : BaseDto
{
    /// <summary>رقم إشعار التسوية الجردية</summary>
    public string AdjustmentNumber { get; set; } = null!;

    /// <summary>تاريخ التسوية</summary>
    public DateTime AdjustmentDate { get; set; }

    /// <summary>اسم المستودع أو الصالة</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>سبب التسوية</summary>
    public StockAdjustmentReason Reason { get; set; }

    /// <summary>اسم سبب التسوية المعرب</summary>
    public string ReasonName { get; set; } = null!;

    /// <summary>عدد البنود المشمولة بالتسوية</summary>
    public int ItemCount { get; set; }
}
