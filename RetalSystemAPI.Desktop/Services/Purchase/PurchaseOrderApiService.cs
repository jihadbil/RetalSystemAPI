using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Purchase;

namespace RetalSystemAPI.Desktop.Services.Purchase;

public interface IPurchaseOrderApiService
{
    Task<ApiResponse<List<PurchaseOrderSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, CancellationToken ct = default);
    Task<ApiResponse<PagedResult<PurchaseOrderSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, string? search = null, CancellationToken ct = default);
    Task<ApiResponse<PurchaseOrderDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken ct = default);
    Task<ApiResponse<PurchaseOrderDto>> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request, CancellationToken ct = default);
    Task<ApiResponse<PurchaseOrderDto>> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class PurchaseOrderApiService : IPurchaseOrderApiService
{
    private readonly ApiClient _apiClient;

    public PurchaseOrderApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<PurchaseOrderSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, CancellationToken ct = default)
    {
        var url = "purchase-orders";
        var queryParams = new List<string>();
        if (branchId.HasValue) queryParams.Add($"branchId={branchId.Value}");
        if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");
        if (status.HasValue) queryParams.Add($"status={(int)status.Value}");
        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

        return _apiClient.GetAsync<List<PurchaseOrderSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PagedResult<PurchaseOrderSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, string? search = null, CancellationToken ct = default)
    {
        var url = $"purchase-orders/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (branchId.HasValue) url += $"&branchId={branchId.Value}";
        if (warehouseId.HasValue) url += $"&warehouseId={warehouseId.Value}";
        if (status.HasValue) url += $"&status={(int)status.Value}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return _apiClient.GetAsync<PagedResult<PurchaseOrderSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PurchaseOrderDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<PurchaseOrderDto>($"purchase-orders/{id}", ct);
    }

    public Task<ApiResponse<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<PurchaseOrderDto>("purchase-orders", request, ct);
    }

    public Task<ApiResponse<PurchaseOrderDto>> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<PurchaseOrderDto>($"purchase-orders/{id}", request, ct);
    }

    public Task<ApiResponse<PurchaseOrderDto>> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<PurchaseOrderDto>($"purchase-orders/{id}/status", status, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"purchase-orders/{id}", ct);
    }
}
