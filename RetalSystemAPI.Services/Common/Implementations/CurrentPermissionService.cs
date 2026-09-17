using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Services.Common.Interfaces;

namespace RetalSystemAPI.Services.Common.Implementations;

/// <summary>
/// تنفيذ خدمة التحقق من صلاحيات المستخدم الحالي بالاعتماد على بيانات ومطالبات (Claims) رمز التوثيق JWT.
/// </summary>
public class CurrentPermissionService : ICurrentPermissionService
{
    // موفر سياق طلب HTTP للوصول لبيانات المستخدم الحالي والجلسة
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// تهيئة الخدمة مع حقن موفر سياق HTTP.
    /// </summary>
    /// <param name="httpContextAccessor">موفر سياق HTTP الحالي</param>
    public CurrentPermissionService(IHttpContextAccessor httpContextAccessor)
    {
        // حفظ المرجع المحقون للموفر
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public bool HasPermission(string permission)
    {
        // استخراج كائن المستخدم من سياق الطلب الحالي إن وجد
        var user = _httpContextAccessor.HttpContext?.User;

        // التحقق من وجود المستخدم وهويته وأنه مسجل دخول وموثق
        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
        {
            // إرجاع عدم وجود صلاحية إذا كان المستخدم غير موثق
            return false;
        }

        // التحقق مما إذا كان المستخدم يملك دور مدير النظام أو المدير العام
        if (user.IsInRole("Admin") || user.IsInRole("SuperAdmin"))
        {
            // مسؤولو النظام يملكون كافة الصلاحيات بصورة كاملة وتلقائية
            return true;
        }

        // فحص مطالبات المستخدم بحثاً عن كود الصلاحية المطلوب
        return user.Claims.Any(c =>
            // التأكد من أن نوع المطالبة يطابق نمط الصلاحيات المعتمد
            (string.Equals(c.Type, Permissions.ClaimType, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)) &&
            // مطابقة قيمة المطالبة مع رمز الصلاحية المحدد مع تجاهل حالة الأحرف
            string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase));
    }
}
