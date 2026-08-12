using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Warehouses;

namespace RetalSystemAPI.Desktop.Services.Warehouses;

public interface IWarehouseApiService
{
    Task<ApiResponse<List<WarehouseSummaryDto>>> GetAllAsync(Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default);
    Task<ApiResponse<PagedResult<WarehouseSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default);
    Task<ApiResponse<WarehouseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<WarehouseDto>> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default);
    Task<ApiResponse<WarehouseDto>> UpdateAsync(Guid id, UpdateWarehouseRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse> ToggleActiveAsync(Guid id, CancellationToken ct = default);
}

public class WarehouseApiService : IWarehouseApiService
{
    private readonly ApiClient _apiClient;

    public WarehouseApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<WarehouseSummaryDto>>> GetAllAsync(Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default)
    {
        var url = "warehouses";
        var queryParams = new List<string>();
        if (branchId.HasValue) queryParams.Add($"branchId={branchId.Value}");
        if (type.HasValue) queryParams.Add($"type={(int)type.Value}");
        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

        return _apiClient.GetAsync<List<WarehouseSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PagedResult<WarehouseSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default)
    {
        var url = $"warehouses/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (branchId.HasValue) url += $"&branchId={branchId.Value}";
        if (type.HasValue) url += $"&type={(int)type.Value}";

        return _apiClient.GetAsync<PagedResult<WarehouseSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<WarehouseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<WarehouseDto>($"warehouses/{id}", ct);
    }

    public Task<ApiResponse<WarehouseDto>> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<WarehouseDto>("warehouses", request, ct);
    }

    public Task<ApiResponse<WarehouseDto>> UpdateAsync(Guid id, UpdateWarehouseRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<WarehouseDto>($"warehouses/{id}", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"warehouses/{id}", ct);
    }

    public Task<ApiResponse> ToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync($"warehouses/{id}/toggle-active", ct);
    }
}
