using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RetalSystemAPI.DataAccess.Services;

// The override belongs to a new DI scope; the caller's principal and scope are never changed.
public sealed class TenantExecutionContext
{
    public Guid? TenantId { get; internal set; }
}

public sealed class TenantExecutionScopeFactory(IServiceScopeFactory scopes, IHttpContextAccessor http)
    : ITenantExecutionScopeFactory
{
    public AsyncServiceScope CreateForProvisioning(Guid tenantId)
    {
        if (tenantId == Guid.Empty || http.HttpContext?.User is not { Identity.IsAuthenticated: true } user ||
            !user.IsInRole("SuperAdmin"))
            throw new UnauthorizedAccessException("تهيئة المستأجر تتطلب مسؤول المنصة.");

        var scope = scopes.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<TenantExecutionContext>().TenantId = tenantId;
        return scope;
    }
}
