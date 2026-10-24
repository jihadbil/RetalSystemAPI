using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Tenant.Interfaces;

public interface ITenantProvisioningService
{
    Task<ServiceResult<ProvisionTenantResponseDto>> ProvisionAsync(ProvisionTenantDto dto, CancellationToken ct = default);
}
