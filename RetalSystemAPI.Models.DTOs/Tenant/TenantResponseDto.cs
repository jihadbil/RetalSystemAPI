using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

/// <summary>
/// ناقل بيانات استجابة تفاصيل المستأجر الشاملة (Tenant Response DTO).
/// يتضمن البيانات القانونية، المالية، إعدادات الفواتير، وموجز أعداد الفروع والمستودعات والمستخدمين.
/// </summary>
public class TenantResponseDto : BaseDto
{
    /// <summary>اسم المنشأة</summary>
    public string Name { get; set; } = null!;

    /// <summary>الوصف</summary>
    public string? Description { get; set; }

    /// <summary>البريد الإلكتروني الرسمي</summary>
    public string? ContactEmail { get; set; }

    /// <summary>رقم الهاتف</summary>
    public string PhoneNumber { get; set; } = null!;

    /// <summary>العنوان</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>رابط الشعار</summary>
    public string LogoUrl { get; set; } = string.Empty;

    /// <summary>حالة نشاط حساب المستأجر</summary>
    public bool IsActive { get; set; }

    // ── البيانات الرسمية والقانونية ────────────────────────────
    /// <summary>الاسم التجاري</summary>
    public string? CommercialName { get; set; }

    /// <summary>الرقم الضريبي</summary>
    public string? TaxNumber { get; set; }

    /// <summary>رقم السجل التجاري</summary>
    public string? CommercialRegistrationNumber { get; set; }

    /// <summary>نوع النشاط</summary>
    public string? BusinessType { get; set; }

    // ── بيانات الاتصال والعناوين المتقدمة ──────────────────────
    /// <summary>رقم هاتف إضافي</summary>
    public string? AdditionalPhone { get; set; }

    /// <summary>رابط الموقع الإلكتروني</summary>
    public string? WebsiteUrl { get; set; }

    /// <summary>المدينة</summary>
    public string? City { get; set; }

    /// <summary>الدولة</summary>
    public string? Country { get; set; }

    /// <summary>الرمز البريدي</summary>
    public string? PostalCode { get; set; }

    // ── الهوية البصرية وإعدادات المطبوعات والفواتير ───────────
    /// <summary>نص ترويسة الفاتورة</summary>
    public string? InvoiceHeaderNote { get; set; }

    /// <summary>نص تذييل وشروط الفاتورة</summary>
    public string? InvoiceFooterNote { get; set; }

    /// <summary>بيانات الحسابات البنكية</summary>
    public string? BankDetails { get; set; }

    // ── الإعدادات المالية والإقليمية ──────────────────────────
    /// <summary>العملة الافتراضية</summary>
    public string DefaultCurrency { get; set; } = "دينار ليبي";

    /// <summary>رمز العملة الافتراضي</summary>
    public string DefaultCurrencySymbol { get; set; } = "د.ل";

    /// <summary>نسبة الضريبة الافتراضية</summary>
    public decimal DefaultTaxRate { get; set; }

    /// <summary>هل الأسعار شاملة الضريبة</summary>
    public bool IsTaxIncludedInPrices { get; set; }

    /// <summary>المنطقة الزمنية</summary>
    public string Timezone { get; set; } = "Africa/Tripoli";

    // ── ملخص الإحصائيات (Counts Summary) ─────────────────────
    /// <summary>عدد الفروع التابعة للمستأجر</summary>
    public int BranchesCount { get; set; }

    /// <summary>عدد المستودعات والمخازن التابعة للمستأجر</summary>
    public int WarehousesCount { get; set; }

    /// <summary>عدد المستخدمين المسجلين في حساب المستأجر</summary>
    public int UsersCount { get; set; }
}
