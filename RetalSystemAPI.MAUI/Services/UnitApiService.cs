using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.MAUI.Core.Http;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;

namespace RetalSystemAPI.MAUI.Services;

public interface IUnitApiService
{
    Task<ApiResponse<List<UnitResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<UnitResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<UnitResponseDto>> CreateAsync(CreateUnitDto dto, CancellationToken ct = default);
    Task<ApiResponse<UnitResponseDto>> UpdateAsync(Guid id, UpdateUnitDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class UnitApiService : IUnitApiService
{
    private readonly ApiClient _apiClient;

    public UnitApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<UnitResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<UnitResponseDto>>("api/catalog/units", ct);
    }

    public Task<ApiResponse<UnitResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<UnitResponseDto>($"api/catalog/units/{id}", ct);
    }

    public Task<ApiResponse<UnitResponseDto>> CreateAsync(CreateUnitDto dto, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<UnitResponseDto>("api/catalog/units", dto, ct);
    }

    public Task<ApiResponse<UnitResponseDto>> UpdateAsync(Guid id, UpdateUnitDto dto, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<UnitResponseDto>($"api/catalog/units/{id}", dto, ct);
    }

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync<bool>($"api/catalog/units/{id}", ct);
    }
}
