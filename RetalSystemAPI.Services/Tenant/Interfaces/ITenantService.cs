using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Tenant.Interfaces;

/// <summary>
/// واجهة خدمة المستأجرين لإدارة المستأجرين على مستوى النظام.
/// </summary>
public interface ITenantService
{
    Task<ServiceResult<TenantResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<TenantResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<PagedResult<TenantResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<ServiceResult<TenantResponseDto>> CreateAsync(CreateTenantDto dto, CancellationToken ct = default);
    Task<ServiceResult<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
