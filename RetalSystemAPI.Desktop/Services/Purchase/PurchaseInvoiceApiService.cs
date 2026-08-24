using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Purchase;
using RetalSystemAPI.Desktop.Models.Sales;

namespace RetalSystemAPI.Desktop.Services.Purchase;

public interface IPurchaseInvoiceApiService
{
    Task<ApiResponse<PagedResult<PurchaseInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? supplierId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<PurchaseInvoiceDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<PurchaseInvoiceDto>> CreateAsync(CreatePurchaseInvoiceRequest request, CancellationToken ct = default);
    Task<ApiResponse<PurchaseInvoiceDto>> UpdateAsync(Guid id, UpdatePurchaseInvoiceRequest request, CancellationToken ct = default);
    Task<ApiResponse> CancelAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class PurchaseInvoiceApiService : IPurchaseInvoiceApiService
{
    private readonly ApiClient _apiClient;

    public PurchaseInvoiceApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<PagedResult<PurchaseInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? supplierId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var queryParams = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (supplierId.HasValue) queryParams.Add($"supplierId={supplierId.Value}");
        if (branchId.HasValue) queryParams.Add($"branchId={branchId.Value}");
        if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");
        if (status.HasValue) queryParams.Add($"status={(int)status.Value}");
        if (paymentMethod.HasValue) queryParams.Add($"paymentMethod={(int)paymentMethod.Value}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

        var endpoint = $"purchase-invoices?{string.Join("&", queryParams)}";
        return _apiClient.GetAsync<PagedResult<PurchaseInvoiceSummaryDto>>(endpoint, ct);
    }

    public Task<ApiResponse<PurchaseInvoiceDto>> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.GetAsync<PurchaseInvoiceDto>($"purchase-invoices/{id}", ct);

    public Task<ApiResponse<PurchaseInvoiceDto>> CreateAsync(CreatePurchaseInvoiceRequest request, CancellationToken ct = default) =>
        _apiClient.PostAsync<PurchaseInvoiceDto>("purchase-invoices", request, ct);

    public Task<ApiResponse<PurchaseInvoiceDto>> UpdateAsync(Guid id, UpdatePurchaseInvoiceRequest request, CancellationToken ct = default) =>
        _apiClient.PutAsync<PurchaseInvoiceDto>($"purchase-invoices/{id}", request, ct);

    public Task<ApiResponse> CancelAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.PatchAsync($"purchase-invoices/{id}/cancel", ct);

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.DeleteAsync($"purchase-invoices/{id}", ct);
}
