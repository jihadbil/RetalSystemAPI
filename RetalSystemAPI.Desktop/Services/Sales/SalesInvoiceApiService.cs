using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Sales;

namespace RetalSystemAPI.Desktop.Services.Sales;

public interface ISalesInvoiceApiService
{
    Task<ApiResponse<List<SalesInvoiceSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<PagedResult<SalesInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    Task<ApiResponse<SalesInvoiceDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<SalesInvoiceDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default);
    Task<ApiResponse<SalesInvoiceDto>> CreateAsync(CreateSalesInvoiceRequest request, CancellationToken ct = default);
    Task<ApiResponse<SalesInvoiceDto>> UpdateAsync(Guid id, UpdateSalesInvoiceRequest request, CancellationToken ct = default);
    Task<ApiResponse<SalesInvoiceDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class SalesInvoiceApiService : ISalesInvoiceApiService
{
    private readonly ApiClient _apiClient;

    public SalesInvoiceApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<SalesInvoiceSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = "sales-invoices";
        var queryParams = new List<string>();
        if (branchId.HasValue) queryParams.Add($"branchId={branchId.Value}");
        if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");
        if (customerId.HasValue) queryParams.Add($"customerId={customerId.Value}");
        if (status.HasValue) queryParams.Add($"status={status.Value}");
        if (paymentMethod.HasValue) queryParams.Add($"paymentMethod={paymentMethod.Value}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);
        return _apiClient.GetAsync<List<SalesInvoiceSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PagedResult<SalesInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var url = $"sales-invoices/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (branchId.HasValue) url += $"&branchId={branchId.Value}";
        if (warehouseId.HasValue) url += $"&warehouseId={warehouseId.Value}";
        if (customerId.HasValue) url += $"&customerId={customerId.Value}";
        if (status.HasValue) url += $"&status={status.Value}";
        if (paymentMethod.HasValue) url += $"&paymentMethod={paymentMethod.Value}";
        if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
        if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return _apiClient.GetAsync<PagedResult<SalesInvoiceSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<SalesInvoiceDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<SalesInvoiceDto>($"sales-invoices/{id}", ct);
    }

    public Task<ApiResponse<SalesInvoiceDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<SalesInvoiceDto>($"sales-invoices/number/{Uri.EscapeDataString(invoiceNumber)}", ct);
    }

    public Task<ApiResponse<SalesInvoiceDto>> CreateAsync(CreateSalesInvoiceRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<SalesInvoiceDto>("sales-invoices", request, ct);
    }

    public Task<ApiResponse<SalesInvoiceDto>> UpdateAsync(Guid id, UpdateSalesInvoiceRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<SalesInvoiceDto>($"sales-invoices/{id}", request, ct);
    }

    public Task<ApiResponse<SalesInvoiceDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<SalesInvoiceDto>($"sales-invoices/{id}/status", status, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"sales-invoices/{id}", ct);
    }
}
