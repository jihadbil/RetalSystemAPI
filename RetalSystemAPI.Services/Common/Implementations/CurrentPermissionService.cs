using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Services.Common.Interfaces;

namespace RetalSystemAPI.Services.Common.Implementations;

/// <summary>
/// تنفيذ التحقق من صلاحيات المستخدم الحالي من مطالبات JWT — نفس منطق فلتر HasPermissionAttribute.
/// </summary>
public class CurrentPermissionService : ICurrentPermissionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// تهيئة الخدمة مع حقن IHttpContextAccessor.
    /// </summary>
    public CurrentPermissionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public bool HasPermission(string permission)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
        {
            return false;
        }

        // مسؤولو النظام يملكون كافة الصلاحيات تلقائياً
        if (user.IsInRole("Admin") || user.IsInRole("SuperAdmin"))
        {
            return true;
        }

        return user.Claims.Any(c =>
            (string.Equals(c.Type, Permissions.ClaimType, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)) &&
            string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase));
    }
}
