using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Customers;

namespace RetalSystemAPI.Desktop.Services.Customers;

public interface ICustomerApiService
{
    Task<ApiResponse<List<CustomerSummaryDto>>> GetAllAsync(CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);
    Task<ApiResponse<PagedResult<CustomerSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);
    Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<CustomerDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<CustomerDto>> AddPhoneAsync(Guid customerId, CreateCustomerPhoneRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeletePhoneAsync(Guid customerId, Guid phoneId, CancellationToken ct = default);
}

public class CustomerApiService : ICustomerApiService
{
    private readonly ApiClient _apiClient;

    public CustomerApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<CustomerSummaryDto>>> GetAllAsync(CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        var url = "customers";
        var queryParams = new List<string>();
        if (type.HasValue) queryParams.Add($"type={type.Value}");
        if (isActive.HasValue) queryParams.Add($"isActive={isActive.Value}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);
        return _apiClient.GetAsync<List<CustomerSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<PagedResult<CustomerSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        var url = $"customers/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (type.HasValue) url += $"&type={type.Value}";
        if (isActive.HasValue) url += $"&isActive={isActive.Value}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return _apiClient.GetAsync<PagedResult<CustomerSummaryDto>>(url, ct);
    }

    public Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<CustomerDto>($"customers/{id}", ct);
    }

    public Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<CustomerDto>("customers", request, ct);
    }

    public Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<CustomerDto>($"customers/{id}", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"customers/{id}", ct);
    }

    public Task<ApiResponse<CustomerDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<CustomerDto>($"customers/{id}/toggle-active", ct);
    }

    public Task<ApiResponse<CustomerDto>> AddPhoneAsync(Guid customerId, CreateCustomerPhoneRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<CustomerDto>($"customers/{customerId}/phones", request, ct);
    }

    public Task<ApiResponse> DeletePhoneAsync(Guid customerId, Guid phoneId, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"customers/{customerId}/phones/{phoneId}", ct);
    }
}
