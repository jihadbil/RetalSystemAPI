using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات استجابة هاتف المورد (Supplier Phone Response DTO).
/// </summary>
public class SupplierPhoneResponseDto : BaseDto
{
    /// <summary>اسم جهة الاتصال</summary>
    public string? Name { get; set; }

    /// <summary>رقم الهاتف المسجل</summary>
    public string PhoneNumber { get; set; } = null!;
}
