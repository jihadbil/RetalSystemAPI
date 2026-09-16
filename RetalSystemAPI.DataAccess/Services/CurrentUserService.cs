using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RetalSystemAPI.DataAccess.Services;

/// <summary>
/// خدمة استخراج معرف المستخدم الحالي (UserId) من المطالبات (Claims) في سياق HTTP Context.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// تهيئة الخدمة مع حقن IHttpContextAccessor.
    /// </summary>
    /// <param name="httpContextAccessor">مزود الوصول إلى سياق الـ HTTP</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// جلب معرف المستخدم المستخرج من ClaimTypes.NameIdentifier.
    /// </summary>
    public string? UserId => _httpContextAccessor.HttpContext?
        .User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
