using RetalSystemAPI.Models.DTOs.Common;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات إنشاء عميل جديد (Create Customer DTO).
/// يستقبل بيانات العميل الشخصية، نوع التعامل التجاري، الرصيد الافتتاحي وسقف الدين، وقائمة أرقام الهواتف.
/// </summary>
public class CreateCustomerDto
{
    public bool IsActive { get; set; } = true;
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string? TaxNumber { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string? CommercialRegister { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Notes { get; set; }
    [System.ComponentModel.DataAnnotations.Range(0, 100)]
    [MoneyAmount]
    public decimal DiscountRate { get; set; }

    /// <summary>اسم العميل أو المؤسسة</summary>
    [Required(ErrorMessage = "اسم العميل مطلوب")]
    [MaxLength(100, ErrorMessage = "اسم العميل يجب أن لا يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>كود العميل الاختياري</summary>
    [MaxLength(50, ErrorMessage = "كود العميل يجب أن لا يتجاوز 50 حرف")]
    public string? Code { get; set; }

    /// <summary>عنوان البريد الإلكتروني للعميل</summary>
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [MaxLength(100, ErrorMessage = "البريد الإلكتروني يجب أن لا يتجاوز 100 حرف")]
    public string? Email { get; set; }

    /// <summary>العنوان الجغرافي أو مكان الإقامة</summary>
    [MaxLength(200, ErrorMessage = "العنوان يجب أن لا يتجاوز 200 حرف")]
    public string? Address { get; set; }

    /// <summary>نوع العميل (تجزئة، جملة، شركات)</summary>
    [EnumDataType(typeof(CustomerType))]
    public CustomerType Type { get; set; } = CustomerType.Retail;

    /// <summary>الرصيد المالي الافتتاحي للعميل</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الرصيد الافتتاحي يجب أن يكون أكبر من أو يساوي 0")]
    [MoneyAmount]
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>الحد الأقصى للديون والتعامل الآجل المسموح به للعميل</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سقف الدين يجب أن يكون أكبر من أو يساوي 0")]
    [MoneyAmount]
    public decimal CreditLimit { get; set; } = 0;

    /// <summary>قائمة أرقام هواتف العميل</summary>
    [NoNullItems]
    public List<CustomerPhoneDto> Phones { get; set; } = new();
}
