using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات إنشاء عميل جديد (Create Customer DTO).
/// يستقبل بيانات العميل الشخصية، نوع التعامل التجاري، الرصيد الافتتاحي وسقف الدين، وقائمة أرقام الهواتف.
/// </summary>
public class CreateCustomerDto
{
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
    public CustomerType Type { get; set; } = CustomerType.Retail;

    /// <summary>الرصيد المالي الافتتاحي للعميل</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الرصيد الافتتاحي يجب أن يكون أكبر من أو يساوي 0")]
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>الحد الأقصى للديون والتعامل الآجل المسموح به للعميل</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سقف الدين يجب أن يكون أكبر من أو يساوي 0")]
    public decimal CreditLimit { get; set; } = 0;

    /// <summary>قائمة أرقام هواتف العميل</summary>
    public List<CustomerPhoneDto> Phones { get; set; } = new();
}
