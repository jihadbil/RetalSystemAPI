using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Warehouses;

namespace RetalSystemAPI.Desktop.Services.Warehouses;

public interface IStockApiService
{
    // Storge Stock
    Task<ApiResponse<List<StorgeStockDto>>> GetStorgeStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);
    Task<ApiResponse<StorgeStockDto>> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);
    Task<ApiResponse<StorgeStockDto>> SetStorgeStockAsync(SetStorgeStockRequest request, CancellationToken ct = default);
    Task<ApiResponse<List<StorgeStockDto>>> GetLowStorgeStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default);

    // Showroom Stock
    Task<ApiResponse<List<ShowroomStockDto>>> GetShowroomStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);
    Task<ApiResponse<ShowroomStockDto>> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);
    Task<ApiResponse<ShowroomStockDto>> SetShowroomStockAsync(SetShowroomStockRequest request, CancellationToken ct = default);
    Task<ApiResponse<List<ShowroomStockDto>>> GetLowShowroomStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default);
}

public class StockApiService : IStockApiService
{
    private readonly ApiClient _apiClient;

    public StockApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<StorgeStockDto>>> GetStorgeStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<StorgeStockDto>>($"stock/storge/{warehouseId}", ct);
    }

    public Task<ApiResponse<StorgeStockDto>> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<StorgeStockDto>($"stock/storge/{warehouseId}/barcode/{productBarcodeId}", ct);
    }

    public Task<ApiResponse<StorgeStockDto>> SetStorgeStockAsync(SetStorgeStockRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<StorgeStockDto>("stock/storge", request, ct);
    }

    public Task<ApiResponse<List<StorgeStockDto>>> GetLowStorgeStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        var url = "stock/storge/low-stock";
        if (warehouseId.HasValue) url += $"?warehouseId={warehouseId.Value}";
        return _apiClient.GetAsync<List<StorgeStockDto>>(url, ct);
    }

    public Task<ApiResponse<List<ShowroomStockDto>>> GetShowroomStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<ShowroomStockDto>>($"stock/showroom/{warehouseId}", ct);
    }

    public Task<ApiResponse<ShowroomStockDto>> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<ShowroomStockDto>($"stock/showroom/{warehouseId}/product/{productId}", ct);
    }

    public Task<ApiResponse<ShowroomStockDto>> SetShowroomStockAsync(SetShowroomStockRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<ShowroomStockDto>("stock/showroom", request, ct);
    }

    public Task<ApiResponse<List<ShowroomStockDto>>> GetLowShowroomStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        var url = "stock/showroom/low-stock";
        if (warehouseId.HasValue) url += $"?warehouseId={warehouseId.Value}";
        return _apiClient.GetAsync<List<ShowroomStockDto>>(url, ct);
    }
}
