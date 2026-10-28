using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات ملخص المورد السريع (Supplier Summary DTO).
/// خفيف ومخصص لعرض الموردين في الجداول وشاشات الاختيار السريع.
/// </summary>
public class SupplierSummaryDto : BaseDto
{
    /// <summary>اسم الشركة الموردة التجارية أو المؤسسة</summary>
    public string? CompanyName { get; set; }

    /// <summary>البريد الإلكتروني للتواصل</summary>
    public string? Email { get; set; }

    /// <summary>الرقم الضريبي للمورد</summary>
    public string? TaxNumber { get; set; }

    /// <summary>ملاحظات إضافية حول المورد</summary>
    public string? Notes { get; set; }

    /// <summary>حالة نشاط حساب المورد</summary>
    public bool IsActive { get; set; }

    /// <summary>رقم الهاتف الأساسي الافتراضي للمورد</summary>
    public string? PrimaryPhone { get; set; }

    /// <summary>اسم المورد</summary>
    public string Name { get; set; } = null!;

    /// <summary>عنوان المورد</summary>
    public string? Address { get; set; }

    /// <summary>الرصيد المالي الافتتاحي</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>الرصيد الحالي المستحق للمورد</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>إجمالي عدد الهواتف المسجلة للمورد</summary>
    public int PhoneCount { get; set; }
}
