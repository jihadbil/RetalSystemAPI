using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.MAUI.Core.Http;
using RetalSystemAPI.Models.DTOs.Branch;
using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.MAUI.Services;

public interface IBranchApiService
{
    Task<ApiResponse<List<BranchResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<PagedResult<BranchResponseDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<BranchResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<BranchResponseDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default);
    Task<ApiResponse<BranchResponseDto>> UpdateAsync(Guid id, UpdateBranchDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<bool>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}

public class BranchApiService : IBranchApiService
{
    private readonly ApiClient _apiClient;

    public BranchApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<BranchResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<BranchResponseDto>>("api/branches", ct);
    }

    public Task<ApiResponse<PagedResult<BranchResponseDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<PagedResult<BranchResponseDto>>($"api/branches/paged?pageNumber={pageNumber}&pageSize={pageSize}", ct);
    }

    public Task<ApiResponse<BranchResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<BranchResponseDto>($"api/branches/{id}", ct);
    }

    public Task<ApiResponse<BranchResponseDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<BranchResponseDto>("api/branches", dto, ct);
    }

    public Task<ApiResponse<BranchResponseDto>> UpdateAsync(Guid id, UpdateBranchDto dto, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<BranchResponseDto>($"api/branches/{id}", dto, ct);
    }

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync<bool>($"api/branches/{id}", ct);
    }

    public Task<ApiResponse<bool>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<bool>($"api/branches/{id}/toggle-active", null, ct);
    }
}
