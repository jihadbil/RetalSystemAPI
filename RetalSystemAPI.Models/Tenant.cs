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
    /// يمثل بيانات المستأجر (المؤسسة / الشركة) في النظام، بما في ذلك بيانات الهوية والاتصال والإعدادات المالية.
    /// </summary>
    // كلاس كيان المستأجر الأساسي في الهيكلية متعددة المستأجرين
    public class Tenant : TenantBaseEntity
    {
        /// <summary>
        /// يمثل اسم المستأجر أو الشركة في النظام.
        /// </summary>
        [Required]
        // خاصية اسم المستأجر الإلزامي
        public required string Name { get; set; }

        /// <summary>
        /// يمثل وصف المستأجر في النظام وتوضيح طبيعة النشاط التجاري.
        /// </summary>
        // خاصية الوصف الاختياري للمستأجر
        public string? Description { get; set; }

        /// <summary>
        /// يمثل البريد الإلكتروني الرسمي للتواصل وإرسال الإشعارات.
        /// </summary>
        // خاصية البريد الإلكتروني للمستأجر
        public string? ContactEmail { get; set; }

        /// <summary>
        /// يمثل رقم الهاتف الأساسي للمستأجر.
        /// </summary>
        [Required]
        [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
        // خاصية رقم الهاتف الإلزامي وفق النمط الليبي
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// يمثل العنوان الجغرافي للمقر الرئيسي للمستأجر.
        /// </summary>
        // خاصية عنوان المقر الرئيسي للمستأجر
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// يمثل رابط أو مسار شعار المؤسسة (Logo) في النظام.
        /// </summary>
        // خاصية مسار شعار المؤسسة
        public string LogoUrl { get; set; } = string.Empty;

        /// <summary>
        /// يمثل حالة حساب المستأجر، حيث يشير إلى كونه نشطاً أو موقوفاً.
        /// </summary>
        // خاصية حالة نشاط حساب المستأجر
        public bool IsActive { get; set; } = true;

        // ── البيانات الرسمية والقانونية ────────────────────────────

        /// <summary>
        /// الاسم التجاري أو القانوني الرسمي للمنشأة في الأوراق الرسمية.
        /// </summary>
        // خاصية الاسم التجاري الرسمي
        public string? CommercialName { get; set; }

        /// <summary>
        /// الرقم الضريبي للمنشأة (Tax / VAT Number).
        /// </summary>
        // خاصية الرقم الضريبي للمؤسسة
        public string? TaxNumber { get; set; }

        /// <summary>
        /// رقم السجل التجاري الرسمي للمنشأة (CRN).
        /// </summary>
        // خاصية رقم السجل التجاري
        public string? CommercialRegistrationNumber { get; set; }

        /// <summary>
        /// نوع النشاط التجاري أو قطاع الأعمال (مثال: تجارة تجزئة، مواد غذائية، أجهزة).
        /// </summary>
        // خاصية نوع النشاط وقطاع الأعمال
        public string? BusinessType { get; set; }

        // ── بيانات الاتصال والعناوين المتقدمة ──────────────────────

        /// <summary>
        /// رقم هاتف إضافي أو رقم خدمة العملاء / واتساب المؤسسة.
        /// </summary>
        // خاصية رقم الهاتف الإضافي
        public string? AdditionalPhone { get; set; }

        /// <summary>
        /// الموقع الإلكتروني الرسمي للمؤسسة على الإنترنت.
        /// </summary>
        // خاصية رابط الموقع الإلكتروني
        public string? WebsiteUrl { get; set; }

        /// <summary>
        /// المدينة التي يقع بها المقر الرئيسي.
        /// </summary>
        // خاصية اسم المدينة
        public string? City { get; set; }

        /// <summary>
        /// الدولة التي تتبع لها المؤسسة.
        /// </summary>
        // خاصية اسم الدولة
        public string? Country { get; set; } = "ليبيا";

        /// <summary>
        /// الرمز البريدي أو صندوق البريد.
        /// </summary>
        // خاصية الرمز البريدي
        public string? PostalCode { get; set; }

        // ── الهوية البصرية وإعدادات المطبوعات والفواتير ───────────

        /// <summary>
        /// نص ترويسة الفاتورة الذي يظهر أعلى الفواتير المطبوعة.
        /// </summary>
        // خاصية ترويسة الفاتورة المطبوعة
        public string? InvoiceHeaderNote { get; set; }

        /// <summary>
        /// نص تذييل الفاتورة وشروط البيع والإرجاع والاستبدال.
        /// </summary>
        // خاصية تذييل وشروط الفاتورة
        public string? InvoiceFooterNote { get; set; }

        /// <summary>
        /// بيانات الحسابات المصرفية ورقم الآيبان لسداد الفواتير والتحويلات.
        /// </summary>
        // خاصية تفاصيل الحسابات البنكية للمطبوعات
        public string? BankDetails { get; set; }

        // ── الإعدادات والسياسات المالية الافتراضية ────────────────

        /// <summary>
        /// اسم العملة الافتراضية المعتمدة في المعاملات (دينار ليبي، دولار).
        /// </summary>
        // خاصية اسم العملة الافتراضية
        public string DefaultCurrency { get; set; } = "دينار ليبي";

        /// <summary>
        /// رمز العملة الافتراضي المختصر (د.ل، USD).
        /// </summary>
        // خاصية رمز العملة المختصر
        public string DefaultCurrencySymbol { get; set; } = "د.ل";

        /// <summary>
        /// نسبة الضريبة الافتراضية بالمائة % على المبيعات.
        /// </summary>
        // خاصية النسبة المئوية للضريبة الافتراضية
        public decimal DefaultTaxRate { get; set; } = 0m;

        /// <summary>
        /// يحدد ما إذا كانت أسعار البيع شاملة للضريبة بشكل افتراضي.
        /// </summary>
        // خاصية مؤشر شمول السعر للضريبة
        public bool IsTaxIncludedInPrices { get; set; } = false;

        /// <summary>
        /// المنطقة الزمنية الافتراضية للمؤسسة لضبط تواريخ العمليات.
        /// </summary>
        // خاصية المنطقة الزمنية للمنشأة
        public string Timezone { get; set; } = "Africa/Tripoli";
    }
}
