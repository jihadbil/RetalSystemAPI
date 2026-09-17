using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات ملخص المورد السريع (Supplier Summary DTO).
/// خفيف ومخصص لعرض الموردين في الجداول وشاشات الاختيار السريع.
/// </summary>
public class SupplierSummaryDto : BaseDto
{
    /// <summary>اسم المورد</summary>
    public string Name { get; set; } = null!;

    /// <summary>عنوان المورد</summary>
    public string? Address { get; set; }

    /// <summary>الرصيد المالي الافتتاحي</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>إجمالي عدد الهواتف المسجلة للمورد</summary>
    public int PhoneCount { get; set; }
}
