using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Sales;

namespace RetalSystemAPI.Desktop.Services.Sales;

public interface ISalesReturnApiService
{
    Task<ApiResponse<List<SalesReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<PagedResult<SalesReturnSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<SalesReturnDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<SalesReturnDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default);
    Task<ApiResponse<SalesReturnDto>> CreateAsync(CreateSalesReturnRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class SalesReturnApiService : ISalesReturnApiService
{
    private readonly ApiClient _apiClient;

    public SalesReturnApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<SalesReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = "sales-returns";
        var queryParams = new List<string>();
        if (branchId.HasValue) queryParams.Add($"branchId={branchId.Value}");
        if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");
        if (customerId.HasValue) queryParams.Add($"customerId={customerId.Value}");
        if (reason.HasValue) queryParams.Add($"reason={reason.Value}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);
        return _apiClient.GetAsync<List<SalesReturnSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PagedResult<SalesReturnSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = $"sales-returns/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (branchId.HasValue) url += $"&branchId={branchId.Value}";
        if (warehouseId.HasValue) url += $"&warehouseId={warehouseId.Value}";
        if (customerId.HasValue) url += $"&customerId={customerId.Value}";
        if (reason.HasValue) url += $"&reason={reason.Value}";
        if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
        if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return _apiClient.GetAsync<PagedResult<SalesReturnSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<SalesReturnDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<SalesReturnDto>($"sales-returns/{id}", ct);
    }

    public Task<ApiResponse<SalesReturnDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<SalesReturnDto>($"sales-returns/number/{Uri.EscapeDataString(returnNumber)}", ct);
    }

    public Task<ApiResponse<SalesReturnDto>> CreateAsync(CreateSalesReturnRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<SalesReturnDto>("sales-returns", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"sales-returns/{id}", ct);
    }
}
