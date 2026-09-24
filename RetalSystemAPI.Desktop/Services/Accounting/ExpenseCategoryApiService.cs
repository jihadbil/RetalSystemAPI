using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Accounting;

namespace RetalSystemAPI.Desktop.Services.Accounting;

public interface IExpenseCategoryApiService
{
    Task<ApiResponse<List<ExpenseCategoryDto>>> GetAllAsync(bool? isActive = null, string? search = null, CancellationToken ct = default);
    Task<ApiResponse<ExpenseCategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ExpenseCategoryDto>> CreateAsync(CreateExpenseCategoryRequest request, CancellationToken ct = default);
    Task<ApiResponse<ExpenseCategoryDto>> UpdateAsync(Guid id, UpdateExpenseCategoryRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ExpenseCategoryDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}

public class ExpenseCategoryApiService : IExpenseCategoryApiService
{
    private readonly ApiClient _apiClient;

    public ExpenseCategoryApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<ExpenseCategoryDto>>> GetAllAsync(bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        var url = "expense-categories";
        var queryParams = new List<string>();
        if (isActive.HasValue) queryParams.Add($"isActive={isActive.Value}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);
        return _apiClient.GetAsync<List<ExpenseCategoryDto>>(url, ct);
    }

    public Task<ApiResponse<ExpenseCategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<ExpenseCategoryDto>($"expense-categories/{id}", ct);
    }

    public Task<ApiResponse<ExpenseCategoryDto>> CreateAsync(CreateExpenseCategoryRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<ExpenseCategoryDto>("expense-categories", request, ct);
    }

    public Task<ApiResponse<ExpenseCategoryDto>> UpdateAsync(Guid id, UpdateExpenseCategoryRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<ExpenseCategoryDto>($"expense-categories/{id}", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"expense-categories/{id}", ct);
    }

    public Task<ApiResponse<ExpenseCategoryDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<ExpenseCategoryDto>($"expense-categories/{id}/toggle-active", ct);
    }
}
