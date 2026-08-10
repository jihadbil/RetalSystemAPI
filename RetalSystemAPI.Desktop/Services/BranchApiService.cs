using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Common;

namespace RetalSystemAPI.Desktop.Services;

public interface IBranchApiService
{
    Task<ApiResponse<List<BranchDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<PagedResult<BranchDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<BranchDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<BranchDto>> CreateAsync(CreateBranchRequest request, CancellationToken ct = default);
    Task<ApiResponse<BranchDto>> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<BranchDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default);
}

public class BranchApiService : IBranchApiService
{
    private readonly ApiClient _apiClient;

    public BranchApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<BranchDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<BranchDto>>("branches", ct);
    }

    public Task<ApiResponse<PagedResult<BranchDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<PagedResult<BranchDto>>($"branches/paged?pageNumber={pageNumber}&pageSize={pageSize}", ct);
    }

    public Task<ApiResponse<BranchDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<BranchDto>($"branches/{id}", ct);
    }

    public Task<ApiResponse<BranchDto>> CreateAsync(CreateBranchRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<BranchDto>("branches", request, ct);
    }

    public Task<ApiResponse<BranchDto>> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<BranchDto>($"branches/{id}", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"branches/{id}", ct);
    }

    public Task<ApiResponse<BranchDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<BranchDto>($"branches/{id}/toggle-active", ct);
    }
}
