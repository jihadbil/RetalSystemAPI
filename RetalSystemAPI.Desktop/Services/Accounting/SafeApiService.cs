using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Accounting;
using RetalSystemAPI.Desktop.Models.Common;

namespace RetalSystemAPI.Desktop.Services.Accounting;

public interface ISafeApiService
{
    Task<ApiResponse<List<SafeSummaryDto>>> GetAllAsync(Guid? branchId = null, SafeType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);
    Task<ApiResponse<PagedResult<SafeSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? branchId = null, SafeType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);
    Task<ApiResponse<SafeDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<SafeDto>> CreateAsync(CreateSafeRequest request, CancellationToken ct = default);
    Task<ApiResponse<SafeDto>> UpdateAsync(Guid id, UpdateSafeRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<SafeDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<SafeTransactionDto>> CreateTransactionAsync(CreateSafeTransactionRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> TransferFundsAsync(TransferFundsRequest request, CancellationToken ct = default);
    Task<ApiResponse<PagedResult<SafeTransactionDto>>> GetTransactionsPagedAsync(Guid safeId, int pageNumber = 1, int pageSize = 20, DateTime? fromDate = null, DateTime? toDate = null, TransactionDirection? direction = null, FinancialDocumentType? documentType = null, CancellationToken ct = default);
}

public class SafeApiService : ISafeApiService
{
    private readonly ApiClient _apiClient;

    public SafeApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<SafeSummaryDto>>> GetAllAsync(Guid? branchId = null, SafeType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        var url = "safes";
        var queryParams = new List<string>();
        if (branchId.HasValue) queryParams.Add($"branchId={branchId.Value}");
        if (type.HasValue) queryParams.Add($"type={type.Value}");
        if (isActive.HasValue) queryParams.Add($"isActive={isActive.Value}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);
        return _apiClient.GetAsync<List<SafeSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PagedResult<SafeSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? branchId = null, SafeType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        var url = $"safes/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (branchId.HasValue) url += $"&branchId={branchId.Value}";
        if (type.HasValue) url += $"&type={type.Value}";
        if (isActive.HasValue) url += $"&isActive={isActive.Value}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return _apiClient.GetAsync<PagedResult<SafeSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<SafeDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<SafeDto>($"safes/{id}", ct);
    }

    public Task<ApiResponse<SafeDto>> CreateAsync(CreateSafeRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<SafeDto>("safes", request, ct);
    }

    public Task<ApiResponse<SafeDto>> UpdateAsync(Guid id, UpdateSafeRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<SafeDto>($"safes/{id}", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"safes/{id}", ct);
    }

    public Task<ApiResponse<SafeDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<SafeDto>($"safes/{id}/toggle-active", ct);
    }

    public Task<ApiResponse<SafeTransactionDto>> CreateTransactionAsync(CreateSafeTransactionRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<SafeTransactionDto>("safes/transactions", request, ct);
    }

    public Task<ApiResponse<bool>> TransferFundsAsync(TransferFundsRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<bool>("safes/transfer", request, ct);
    }

    public Task<ApiResponse<PagedResult<SafeTransactionDto>>> GetTransactionsPagedAsync(Guid safeId, int pageNumber = 1, int pageSize = 20, DateTime? fromDate = null, DateTime? toDate = null, TransactionDirection? direction = null, FinancialDocumentType? documentType = null, CancellationToken ct = default)
    {
        var url = $"safes/transactions?safeId={safeId}&pageNumber={pageNumber}&pageSize={pageSize}";
        if (fromDate.HasValue) url += $"&fromDate={Uri.EscapeDataString(fromDate.Value.ToString("o"))}";
        if (toDate.HasValue) url += $"&toDate={Uri.EscapeDataString(toDate.Value.ToString("o"))}";
        if (direction.HasValue) url += $"&direction={direction.Value}";
        if (documentType.HasValue) url += $"&documentType={documentType.Value}";

        return _apiClient.GetAsync<PagedResult<SafeTransactionDto>>(url, ct);
    }
}
