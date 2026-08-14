using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.DTOs.Users;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Users.Interfaces;

namespace RetalSystemAPI.Services.Users.Implementations;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        ICurrentTenantService currentTenantService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _currentTenantService = currentTenantService;
    }

    public async Task<ServiceResult<PagedResult<UserSummaryDto>>> GetPagedUsersAsync(
        int pageNumber,
        int pageSize,
        string? search,
        Guid? branchId,
        CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;

        var query = _context.Users
            .Include(u => u.Branch)
            .Where(u => u.TenantId == tenantId)
            .AsNoTracking();

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            query = query.Where(u => u.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(searchLower)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchLower)));
        }

        int totalCount = await query.CountAsync(ct);

        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtoList = new List<UserSummaryDto>();

        foreach (var u in users)
        {
            var roles = (await _userManager.GetRolesAsync(u)).ToList();
            bool isActive = !u.LockoutEnd.HasValue || u.LockoutEnd.Value <= DateTimeOffset.UtcNow;

            dtoList.Add(new UserSummaryDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber,
                TenantId = u.TenantId,
                BranchId = u.BranchId,
                BranchName = u.Branch?.Name,
                Roles = roles,
                IsActive = isActive
            });
        }

        var pagedResult = PagedResult<UserSummaryDto>.Create(dtoList, totalCount, pageNumber, pageSize);
        return ServiceResult<PagedResult<UserSummaryDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<UserDetailsDto>> GetUserByIdAsync(string id, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);

        if (user == null)
        {
            return ServiceResult<UserDetailsDto>.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        bool isActive = !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;

        var details = new UserDetailsDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            BranchName = user.Branch?.Name,
            Roles = roles,
            IsActive = isActive
        };

        return ServiceResult<UserDetailsDto>.Success(details);
    }

    public async Task<ServiceResult<UserSummaryDto>> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;

        var existingUser = await _userManager.FindByNameAsync(dto.UserName);
        if (existingUser != null)
        {
            return ServiceResult<UserSummaryDto>.Failure("اسم المستخدم موجود بالفعل", ErrorCodes.UserAlreadyExists);
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail != null)
            {
                return ServiceResult<UserSummaryDto>.Failure("البريد الإلكتروني مستخدم بالفعل", ErrorCodes.UserAlreadyExists);
            }
        }

        var targetTenantId = dto.TenantId != Guid.Empty ? dto.TenantId : tenantId;

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            TenantId = targetTenantId,
            BranchId = dto.BranchId
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ServiceResult<UserSummaryDto>.Failure($"فشل إنشاء المستخدم: {errors}", ErrorCodes.ValidationError);
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));
            }
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var branch = await _context.Branches.FindAsync(new object[] { user.BranchId }, ct);

        var summary = new UserSummaryDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            BranchName = branch?.Name,
            Roles = roles,
            IsActive = true
        };

        return ServiceResult<UserSummaryDto>.Success(summary);
    }

    public async Task<ServiceResult<UserSummaryDto>> UpdateUserAsync(string id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);

        if (user == null)
        {
            return ServiceResult<UserSummaryDto>.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.BranchId = dto.BranchId;

        if (dto.IsActive)
        {
            user.LockoutEnd = null;
        }
        else
        {
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return ServiceResult<UserSummaryDto>.Failure($"فشل تحديث بيانات المستخدم: {errors}", ErrorCodes.ValidationError);
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.Role))
            {
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                }
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Role);
            }
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var branch = await _context.Branches.FindAsync(new object[] { user.BranchId }, ct);

        var summary = new UserSummaryDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            BranchName = branch?.Name,
            Roles = roles,
            IsActive = dto.IsActive
        };

        return ServiceResult<UserSummaryDto>.Success(summary);
    }

    public async Task<ServiceResult> DeleteUserAsync(string id, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);

        if (user == null)
        {
            return ServiceResult.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            return ServiceResult.Failure($"فشل حذف المستخدم: {errors}", ErrorCodes.ValidationError);
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ResetPasswordAsync(string id, ResetPasswordDto dto, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);

        if (user == null)
        {
            return ServiceResult.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ServiceResult.Failure($"فشل تعيين كلمة المرور: {errors}", ErrorCodes.ValidationError);
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default)
    {
        var defaultRoles = new[] { "Admin", "Manager", "User", "Cashier" };
        foreach (var roleName in defaultRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var roles = await _roleManager.Roles
            .Select(r => new RoleDto { Id = r.Id, Name = r.Name ?? string.Empty })
            .ToListAsync(ct);

        return ServiceResult<List<RoleDto>>.Success(roles);
    }
}
