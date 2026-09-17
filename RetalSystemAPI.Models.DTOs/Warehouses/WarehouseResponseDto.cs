using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses;

/// <summary>
/// ناقل بيانات استجابة تفاصيل المستودع (Warehouse Response DTO).
/// </summary>
public class WarehouseResponseDto : BaseDto
{
    /// <summary>اسم المستودع أو الصالة</summary>
    public string Name { get; set; } = null!;

    /// <summary>نوع المستودع التعدادي</summary>
    public WarehouseType Type { get; set; }

    /// <summary>نوع المستودع معرباً (مخزن / صالة عرض)</summary>
    public string TypeName { get; set; } = null!;

    /// <summary>حالة نشاط المستودع</summary>
    public bool IsActive { get; set; }

    /// <summary>معرف الفرع التابع له</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;
}
