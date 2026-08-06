using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RetalSystemAPI.DataAccess.Services;

/// <summary>
/// خدمة لاستخراج UserId الخاص بالمستخدم الحالي من المطالبات (Claims) في HTTP Context.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?
        .User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
