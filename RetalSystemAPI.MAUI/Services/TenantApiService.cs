using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.MAUI.Core.Http;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.DTOs.Tenant;

namespace RetalSystemAPI.MAUI.Services;

public interface ITenantApiService
{
    Task<ApiResponse<TenantResponseDto>> GetMyProfileAsync(CancellationToken ct = default);
    Task<ApiResponse<TenantResponseDto>> UpdateMyProfileAsync(UpdateTenantDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<TenantResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<PagedResult<TenantResponseDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<TenantResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<TenantResponseDto>> CreateAsync(CreateTenantDto dto, CancellationToken ct = default);
    Task<ApiResponse<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<bool>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}

public class TenantApiService : ITenantApiService
{
    private readonly ApiClient _apiClient;

    public TenantApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<TenantResponseDto>> GetMyProfileAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<TenantResponseDto>("api/tenants/me", ct);
    }

    public Task<ApiResponse<TenantResponseDto>> UpdateMyProfileAsync(UpdateTenantDto dto, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<TenantResponseDto>("api/tenants/me", dto, ct);
    }

    public Task<ApiResponse<List<TenantResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<TenantResponseDto>>("api/tenants", ct);
    }

    public Task<ApiResponse<PagedResult<TenantResponseDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<PagedResult<TenantResponseDto>>($"api/tenants/paged?pageNumber={pageNumber}&pageSize={pageSize}", ct);
    }

    public Task<ApiResponse<TenantResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<TenantResponseDto>($"api/tenants/{id}", ct);
    }

    public Task<ApiResponse<TenantResponseDto>> CreateAsync(CreateTenantDto dto, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<TenantResponseDto>("api/tenants", dto, ct);
    }

    public Task<ApiResponse<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto dto, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<TenantResponseDto>($"api/tenants/{id}", dto, ct);
    }

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync<bool>($"api/tenants/{id}", ct);
    }

    public Task<ApiResponse<bool>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<bool>($"api/tenants/{id}/toggle-active", null, ct);
    }
}
