using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Tenant;

namespace RetalSystemAPI.Desktop.Services;

public interface ITenantApiService
{
    Task<ApiResponse<TenantDto>> GetMyProfileAsync(CancellationToken ct = default);
    Task<ApiResponse<TenantDto>> UpdateMyProfileAsync(UpdateTenantRequest request, CancellationToken ct = default);
    Task<ApiResponse<List<TenantDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<PagedResult<TenantDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<TenantDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);
    Task<ApiResponse<TenantDto>> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<TenantDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default);
}

public class TenantApiService : ITenantApiService
{
    private readonly ApiClient _apiClient;

    public TenantApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<TenantDto>> GetMyProfileAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<TenantDto>("tenants/me", ct);
    }

    public Task<ApiResponse<TenantDto>> UpdateMyProfileAsync(UpdateTenantRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<TenantDto>("tenants/me", request, ct);
    }

    public Task<ApiResponse<List<TenantDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<TenantDto>>("tenants", ct);
    }

    public Task<ApiResponse<PagedResult<TenantDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<PagedResult<TenantDto>>($"tenants/paged?pageNumber={pageNumber}&pageSize={pageSize}", ct);
    }

    public Task<ApiResponse<TenantDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<TenantDto>($"tenants/{id}", ct);
    }

    public Task<ApiResponse<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<TenantDto>("tenants", request, ct);
    }

    public Task<ApiResponse<TenantDto>> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<TenantDto>($"tenants/{id}", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"tenants/{id}", ct);
    }

    public Task<ApiResponse<TenantDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<TenantDto>($"tenants/{id}/toggle-active", ct);
    }
}
