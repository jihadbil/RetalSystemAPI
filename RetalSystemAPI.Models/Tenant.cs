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
    public class Tenant : BaseEntity
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
        [RegularExpression(@"^(09\d{8}|\+2189\d{8})$", ErrorMessage = "رقم الهاتف غير صحيح.")]
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
        /// يمثل حالة المستأجر في النظام، حيث يشير إلى ما إذا كان المستأجر نشطًا أم لا. يمكن استخدام هذا الحقل لتحديد ما إذا كان المستأجر يمكنه الوصول إلى النظام واستخدامه.
        /// </summary>
        public bool IsActive { get; set; } = true;



    }
}
