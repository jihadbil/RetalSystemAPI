using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

/// <summary>
/// ناقل بيانات تعديل بيانات مستأجر قائم (Update Tenant DTO).
/// </summary>
public class UpdateTenantDto
{
    /// <summary>المعرف الفريد للمستأجر المطلوب تعديله</summary>
    [Required(ErrorMessage = "معرف المستأجر مطلوب")]
    public Guid Id { get; set; }

    /// <summary>اسم المنشأة الجديد</summary>
    [Required(ErrorMessage = "اسم المنشأة مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم المنشأة يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>الوصف الجديد</summary>
    [MaxLength(1000, ErrorMessage = "الوصف يجب أن لا يتجاوز 1000 حرف")]
    public string? Description { get; set; }

    /// <summary>البريد الإلكتروني للتواصل</summary>
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [MaxLength(100, ErrorMessage = "البريد الإلكتروني يجب أن لا يتجاوز 100 حرف")]
    public string? ContactEmail { get; set; }

    /// <summary>رقم الهاتف الأساسي</summary>
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;

    /// <summary>العنوان الجغرافي</summary>
    [MaxLength(500, ErrorMessage = "العنوان يجب أن لا يتجاوز 500 حرف")]
    public string Address { get; set; } = string.Empty;

    /// <summary>رابط أو مسار الشعار الجديد</summary>
    public string LogoUrl { get; set; } = string.Empty;

    /// <summary>حالة نشاط الحساب</summary>
    public bool IsActive { get; set; } = true;

    // ── البيانات الرسمية والقانونية ────────────────────────────

    /// <summary>الاسم التجاري</summary>
    [MaxLength(250, ErrorMessage = "الاسم التجاري يجب أن لا يتجاوز 250 حرف")]
    public string? CommercialName { get; set; }

    /// <summary>الرقم الضريبي</summary>
    [MaxLength(50, ErrorMessage = "الرقم الضريبي يجب أن لا يتجاوز 50 حرف")]
    public string? TaxNumber { get; set; }

    /// <summary>رقم السجل التجاري</summary>
    [MaxLength(50, ErrorMessage = "رقم السجل التجاري يجب أن لا يتجاوز 50 حرف")]
    public string? CommercialRegistrationNumber { get; set; }

    /// <summary>نوع النشاط التجاري</summary>
    [MaxLength(100, ErrorMessage = "نوع النشاط يجب أن لا يتجاوز 100 حرف")]
    public string? BusinessType { get; set; }

    // ── بيانات الاتصال والعناوين المتقدمة ──────────────────────

    /// <summary>رقم الهاتف الإضافي</summary>
    [MaxLength(30, ErrorMessage = "رقم الهاتف الإضافي يجب أن لا يتجاوز 30 حرف")]
    public string? AdditionalPhone { get; set; }

    /// <summary>رابط الموقع الإلكتروني</summary>
    [MaxLength(250, ErrorMessage = "رابط الموقع يجب أن لا يتجاوز 250 حرف")]
    public string? WebsiteUrl { get; set; }

    /// <summary>المدينة</summary>
    [MaxLength(100, ErrorMessage = "المدينة يجب أن لا يتجاوز 100 حرف")]
    public string? City { get; set; }

    /// <summary>الدولة</summary>
    [MaxLength(100, ErrorMessage = "الدولة يجب أن لا يتجاوز 100 حرف")]
    public string? Country { get; set; }

    /// <summary>الرمز البريدي</summary>
    [MaxLength(20, ErrorMessage = "الرمز البريدي يجب أن لا يتجاوز 20 حرف")]
    public string? PostalCode { get; set; }

    // ── الهوية البصرية وإعدادات المطبوعات والفواتير ───────────

    /// <summary>نص ترويسة الفاتورة</summary>
    [MaxLength(1000, ErrorMessage = "نص الترويسة يجب أن لا يتجاوز 1000 حرف")]
    public string? InvoiceHeaderNote { get; set; }

    /// <summary>نص تذييل وشروط الفاتورة</summary>
    [MaxLength(2000, ErrorMessage = "نص التذييل وشروط الإرجاع يجب أن لا يتجاوز 2000 حرف")]
    public string? InvoiceFooterNote { get; set; }

    /// <summary>بيانات الحسابات البنكية</summary>
    [MaxLength(2000, ErrorMessage = "بيانات الحسابات البنكية يجب أن لا يتجاوز 2000 حرف")]
    public string? BankDetails { get; set; }

    // ── الإعدادات المالية والإقليمية ──────────────────────────

    /// <summary>اسم العملة الافتراضية</summary>
    [MaxLength(50, ErrorMessage = "اسم العملة يجب أن لا يتجاوز 50 حرف")]
    public string DefaultCurrency { get; set; } = "دينار ليبي";

    /// <summary>رمز العملة الافتراضي</summary>
    [MaxLength(20, ErrorMessage = "رمز العملة يجب أن لا يتجاوز 20 حرف")]
    public string DefaultCurrencySymbol { get; set; } = "د.ل";

    /// <summary>نسبة الضريبة</summary>
    public decimal DefaultTaxRate { get; set; } = 0m;

    /// <summary>هل الأسعار شاملة الضريبة</summary>
    public bool IsTaxIncludedInPrices { get; set; } = false;

    /// <summary>المنطقة الزمنية</summary>
    [MaxLength(100, ErrorMessage = "المنطقة الزمنية يجب أن لا تتجاوز 100 حرف")]
    public string Timezone { get; set; } = "Africa/Tripoli";
}
