using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.MAUI.Core.Http;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.MAUI.Services;

public interface ICategoryApiService
{
    Task<ApiResponse<List<CategoryResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<List<CategoryResponseDto>>> GetRootsAsync(CancellationToken ct = default);
    Task<ApiResponse<List<CategoryResponseDto>>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);
    Task<ApiResponse<PagedResult<CategoryResponseDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<ApiResponse<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<bool>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}

public class CategoryApiService : ICategoryApiService
{
    private readonly ApiClient _apiClient;

    public CategoryApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<CategoryResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<CategoryResponseDto>>("api/catalog/categories", ct);
    }

    public Task<ApiResponse<List<CategoryResponseDto>>> GetRootsAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<CategoryResponseDto>>("api/catalog/categories/roots", ct);
    }

    public Task<ApiResponse<List<CategoryResponseDto>>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<CategoryResponseDto>>($"api/catalog/categories/{parentId}/children", ct);
    }

    public Task<ApiResponse<PagedResult<CategoryResponseDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<PagedResult<CategoryResponseDto>>($"api/catalog/categories/paged?pageNumber={pageNumber}&pageSize={pageSize}", ct);
    }

    public Task<ApiResponse<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<CategoryResponseDto>($"api/catalog/categories/{id}", ct);
    }

    public Task<ApiResponse<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<CategoryResponseDto>("api/catalog/categories", dto, ct);
    }

    public Task<ApiResponse<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<CategoryResponseDto>($"api/catalog/categories/{id}", dto, ct);
    }

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync<bool>($"api/catalog/categories/{id}", ct);
    }

    public Task<ApiResponse<bool>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<bool>($"api/catalog/categories/{id}/toggle-active", null, ct);
    }
}
