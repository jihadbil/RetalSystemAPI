using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace RetalSystemAPI.DataAccess.Services;

/// <summary>
/// خدمة لاستخراج TenantId الخاص بالمستأجر الحالي من المطالبات (Claims) في HTTP Context.
/// </summary>
public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return Guid.Empty;

            var tenantClaim = user.FindFirst("TenantId")?.Value
                              ?? user.FindFirst("tenantid")?.Value
                              ?? user.FindFirst("tenant_id")?.Value;

            if (Guid.TryParse(tenantClaim, out var tenantId))
            {
                return tenantId;
            }

            return Guid.Empty;
        }
    }
}
