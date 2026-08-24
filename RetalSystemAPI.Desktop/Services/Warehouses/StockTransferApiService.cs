using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Warehouses;

namespace RetalSystemAPI.Desktop.Services.Warehouses;

public interface IStockTransferApiService
{
    Task<ApiResponse<List<StockTransferSummaryDto>>> GetAllAsync(
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<PagedResult<StockTransferSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<StockTransferDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<StockTransferDto>> GetByTransferNumberAsync(string transferNumber, CancellationToken ct = default);
    Task<ApiResponse<StockTransferDto>> CreateAsync(CreateStockTransferRequest request, CancellationToken ct = default);
    Task<ApiResponse<StockTransferDto>> UpdateAsync(Guid id, UpdateStockTransferRequest request, CancellationToken ct = default);
    Task<ApiResponse<StockTransferDto>> UpdateStatusAsync(Guid id, StockTransferStatus status, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class StockTransferApiService : IStockTransferApiService
{
    private readonly ApiClient _apiClient;

    public StockTransferApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<StockTransferSummaryDto>>> GetAllAsync(
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = "stock-transfers";
        var queryParams = new List<string>();
        if (fromWarehouseId.HasValue) queryParams.Add($"fromWarehouseId={fromWarehouseId.Value}");
        if (toWarehouseId.HasValue) queryParams.Add($"toWarehouseId={toWarehouseId.Value}");
        if (status.HasValue) queryParams.Add($"status={status.Value}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);
        return _apiClient.GetAsync<List<StockTransferSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PagedResult<StockTransferSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = $"stock-transfers/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (fromWarehouseId.HasValue) url += $"&fromWarehouseId={fromWarehouseId.Value}";
        if (toWarehouseId.HasValue) url += $"&toWarehouseId={toWarehouseId.Value}";
        if (status.HasValue) url += $"&status={status.Value}";
        if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
        if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return _apiClient.GetAsync<PagedResult<StockTransferSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<StockTransferDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<StockTransferDto>($"stock-transfers/{id}", ct);
    }

    public Task<ApiResponse<StockTransferDto>> GetByTransferNumberAsync(string transferNumber, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<StockTransferDto>($"stock-transfers/number/{Uri.EscapeDataString(transferNumber)}", ct);
    }

    public Task<ApiResponse<StockTransferDto>> CreateAsync(CreateStockTransferRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<StockTransferDto>("stock-transfers", request, ct);
    }

    public Task<ApiResponse<StockTransferDto>> UpdateAsync(Guid id, UpdateStockTransferRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<StockTransferDto>($"stock-transfers/{id}", request, ct);
    }

    public Task<ApiResponse<StockTransferDto>> UpdateStatusAsync(Guid id, StockTransferStatus status, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<StockTransferDto>($"stock-transfers/{id}/status", status, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"stock-transfers/{id}", ct);
    }
}
