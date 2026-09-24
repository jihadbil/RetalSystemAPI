using Microsoft.AspNetCore.Identity;
using RetalSystemAPI.Models.Branchs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetalSystemAPI.Models
{
    /// <summary>
    /// يمثل مستخدم النظام في إطار إدارة الهوية والمصادقة والصلاحيات (ASP.NET Core Identity).
    /// يرتبط كل مستخدم بمستأجر محدد وفرع عمل رئيسي.
    /// </summary>
    // كلاس المستخدم المشتق من كائن IdentityUser الأساسي
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// معرف المستأجر الذي ينتمي إليه المستخدم لضمان عزل البيانات بين المستأجرين.
        /// </summary>
        // المفتاح الأجنبي لمعرف المستأجر
        public Guid TenantId { get; set; }

        /// <summary>
        /// كائن المستأجر المرتبط بالمستخدم (خاصية التنقل).
        /// </summary>
        // خاصية الملاحة لبيانات المستأجر
        public Tenant? Tenant { get; set; }

        /// <summary>
        /// معرف الفرع الذي يعمل به المستخدم أو تم تعيينه إليه افتراضياً.
        /// </summary>
        // المفتاح الأجنبي لمعرف الفرع التابع له المستخدم
        public Guid BranchId { get; set; } 

        /// <summary>
        /// كائن الفرع المرتبط بالمستخدم (خاصية التنقل).
        /// </summary>
        // خاصية الملاحة لبيانات الفرع
        public Branch? Branch { get; set; } 
    }
}
