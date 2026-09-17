using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Responses;

namespace RetalSystemAPI.Filters;

/// <summary>
/// سمة مرشح الصلاحيات المخصصة (Authorization Filter Attribute) للتحقق الدقيق من امتلاك المستخدم الحالي
/// للصلاحية المحددة قبل السماح له بالوصول إلى متحكم أو نقطة نهاية محددة (Endpoint).
/// في حال عدم توفر الصلاحية يتم إرجاع كود الاستجابة 403 Forbidden مع رسالة تفصيلية موحدة.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// اسم الصلاحية المطلوب فحصها وتوافرها لدى المستخدم.
    /// </summary>
    private readonly string _permission;

    /// <summary>
    /// تهيئة نسخة جديدة من السمة وتمرير الصلاحية المطلوب التحقق منها.
    /// </summary>
    /// <param name="permission">اسم الصلاحية المطلوب التأكد من وجودها (مثل: Products.View, Sales.Create).</param>
    public HasPermissionAttribute(string permission)
    {
        // إسناد اسم الصلاحية المطلوبة إلى الحقل الخاص
        _permission = permission;
    }

    /// <summary>
    /// تنفيذ فحص الأمان والصلاحيات بشكل غير متزامن عند تدفق طلب HTTP عبر خط الأنابيب (Pipeline).
    /// </summary>
    /// <param name="context">سياق مرشح التحقق من الصلاحيات الذي يحتوي على بيانات الطلب والمستخدم الحالي.</param>
    /// <returns>مهمة تمثل العملية غير المتزامنة للتحقق من الأذونات.</returns>
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // استخراج كائن المستخدم الرئيسي الحامل للهوية والمطالبات (ClaimsPrincipal) من سياق الطلب
        var user = context.HttpContext.User;

        // 1. التحقق من وجود هوية المستخدم وتوثيق تسجيل دخوله مسبقاً
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            // إعداد كائن استجابة يفيد بعدم التوثيق وتعيين كود الحالة 401 Unauthorized
            context.Result = new ObjectResult(new ApiResponse
            {
                // ضبط مؤشر النجاح إلى خطأ
                Success = false,
                // إرجاع رسالة خطأ توضح وجوب تسجيل الدخول أولاً
                Message = "يجب تسجيل الدخول أولاً للوصول إلى هذا المورد.",
                // تحديد كود الخطأ الدال على غياب المصادقة
                ErrorCode = "UNAUTHORIZED"
            })
            {
                // تعيين كود الـ HTTP إلى 401
                StatusCode = StatusCodes.Status401Unauthorized
            };
            // إنهاء تنفيذ المرشح وإرجاع مهمة مكتملة لإيقاف الطلب فوراً
            return Task.CompletedTask;
        }

        // 2. السماح التلقائي والمباشر لمسؤولي النظام الحاملين لأدوار Admin أو SuperAdmin
        if (user.IsInRole("Admin") || user.IsInRole("SuperAdmin"))
        {
            // تخطي فحص الصلاحيات الفردية والمتابعة بنجاح لمسؤولي النظام
            return Task.CompletedTask;
        }

        // 3. فحص ومطابقة وجود الصلاحية المطلوبة ضمن قائمة المطالبات (Claims) الممنوحة للمستخدم
        bool hasPermission = user.Claims.Any(c =>
            // التحقق من نوع المطالبة إما بالمسمى المخصص أو المسمى العام permission
            (string.Equals(c.Type, Permissions.ClaimType, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)) &&
            // التحقق من تطابق قيمة المطالبة مع الصلاحية المستهدفة بدون حساسية لحالة الأحرف
            string.Equals(c.Value, _permission, StringComparison.OrdinalIgnoreCase));

        // التحقق مما إذا كان المستخدم يفتقر إلى الصلاحية المطلوبة
        if (!hasPermission)
        {
            // بناء استجابة رفض الوصول مع تعيين كود الحالة 403 Forbidden
            context.Result = new ObjectResult(new ApiResponse
            {
                // ضبط مؤشر النجاح إلى خطأ
                Success = false,
                // إرجاع رسالة خطأ توضح حظر الوصول لعدم كفاية الصلاحيات
                Message = "ليس لديك الصلاحية المطلوبة لتنفيذ هذا الإجراء.",
                // تحديد كود الخطأ المنطقي للرفض
                ErrorCode = "FORBIDDEN"
            })
            {
                // تعيين كود الـ HTTP إلى 403
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        // إرجاع مهمة مكتملة لمتابعة مسار معالجة الطلب
        return Task.CompletedTask;
    }
}
