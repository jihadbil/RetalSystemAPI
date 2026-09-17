using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات ملخص العميل السريع (Customer Summary DTO).
/// خفيف ومخصص لعرض العملاء في جداول النظام وعمليات البحث الفوري.
/// </summary>
public class CustomerSummaryDto : BaseDto
{
    /// <summary>اسم العميل</summary>
    public string Name { get; set; } = null!;

    /// <summary>كود العميل</summary>
    public string? Code { get; set; }

    /// <summary>نوع العميل التعدادي</summary>
    public CustomerType Type { get; set; }

    /// <summary>اسم نوع العميل بالعربية</summary>
    public string TypeName { get; set; } = null!;

    /// <summary>سقف الائتمان المسموح به</summary>
    public decimal CreditLimit { get; set; }

    /// <summary>حالة نشاط حساب العميل</summary>
    public bool IsActive { get; set; }

    /// <summary>إجمالي عدد الهواتف المسجلة للعميل</summary>
    public int PhoneCount { get; set; }

    /// <summary>رقم الهاتف الأساسي الافتراضي للعميل</summary>
    public string? PrimaryPhone { get; set; }
}
