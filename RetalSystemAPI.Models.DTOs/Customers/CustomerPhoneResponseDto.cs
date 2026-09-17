using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات استجابة هاتف العميل (Customer Phone Response DTO).
/// </summary>
public class CustomerPhoneResponseDto : BaseDto
{
    /// <summary>معرف العميل التابع له الهاتف</summary>
    public Guid CustomerId { get; set; }

    /// <summary>رقم الهاتف المسجل</summary>
    public string PhoneNumber { get; set; } = null!;

    /// <summary>اسم جهة الاتصال</summary>
    public string? ContactName { get; set; }

    /// <summary>هل هو الرقم الافتراضي</summary>
    public bool IsDefault { get; set; }
}
