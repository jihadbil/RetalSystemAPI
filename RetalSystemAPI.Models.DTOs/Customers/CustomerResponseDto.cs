using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات استجابة تفاصيل العميل (Customer Response DTO).
/// يتضمن البيانات المالية وسقف الائتمان والحالة وقائمة الهواتف المسجلة.
/// </summary>
public class CustomerResponseDto : BaseDto
{
    /// <summary>اسم العميل</summary>
    public string Name { get; set; } = null!;

    /// <summary>كود العميل</summary>
    public string? Code { get; set; }

    /// <summary>البريد الإلكتروني</summary>
    public string? Email { get; set; }

    /// <summary>العنوان</summary>
    public string? Address { get; set; }

    /// <summary>نوع العميل التعدادي</summary>
    public CustomerType Type { get; set; }

    /// <summary>اسم نوع العميل بالعربية (قطاعي، جملة، شركات)</summary>
    public string TypeName { get; set; } = null!;

    /// <summary>الرصيد الافتتاحي</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>سقف الائتمان والمديونية المسموح به</summary>
    public decimal CreditLimit { get; set; }

    /// <summary>حالة نشاط حساب العميل</summary>
    public bool IsActive { get; set; }

    /// <summary>قائمة أرقام هواتف العميل</summary>
    public List<CustomerPhoneResponseDto> Phones { get; set; } = new();
}
