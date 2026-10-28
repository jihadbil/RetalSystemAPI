using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات استجابة تفاصيل المورد (Supplier Response DTO).
/// يتضمن الرصيد الافتتاحي، العنوان، وقائمة أرقام هواتف المورد.
/// </summary>
public class SupplierResponseDto : BaseDto
{
    /// <summary>اسم الشركة الموردة التجارية أو المؤسسة</summary>
    public string? CompanyName { get; set; }

    /// <summary>البريد الإلكتروني للمورد</summary>
    public string? Email { get; set; }

    /// <summary>الرقم الضريبي للمورد</summary>
    public string? TaxNumber { get; set; }

    /// <summary>ملاحظات إضافية حول المورد</summary>
    public string? Notes { get; set; }

    /// <summary>حالة نشاط حساب المورد</summary>
    public bool IsActive { get; set; }

    /// <summary>اسم المورد أو الشركة</summary>
    public string Name { get; set; } = null!;

    /// <summary>عنوان مقر المورد</summary>
    public string? Address { get; set; }

    /// <summary>الرصيد المالي الافتتاحي</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>الرصيد الحالي المستحق للمورد</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>قائمة أرقام هواتف المورد المسجلة</summary>
    public List<SupplierPhoneResponseDto> Phones { get; set; } = new();
}
