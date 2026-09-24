using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Accounting;
using RetalSystemAPI.Desktop.Models.Common;

namespace RetalSystemAPI.Desktop.Services.Accounting;

public interface IExpenseApiService
{
    Task<ApiResponse<PagedResult<ExpenseSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, Guid? safeId = null, Guid? branchId = null, DateTime? fromDate = null, DateTime? toDate = null, bool? isCancelled = null, string? search = null, CancellationToken ct = default);
    Task<ApiResponse<ExpenseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ExpenseDto>> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default);
    Task<ApiResponse<ExpenseDto>> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> CancelAsync(Guid id, CancelExpenseRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class ExpenseApiService : IExpenseApiService
{
    private readonly ApiClient _apiClient;

    public ExpenseApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<PagedResult<ExpenseSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, Guid? safeId = null, Guid? branchId = null, DateTime? fromDate = null, DateTime? toDate = null, bool? isCancelled = null, string? search = null, CancellationToken ct = default)
    {
        var url = $"expenses?pageNumber={pageNumber}&pageSize={pageSize}";
        if (categoryId.HasValue) url += $"&expenseCategoryId={categoryId.Value}";
        if (safeId.HasValue) url += $"&safeId={safeId.Value}";
        if (branchId.HasValue) url += $"&branchId={branchId.Value}";
        if (fromDate.HasValue) url += $"&fromDate={Uri.EscapeDataString(fromDate.Value.ToString("o"))}";
        if (toDate.HasValue) url += $"&toDate={Uri.EscapeDataString(toDate.Value.ToString("o"))}";
        if (isCancelled.HasValue) url += $"&isCancelled={isCancelled.Value}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&searchTerm={Uri.EscapeDataString(search)}";

        return _apiClient.GetAsync<PagedResult<ExpenseSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<ExpenseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<ExpenseDto>($"expenses/{id}", ct);
    }

    public Task<ApiResponse<ExpenseDto>> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<ExpenseDto>("expenses", request, ct);
    }

    public Task<ApiResponse<ExpenseDto>> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<ExpenseDto>($"expenses/{id}", request, ct);
    }

    public Task<ApiResponse<bool>> CancelAsync(Guid id, CancelExpenseRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<bool>($"expenses/{id}/cancel", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"expenses/{id}", ct);
    }
}
