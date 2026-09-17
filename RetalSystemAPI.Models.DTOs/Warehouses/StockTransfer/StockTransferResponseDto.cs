using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

/// <summary>
/// ناقل بيانات استجابة تفاصيل أمر التحويل المخزني (Stock Transfer Response DTO).
/// يتضمن بيانات المستودع المصدر والمستودع الوجهة وحالة التحويل وقائمة البنود.
/// </summary>
public class StockTransferResponseDto : BaseDto
{
    /// <summary>رقم أمر التحويل</summary>
    public string TransferNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت أمر التحويل</summary>
    public DateTime TransferDate { get; set; }

    /// <summary>معرف المستودع المصدر</summary>
    public Guid FromWarehouseId { get; set; }

    /// <summary>اسم المستودع المصدر</summary>
    public string FromWarehouseName { get; set; } = null!;

    /// <summary>معرف المستودع الوجهة</summary>
    public Guid ToWarehouseId { get; set; }

    /// <summary>اسم المستودع الوجهة</summary>
    public string ToWarehouseName { get; set; } = null!;

    /// <summary>حالة أمر التحويل</summary>
    public StockTransferStatus Status { get; set; }

    /// <summary>اسم حالة أمر التحويل المعرب</summary>
    public string StatusName { get; set; } = null!;

    /// <summary>ملاحظات إضافية</summary>
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الأصناف المحولة</summary>
    public List<StockTransferItemResponseDto> Items { get; set; } = new();
}
