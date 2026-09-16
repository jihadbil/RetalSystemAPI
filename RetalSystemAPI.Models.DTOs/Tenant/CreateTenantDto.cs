using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

public class CreateTenantDto
{
    [Required(ErrorMessage = "اسم المنشأة مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم المنشأة يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    [MaxLength(1000, ErrorMessage = "الوصف يجب أن لا يتجاوز 1000 حرف")]
    public string? Description { get; set; }

    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [MaxLength(100, ErrorMessage = "البريد الإلكتروني يجب أن لا يتجاوز 100 حرف")]
    public string? ContactEmail { get; set; }

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "العنوان يجب أن لا يتجاوز 500 حرف")]
    public string Address { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;

    [MaxLength(250, ErrorMessage = "الاسم التجاري يجب أن لا يتجاوز 250 حرف")]
    public string? CommercialName { get; set; }

    [MaxLength(50, ErrorMessage = "الرقم الضريبي يجب أن لا يتجاوز 50 حرف")]
    public string? TaxNumber { get; set; }

    [MaxLength(50, ErrorMessage = "رقم السجل التجاري يجب أن لا يتجاوز 50 حرف")]
    public string? CommercialRegistrationNumber { get; set; }

    [MaxLength(100, ErrorMessage = "نوع النشاط يجب أن لا يتجاوز 100 حرف")]
    public string? BusinessType { get; set; }

    [MaxLength(30, ErrorMessage = "رقم الهاتف الإضافي يجب أن لا يتجاوز 30 حرف")]
    public string? AdditionalPhone { get; set; }

    [MaxLength(250, ErrorMessage = "رابط الموقع يجب أن لا يتجاوز 250 حرف")]
    public string? WebsiteUrl { get; set; }

    [MaxLength(100, ErrorMessage = "المدينة يجب أن لا يتجاوز 100 حرف")]
    public string? City { get; set; }

    [MaxLength(100, ErrorMessage = "الدولة يجب أن لا يتجاوز 100 حرف")]
    public string? Country { get; set; } = "ليبيا";

    [MaxLength(20, ErrorMessage = "الرمز البريدي يجب أن لا يتجاوز 20 حرف")]
    public string? PostalCode { get; set; }

    [MaxLength(1000, ErrorMessage = "نص الترويسة يجب أن لا يتجاوز 1000 حرف")]
    public string? InvoiceHeaderNote { get; set; }

    [MaxLength(2000, ErrorMessage = "نص التذييل وشروط الإرجاع يجب أن لا يتجاوز 2000 حرف")]
    public string? InvoiceFooterNote { get; set; }

    [MaxLength(2000, ErrorMessage = "بيانات الحسابات البنكية يجب أن لا يتجاوز 2000 حرف")]
    public string? BankDetails { get; set; }

    [MaxLength(50, ErrorMessage = "اسم العملة يجب أن لا يتجاوز 50 حرف")]
    public string DefaultCurrency { get; set; } = "دينار ليبي";

    [MaxLength(20, ErrorMessage = "رمز العملة يجب أن لا يتجاوز 20 حرف")]
    public string DefaultCurrencySymbol { get; set; } = "د.ل";

    public decimal DefaultTaxRate { get; set; } = 0m;

    public bool IsTaxIncludedInPrices { get; set; } = false;

    [MaxLength(100, ErrorMessage = "المنطقة الزمنية يجب أن لا تتجاوز 100 حرف")]
    public string Timezone { get; set; } = "Africa/Tripoli";
}
