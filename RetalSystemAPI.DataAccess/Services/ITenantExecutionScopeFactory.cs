using Microsoft.Extensions.DependencyInjection;

namespace RetalSystemAPI.DataAccess.Services;

public interface ITenantExecutionScopeFactory
{
    AsyncServiceScope CreateForProvisioning(Guid tenantId);
}
