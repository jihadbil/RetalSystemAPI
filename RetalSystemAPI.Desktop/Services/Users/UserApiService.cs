using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Users;

namespace RetalSystemAPI.Desktop.Services.Users;

public interface IUserApiService
{
    Task<ApiResponse<PagedResult<UserSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null,
        Guid? branchId = null,
        CancellationToken ct = default);

    Task<ApiResponse<UserDetailsDto>> GetByIdAsync(string id, CancellationToken ct = default);

    Task<ApiResponse<UserSummaryDto>> CreateAsync(CreateUserRequest request, CancellationToken ct = default);

    Task<ApiResponse<UserSummaryDto>> UpdateAsync(string id, UpdateUserRequest request, CancellationToken ct = default);

    Task<ApiResponse> DeleteAsync(string id, CancellationToken ct = default);

    Task<ApiResponse> ResetPasswordAsync(string id, ResetPasswordRequest request, CancellationToken ct = default);

    Task<ApiResponse<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default);

    Task<ApiResponse<RoleDto>> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default);

    Task<ApiResponse> DeleteRoleAsync(string roleId, CancellationToken ct = default);

    Task<ApiResponse<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken ct = default);

    Task<ApiResponse<RolePermissionsDto>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default);

    Task<ApiResponse> UpdateRolePermissionsAsync(string roleId, UpdateRolePermissionsDto request, CancellationToken ct = default);

    Task<ApiResponse<UserPermissionsDto>> GetUserPermissionsAsync(string userId, CancellationToken ct = default);

    Task<ApiResponse> UpdateUserPermissionsAsync(string userId, UpdateUserPermissionsDto request, CancellationToken ct = default);
}

public class UserApiService : IUserApiService
{
    private readonly ApiClient _apiClient;

    public UserApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<PagedResult<UserSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null,
        Guid? branchId = null,
        CancellationToken ct = default)
    {
        var query = $"users?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }
        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            query += $"&branchId={branchId.Value}";
        }

        return _apiClient.GetAsync<PagedResult<UserSummaryDto>>(query, ct);
    }

    public Task<ApiResponse<UserDetailsDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<UserDetailsDto>($"users/{id}", ct);
    }

    public Task<ApiResponse<UserSummaryDto>> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<UserSummaryDto>("users", request, ct);
    }

    public Task<ApiResponse<UserSummaryDto>> UpdateAsync(string id, UpdateUserRequest request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<UserSummaryDto>($"users/{id}", request, ct);
    }

    public Task<ApiResponse> DeleteAsync(string id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"users/{id}", ct);
    }

    public Task<ApiResponse> ResetPasswordAsync(string id, ResetPasswordRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync($"users/{id}/reset-password", request, ct);
    }

    public Task<ApiResponse<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<RoleDto>>("users/roles", ct);
    }

    public Task<ApiResponse<RoleDto>> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<RoleDto>("users/roles", request, ct);
    }

    public Task<ApiResponse> DeleteRoleAsync(string roleId, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync($"users/roles/{roleId}", ct);
    }

    public Task<ApiResponse<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<PermissionDto>>("users/permissions", ct);
    }

    public Task<ApiResponse<RolePermissionsDto>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<RolePermissionsDto>($"users/roles/{roleId}/permissions", ct);
    }

    public Task<ApiResponse> UpdateRolePermissionsAsync(string roleId, UpdateRolePermissionsDto request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync($"users/roles/{roleId}/permissions", request, ct);
    }

    public Task<ApiResponse<UserPermissionsDto>> GetUserPermissionsAsync(string userId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<UserPermissionsDto>($"users/{userId}/permissions", ct);
    }

    public Task<ApiResponse> UpdateUserPermissionsAsync(string userId, UpdateUserPermissionsDto request, CancellationToken ct = default)
    {
        return _apiClient.PutAsync($"users/{userId}/permissions", request, ct);
    }
}
