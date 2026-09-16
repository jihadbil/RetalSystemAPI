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
/// سمة مرشح الصلاحيات للتحقق من امتلاك المستخدم للإذن المحدد لتنفيذ العملية أو دخول الشاشة.
/// في حال عدم توفر الصلاحية يتم إرجاع 403 Forbidden مع رسالة تفصيلية بالعربية.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public HasPermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // 1. التحقق من تسجيل الدخول
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new ObjectResult(new ApiResponse
            {
                Success = false,
                Message = "يجب تسجيل الدخول أولاً للوصول إلى هذا المورد.",
                ErrorCode = "UNAUTHORIZED"
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return Task.CompletedTask;
        }

        // 2. السماح التلقائي لمسؤولي النظام (Admin / SuperAdmin)
        if (user.IsInRole("Admin") || user.IsInRole("SuperAdmin"))
        {
            return Task.CompletedTask;
        }

        // 3. التحقق من وجود إذن الصلاحية المطلوب في الـ Claims
        bool hasPermission = user.Claims.Any(c =>
            (string.Equals(c.Type, Permissions.ClaimType, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)) &&
            string.Equals(c.Value, _permission, StringComparison.OrdinalIgnoreCase));

        if (!hasPermission)
        {
            context.Result = new ObjectResult(new ApiResponse
            {
                Success = false,
                Message = "ليس لديك الصلاحية المطلوبة لتنفيذ هذا الإجراء.",
                ErrorCode = "FORBIDDEN"
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return Task.CompletedTask;
    }
}
