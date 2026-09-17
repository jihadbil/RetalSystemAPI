using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

/// <summary>
/// ناقل بيانات ملخص أمر التحويل المخزني السريع (Stock Transfer Summary DTO).
/// خفيف ومخصص لعرض التحويلات في الجداول وقوائم المتابعة والبحث.
/// </summary>
public class StockTransferSummaryDto : BaseDto
{
    /// <summary>رقم أمر التحويل</summary>
    public string TransferNumber { get; set; } = null!;

    /// <summary>تاريخ التحويل</summary>
    public DateTime TransferDate { get; set; }

    /// <summary>اسم المستودع المصدر</summary>
    public string FromWarehouseName { get; set; } = null!;

    /// <summary>اسم المستودع الوجهة</summary>
    public string ToWarehouseName { get; set; } = null!;

    /// <summary>حالة أمر التحويل</summary>
    public StockTransferStatus Status { get; set; }

    /// <summary>اسم حالة أمر التحويل المعرب</summary>
    public string StatusName { get; set; } = null!;

    /// <summary>عدد البنود المحولة</summary>
    public int ItemCount { get; set; }
}
