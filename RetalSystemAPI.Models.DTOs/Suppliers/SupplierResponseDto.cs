using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات استجابة تفاصيل المورد (Supplier Response DTO).
/// يتضمن الرصيد الافتتاحي، العنوان، وقائمة أرقام هواتف المورد.
/// </summary>
public class SupplierResponseDto : BaseDto
{
    /// <summary>اسم المورد أو الشركة</summary>
    public string Name { get; set; } = null!;

    /// <summary>عنوان مقر المورد</summary>
    public string? Address { get; set; }

    /// <summary>الرصيد المالي الافتتاحي</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>قائمة أرقام هواتف المورد المسجلة</summary>
    public List<SupplierPhoneResponseDto> Phones { get; set; } = new();
}
