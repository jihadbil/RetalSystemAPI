using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Suppliers;

namespace RetalSystemAPI.Desktop.Services.Suppliers;

public interface ISupplierApiService
{
    Task<ApiResponse<List<SupplierSummaryDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<PagedResult<SupplierSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string? search = null, CancellationToken ct = default);
    Task<ApiResponse<SupplierDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<SupplierDto>> CreateAsync(CreateSupplierRequest request, CancellationToken ct = default);
    Task<ApiResponse<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<SupplierDto>> AddPhoneAsync(Guid supplierId, CreateSupplierPhoneRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeletePhoneAsync(Guid supplierId, Guid phoneId, CancellationToken ct = default);
}

public class SupplierApiService : ISupplierApiService
{
    private readonly ApiClient _apiClient;

    public SupplierApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<SupplierSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<SupplierSummaryDto>>("suppliers", ct);
    }

    public Task<ApiResponse<PagedResult<SupplierSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string? search = null, CancellationToken ct = default)
    {
        var url = $"suppliers/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }
        return _apiClient.GetAsync<PagedResult<SupplierSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<SupplierDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<SupplierDto>($"suppliers/{id}", ct);
    }

    public Task<ApiResponse<SupplierDto>> CreateAsync(CreateSupplierRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<SupplierDto>("suppliers", request, ct);
    }

    public Task<ApiResponse<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<SupplierDto>($"suppliers/{id}", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"suppliers/{id}", ct);
    }

    public Task<ApiResponse<SupplierDto>> AddPhoneAsync(Guid supplierId, CreateSupplierPhoneRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<SupplierDto>($"suppliers/{supplierId}/phones", request, ct);
    }

    public Task<ApiResponse> DeletePhoneAsync(Guid supplierId, Guid phoneId, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"suppliers/{supplierId}/phones/{phoneId}", ct);
    }
}
