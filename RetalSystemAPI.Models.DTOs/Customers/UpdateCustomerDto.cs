using RetalSystemAPI.Models.DTOs.Common;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات تعديل بيانات عميل قائم (Update Customer DTO).
/// </summary>
public class UpdateCustomerDto
{
    /// <summary>الرقم الضريبي المحدث للعميل</summary>
    [MaxLength(100)]
    public string? TaxNumber { get; set; }

    /// <summary>رقم السجل التجاري المحدث</summary>
    [MaxLength(100)]
    public string? CommercialRegister { get; set; }

    /// <summary>ملاحظات إضافية حول العميل</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>نسبة الخصم الممنوح بالنسبة المئوية (0 - 100)</summary>
    [Range(0, 100)]
    [MoneyAmount]
    public decimal DiscountRate { get; set; }

    /// <summary>المعرف الفريد للعميل المطلوب تعديله</summary>
    [Required(ErrorMessage = "معرف العميل مطلوب")]
    public Guid Id { get; set; }

    /// <summary>اسم العميل أو المؤسسة الجديد</summary>
    [Required(ErrorMessage = "اسم العميل مطلوب")]
    [MaxLength(100, ErrorMessage = "اسم العميل يجب أن لا يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>كود العميل</summary>
    [MaxLength(50, ErrorMessage = "كود العميل يجب أن لا يتجاوز 50 حرف")]
    public string? Code { get; set; }

    /// <summary>عنوان البريد الإلكتروني</summary>
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [MaxLength(100, ErrorMessage = "البريد الإلكتروني يجب أن لا يتجاوز 100 حرف")]
    public string? Email { get; set; }

    /// <summary>العنوان</summary>
    [MaxLength(200, ErrorMessage = "العنوان يجب أن لا يتجاوز 200 حرف")]
    public string? Address { get; set; }

    /// <summary>نوع العميل (تجزئة، جملة، شركات)</summary>
    [EnumDataType(typeof(CustomerType))]
    public CustomerType Type { get; set; } = CustomerType.Retail;

    /// <summary>سقف الائتمان المالي</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سقف الدين يجب أن يكون أكبر من أو يساوي 0")]
    [MoneyAmount]
    public decimal CreditLimit { get; set; }

    /// <summary>حالة نشاط حساب العميل</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>قائمة أرقام هواتف العميل المحدثة</summary>
    [NoNullItems]
    public List<CustomerPhoneDto> Phones { get; set; } = new();
}
