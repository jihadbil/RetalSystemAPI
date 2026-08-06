using System;
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
            var tenantClaim = _httpContextAccessor.HttpContext?
                .User?.FindFirst("TenantId")?.Value;

            if (Guid.TryParse(tenantClaim, out var tenantId))
            {
                return tenantId;
            }

            return Guid.Empty;
        }
    }
}
