using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Users;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Users.Interfaces;

public interface IUserService
{
    Task<ServiceResult<PagedResult<UserSummaryDto>>> GetPagedUsersAsync(
        int pageNumber,
        int pageSize,
        string? search,
        Guid? branchId,
        CancellationToken ct = default);

    Task<ServiceResult<UserDetailsDto>> GetUserByIdAsync(string id, CancellationToken ct = default);

    Task<ServiceResult<UserSummaryDto>> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default);

    Task<ServiceResult<UserSummaryDto>> UpdateUserAsync(string id, UpdateUserDto dto, CancellationToken ct = default);

    Task<ServiceResult> DeleteUserAsync(string id, CancellationToken ct = default);

    Task<ServiceResult> ResetPasswordAsync(string id, ResetPasswordDto dto, CancellationToken ct = default);

    Task<ServiceResult<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default);
}
