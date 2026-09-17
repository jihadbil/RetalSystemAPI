using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses;

/// <summary>
/// ناقل بيانات ملخص المستودع السريع (Warehouse Summary DTO).
/// خفيف ومخصص لعرض المستودعات في القوائم المنسدلة وجداول النظام.
/// </summary>
public class WarehouseSummaryDto : BaseDto
{
    /// <summary>اسم المستودع</summary>
    public string Name { get; set; } = null!;

    /// <summary>نوع المستودع</summary>
    public WarehouseType Type { get; set; }

    /// <summary>اسم نوع المستودع المعرب</summary>
    public string TypeName { get; set; } = null!;

    /// <summary>حالة نشاط المستودع</summary>
    public bool IsActive { get; set; }

    /// <summary>اسم الفرع التابع له</summary>
    public string BranchName { get; set; } = null!;
}
