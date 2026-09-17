using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

/// <summary>
/// ناقل بيانات إنشاء مستأجر جديد (Create Tenant DTO).
/// يتضمن بيانات المنشأة القانونية، بيانات الاتصال، الهوية البصرية، وإعدادات الفواتير والعملة والضرائب.
/// </summary>
public class CreateTenantDto
{
    /// <summary>اسم المنشأة أو الشركة</summary>
    [Required(ErrorMessage = "اسم المنشأة مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم المنشأة يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>وصف المنشأة أو نبذة عن النشاط</summary>
    [MaxLength(1000, ErrorMessage = "الوصف يجب أن لا يتجاوز 1000 حرف")]
    public string? Description { get; set; }

    /// <summary>البريد الإلكتروني الرسمي للتواصل</summary>
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [MaxLength(100, ErrorMessage = "البريد الإلكتروني يجب أن لا يتجاوز 100 حرف")]
    public string? ContactEmail { get; set; }

    /// <summary>رقم الهاتف الأساسي للمنشأة</summary>
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;

    /// <summary>العنوان الجغرافي للمقر الرئيسي</summary>
    [MaxLength(500, ErrorMessage = "العنوان يجب أن لا يتجاوز 500 حرف")]
    public string Address { get; set; } = string.Empty;

    /// <summary>رابط أو مسار شعار المنشأة (Logo)</summary>
    public string LogoUrl { get; set; } = string.Empty;

    /// <summary>الاسم التجاري الرسمي المسجل</summary>
    [MaxLength(250, ErrorMessage = "الاسم التجاري يجب أن لا يتجاوز 250 حرف")]
    public string? CommercialName { get; set; }

    /// <summary>الرقم الضريبي الموحد للمنشأة</summary>
    [MaxLength(50, ErrorMessage = "الرقم الضريبي يجب أن لا يتجاوز 50 حرف")]
    public string? TaxNumber { get; set; }

    /// <summary>رقم السجل التجاري</summary>
    [MaxLength(50, ErrorMessage = "رقم السجل التجاري يجب أن لا يتجاوز 50 حرف")]
    public string? CommercialRegistrationNumber { get; set; }

    /// <summary>نوع النشاط التجاري</summary>
    [MaxLength(100, ErrorMessage = "نوع النشاط يجب أن لا يتجاوز 100 حرف")]
    public string? BusinessType { get; set; }

    /// <summary>رقم هاتف إضافي</summary>
    [MaxLength(30, ErrorMessage = "رقم الهاتف الإضافي يجب أن لا يتجاوز 30 حرف")]
    public string? AdditionalPhone { get; set; }

    /// <summary>رابط الموقع الإلكتروني</summary>
    [MaxLength(250, ErrorMessage = "رابط الموقع يجب أن لا يتجاوز 250 حرف")]
    public string? WebsiteUrl { get; set; }

    /// <summary>المدينة</summary>
    [MaxLength(100, ErrorMessage = "المدينة يجب أن لا يتجاوز 100 حرف")]
    public string? City { get; set; }

    /// <summary>الدولة (افتراضي: ليبيا)</summary>
    [MaxLength(100, ErrorMessage = "الدولة يجب أن لا يتجاوز 100 حرف")]
    public string? Country { get; set; } = "ليبيا";

    /// <summary>الرمز البريدي</summary>
    [MaxLength(20, ErrorMessage = "الرمز البريدي يجب أن لا يتجاوز 20 حرف")]
    public string? PostalCode { get; set; }

    /// <summary>نص الترويسة المطبوع أعلى الفواتير</summary>
    [MaxLength(1000, ErrorMessage = "نص الترويسة يجب أن لا يتجاوز 1000 حرف")]
    public string? InvoiceHeaderNote { get; set; }

    /// <summary>نص التذييل وشروط الإرجاع المطبوعة أسفل الفواتير</summary>
    [MaxLength(2000, ErrorMessage = "نص التذييل وشروط الإرجاع يجب أن لا يتجاوز 2000 حرف")]
    public string? InvoiceFooterNote { get; set; }

    /// <summary>بيانات الحسابات البنكية الخاصة بالمستأجر</summary>
    [MaxLength(2000, ErrorMessage = "بيانات الحسابات البنكية يجب أن لا يتجاوز 2000 حرف")]
    public string? BankDetails { get; set; }

    /// <summary>اسم العملة الافتراضية</summary>
    [MaxLength(50, ErrorMessage = "اسم العملة يجب أن لا يتجاوز 50 حرف")]
    public string DefaultCurrency { get; set; } = "دينار ليبي";

    /// <summary>رمز العملة الافتراضي</summary>
    [MaxLength(20, ErrorMessage = "رمز العملة يجب أن لا يتجاوز 20 حرف")]
    public string DefaultCurrencySymbol { get; set; } = "د.ل";

    /// <summary>نسبة الضريبة الافتراضية المطبقة</summary>
    public decimal DefaultTaxRate { get; set; } = 0m;

    /// <summary>هل الأسعار المعروضة شاملة الضريبة</summary>
    public bool IsTaxIncludedInPrices { get; set; } = false;

    /// <summary>المنطقة الزمنية المعتمدة للعمليات</summary>
    [MaxLength(100, ErrorMessage = "المنطقة الزمنية يجب أن لا تتجاوز 100 حرف")]
    public string Timezone { get; set; } = "Africa/Tripoli";
}
