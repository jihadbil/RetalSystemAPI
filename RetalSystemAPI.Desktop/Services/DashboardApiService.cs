using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Dashboard;

namespace RetalSystemAPI.Desktop.Services;

public interface IDashboardApiService
{
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(Guid? branchId = null, CancellationToken ct = default);
    Task<ApiResponse<List<DailySalesPointDto>>> GetSalesTrendAsync(int days = 7, Guid? branchId = null, CancellationToken ct = default);
    Task<ApiResponse<List<LowStockItemDto>>> GetLowStockAlertsAsync(Guid? branchId = null, CancellationToken ct = default);
    Task<ApiResponse<List<TopSellingProductDto>>> GetTopSellingProductsAsync(int count = 5, Guid? branchId = null, CancellationToken ct = default);
}

public class DashboardApiService : IDashboardApiService
{
    private readonly ApiClient _apiClient;

    public DashboardApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(Guid? branchId = null, CancellationToken ct = default)
    {
        string endpoint = branchId.HasValue && branchId.Value != Guid.Empty
            ? $"dashboard/summary?branchId={branchId.Value}"
            : "dashboard/summary";

        return _apiClient.GetAsync<DashboardSummaryDto>(endpoint, ct);
    }

    public Task<ApiResponse<List<DailySalesPointDto>>> GetSalesTrendAsync(int days = 7, Guid? branchId = null, CancellationToken ct = default)
    {
        string endpoint = branchId.HasValue && branchId.Value != Guid.Empty
            ? $"dashboard/sales-trend?days={days}&branchId={branchId.Value}"
            : $"dashboard/sales-trend?days={days}";

        return _apiClient.GetAsync<List<DailySalesPointDto>>(endpoint, ct);
    }

    public Task<ApiResponse<List<LowStockItemDto>>> GetLowStockAlertsAsync(Guid? branchId = null, CancellationToken ct = default)
    {
        string endpoint = branchId.HasValue && branchId.Value != Guid.Empty
            ? $"dashboard/low-stock?branchId={branchId.Value}"
            : "dashboard/low-stock";

        return _apiClient.GetAsync<List<LowStockItemDto>>(endpoint, ct);
    }

    public Task<ApiResponse<List<TopSellingProductDto>>> GetTopSellingProductsAsync(int count = 5, Guid? branchId = null, CancellationToken ct = default)
    {
        string endpoint = branchId.HasValue && branchId.Value != Guid.Empty
            ? $"dashboard/top-products?count={count}&branchId={branchId.Value}"
            : $"dashboard/top-products?count={count}";

        return _apiClient.GetAsync<List<TopSellingProductDto>>(endpoint, ct);
    }
}
