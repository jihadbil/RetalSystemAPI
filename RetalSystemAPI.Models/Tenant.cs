using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Branchs;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models
{
    /// <summary>
    /// يمتل بيانات المستأجر في النظام، بما في ذلك معلومات الاتصال والتفاصيل الأخرى ذات الصلة.
    /// </summary>
    public class Tenant : TenantBaseEntity
    {
        /// <summary>
        /// يمثل اسم المستأجر في النظام.
        /// </summary>
        /// 
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// يمثل وصف المستأجر في النظام، ويمكن استخدامه لتوضيح طبيعة النشاط أو الخدمات المقدمة من قبل المستأجر.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// يمثل البريد الإلكتروني للمستأجر في النظام، ويمكن استخدامه للتواصل مع المستأجر أو لإرسال الإشعارات والتنبيهات المتعلقة بالنظام.
        /// </summary>
        public string? ContactEmail { get; set; }

        /// <summary>
        /// يمثل رقم الهاتف للمستأجر في النظام.
        /// </summary>
        /// 
        [Required]
        [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// يمثل عنوان المستأجر في النظام.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// يمثل رابط شعار المستأجر في النظام.
        /// </summary>
        public string LogoUrl { get; set; } = string.Empty;

        /// <summary>
        /// يمثل حالة المستأجر في النظام، حيث يشير إلى ما إذا كان المستأجر نشطًا أم لا.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ── البيانات الرسمية والقانونية ────────────────────────────

        /// <summary>الاسم التجاري / القانوني الرسمي للمنشأة</summary>
        public string? CommercialName { get; set; }

        /// <summary>الرقم الضريبي للمنشأة (Tax / VAT Number)</summary>
        public string? TaxNumber { get; set; }

        /// <summary>رقم السجل التجاري (Commercial Registration Number - CRN)</summary>
        public string? CommercialRegistrationNumber { get; set; }

        /// <summary>نوع النشاط التجاري أو قطاع الأعمال (مثال: تجارة تجزئة، مواد غذائية، أجهزة، إلخ)</summary>
        public string? BusinessType { get; set; }

        // ── بيانات الاتصال والعناوين المتقدمة ──────────────────────

        /// <summary>رقم هاتف إضافي أو رقم خدمة العملاء / واتساب المؤسسة</summary>
        public string? AdditionalPhone { get; set; }

        /// <summary>الموقع الإلكتروني الرسمي للمؤسسة</summary>
        public string? WebsiteUrl { get; set; }

        /// <summary>المدينة</summary>
        public string? City { get; set; }

        /// <summary>الدولة</summary>
        public string? Country { get; set; } = "ليبيا";

        /// <summary>الرمز البريدي أو صندوق البريد</summary>
        public string? PostalCode { get; set; }

        // ── الهوية البصرية وإعدادات المطبوعات والفواتير ───────────

        /// <summary>نص ترويسة الفاتورة (يظهر أعلى الفواتير المطبوعة)</summary>
        public string? InvoiceHeaderNote { get; set; }

        /// <summary>نص تذييل الفاتورة وشروط البيع والإرجاع والاستبدال</summary>
        public string? InvoiceFooterNote { get; set; }

        /// <summary>بيانات الحسابات البنكية ورقم الآيبان لسداد الفواتير</summary>
        public string? BankDetails { get; set; }

        // ── الإعدادات والسياسات المالية الافتراضية ────────────────

        /// <summary>اسم العملة الافتراضية (مثال: دينار ليبي، ريال سعودي، دولار)</summary>
        public string DefaultCurrency { get; set; } = "دينار ليبي";

        /// <summary>رمز العملة الافتراضي (مثال: د.ل، ر.س، USD)</summary>
        public string DefaultCurrencySymbol { get; set; } = "د.ل";

        /// <summary>نسبة الضريبة الافتراضية بالمائة % (مثال: 0 أو 15)</summary>
        public decimal DefaultTaxRate { get; set; } = 0m;

        /// <summary>هل أسعار البيع شاملة للضريبة بشكل افتراضي</summary>
        public bool IsTaxIncludedInPrices { get; set; } = false;

        /// <summary>المنطقة الزمنية الافتراضية للمؤسسة</summary>
        public string Timezone { get; set; } = "Africa/Tripoli";
    }
}
