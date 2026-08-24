using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Warehouses;

namespace RetalSystemAPI.Desktop.Services.Warehouses;

public interface IStockAdjustmentApiService
{
    Task<ApiResponse<List<StockAdjustmentSummaryDto>>> GetAllAsync(
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<PagedResult<StockAdjustmentSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<StockAdjustmentDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<StockAdjustmentDto>> GetByAdjustmentNumberAsync(string adjustmentNumber, CancellationToken ct = default);
    Task<ApiResponse<StockAdjustmentDto>> CreateAsync(CreateStockAdjustmentRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class StockAdjustmentApiService : IStockAdjustmentApiService
{
    private readonly ApiClient _apiClient;

    public StockAdjustmentApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<StockAdjustmentSummaryDto>>> GetAllAsync(
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = "stock-adjustments";
        var queryParams = new List<string>();
        if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");
        if (reason.HasValue) queryParams.Add($"reason={reason.Value}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);
        return _apiClient.GetAsync<List<StockAdjustmentSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PagedResult<StockAdjustmentSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = $"stock-adjustments/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (warehouseId.HasValue) url += $"&warehouseId={warehouseId.Value}";
        if (reason.HasValue) url += $"&reason={reason.Value}";
        if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
        if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return _apiClient.GetAsync<PagedResult<StockAdjustmentSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<StockAdjustmentDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<StockAdjustmentDto>($"stock-adjustments/{id}", ct);
    }

    public Task<ApiResponse<StockAdjustmentDto>> GetByAdjustmentNumberAsync(string adjustmentNumber, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<StockAdjustmentDto>($"stock-adjustments/number/{Uri.EscapeDataString(adjustmentNumber)}", ct);
    }

    public Task<ApiResponse<StockAdjustmentDto>> CreateAsync(CreateStockAdjustmentRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<StockAdjustmentDto>("stock-adjustments", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"stock-adjustments/{id}", ct);
    }
}
