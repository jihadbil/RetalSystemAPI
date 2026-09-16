using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Purchase;
using RetalSystemAPI.Desktop.Models.Sales;

namespace RetalSystemAPI.Desktop.Services.Purchase;

public interface IPurchaseReturnApiService
{
    Task<ApiResponse<List<PurchaseReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<PagedResult<PurchaseReturnSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<PurchaseReturnDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<PurchaseReturnDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default);
    Task<ApiResponse<PurchaseReturnDto>> CreateAsync(CreatePurchaseReturnRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class PurchaseReturnApiService : IPurchaseReturnApiService
{
    private readonly ApiClient _apiClient;

    public PurchaseReturnApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<PurchaseReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = "purchase-returns";
        var queryParams = new List<string>();
        if (branchId.HasValue) queryParams.Add($"branchId={branchId.Value}");
        if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");
        if (supplierId.HasValue) queryParams.Add($"supplierId={supplierId.Value}");
        if (reason.HasValue) queryParams.Add($"reason={(int)reason.Value}");
        if (paymentMethod.HasValue) queryParams.Add($"paymentMethod={(int)paymentMethod.Value}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);
        return _apiClient.GetAsync<List<PurchaseReturnSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PagedResult<PurchaseReturnSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = $"purchase-returns/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (branchId.HasValue) url += $"&branchId={branchId.Value}";
        if (warehouseId.HasValue) url += $"&warehouseId={warehouseId.Value}";
        if (supplierId.HasValue) url += $"&supplierId={supplierId.Value}";
        if (reason.HasValue) url += $"&reason={(int)reason.Value}";
        if (paymentMethod.HasValue) url += $"&paymentMethod={(int)paymentMethod.Value}";
        if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
        if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return _apiClient.GetAsync<PagedResult<PurchaseReturnSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PurchaseReturnDto>> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.GetAsync<PurchaseReturnDto>($"purchase-returns/{id}", ct);

    public Task<ApiResponse<PurchaseReturnDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default) =>
        _apiClient.GetAsync<PurchaseReturnDto>($"purchase-returns/number/{Uri.EscapeDataString(returnNumber)}", ct);

    public Task<ApiResponse<PurchaseReturnDto>> CreateAsync(CreatePurchaseReturnRequest request, CancellationToken ct = default) =>
        _apiClient.PostAsync<PurchaseReturnDto>("purchase-returns", request, ct);

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.DeleteAsync($"purchase-returns/{id}", ct);
}
