using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Users;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Users.Interfaces;

namespace RetalSystemAPI.Services.Users.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة المستخدمين والأدوار والتحقق من هوية المستأجر وإعادة تعيين كلمات المرور.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;

    /// <summary>
    /// تهيئة خدمة المستخدمين وحقن خدمات الهوية وقاعدة البيانات والمستأجر الحالي.
    /// </summary>
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

    /// <inheritdoc />
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

        // جلب أدوار صفحة المستخدمين في استعلامين موحدين بدل استعلام لكل مستخدم (N+1)
        var userIds = users.Select(u => u.Id).ToList();
        var rolesById = (await _context.UserRoles
                .AsNoTracking()
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, RoleName = r.Name })
                .ToListAsync(ct))
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName ?? string.Empty).ToList());

        foreach (var u in users)
        {
            var roles = rolesById.TryGetValue(u.Id, out var userRoles) ? userRoles : new List<string>();
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

    /// <inheritdoc />
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

        var userClaims = await _userManager.GetClaimsAsync(user);
        var directPermissions = userClaims
            .Where(c => c.Type == Permissions.ClaimType)
            .Select(c => c.Value)
            .ToList();

        var effectivePermissions = new HashSet<string>(directPermissions, StringComparer.OrdinalIgnoreCase);
        bool isAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));
        if (isAdmin)
        {
            foreach (var code in AppPermissions.GetAllPermissionCodes())
            {
                effectivePermissions.Add(code);
            }
        }
        else
        {
            foreach (var r in roles)
            {
                var roleObj = await _roleManager.FindByNameAsync(r);
                if (roleObj != null)
                {
                    var rClaims = await _roleManager.GetClaimsAsync(roleObj);
                    foreach (var c in rClaims.Where(rc => rc.Type == Permissions.ClaimType))
                    {
                        effectivePermissions.Add(c.Value);
                    }
                }
            }
        }

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
            IsActive = isActive,
            DirectPermissions = directPermissions,
            EffectivePermissions = effectivePermissions.OrderBy(p => p).ToList()
        };

        return ServiceResult<UserDetailsDto>.Success(details);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

        var roles = await _roleManager.Roles.ToListAsync(ct);
        var roleIds = roles.Select(r => r.Id).ToList();

        // جلب مطالبات الصلاحيات لكل الأدوار في استعلام واحد بدل استعلام لكل دور (N+1)
        var permissionClaimsByRole = (await _context.RoleClaims
                .AsNoTracking()
                .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == Permissions.ClaimType)
                .Select(rc => new { rc.RoleId, rc.ClaimValue })
                .ToListAsync(ct))
            .GroupBy(x => x.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ClaimValue)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .OrderBy(p => p)
                .Select(p => p!)
                .ToList());

        var roleDtos = new List<RoleDto>();

        foreach (var r in roles)
        {
            var perms = permissionClaimsByRole.TryGetValue(r.Id, out var rolePerms)
                ? rolePerms
                : new List<string>();

            if (r.Name != null && (r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                perms = AppPermissions.GetAllPermissionCodes();
            }

            roleDtos.Add(new RoleDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                Permissions = perms
            });
        }

        return ServiceResult<List<RoleDto>>.Success(roleDtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<RoleDto>> CreateRoleAsync(string roleName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return ServiceResult<RoleDto>.Failure("اسم الدور مطلوب", ErrorCodes.ValidationError);
        }

        roleName = roleName.Trim();
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return ServiceResult<RoleDto>.Failure("هذا الدور موجود بالفعل", ErrorCodes.ValidationError);
        }

        var role = new IdentityRole(roleName);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ServiceResult<RoleDto>.Failure($"فشل إنشاء الدور: {errors}", ErrorCodes.ValidationError);
        }

        return ServiceResult<RoleDto>.Success(new RoleDto { Id = role.Id, Name = role.Name ?? string.Empty });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteRoleAsync(string roleId, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return ServiceResult.Failure("الدور غير موجود", ErrorCodes.ValidationError);
        }

        if (role.Name != null && (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
        {
            return ServiceResult.Failure("لا يمكن حذف دور المسؤول الرئيسي", ErrorCodes.ValidationError);
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ServiceResult.Failure($"فشل حذف الدور: {errors}", ErrorCodes.ValidationError);
        }

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public Task<ServiceResult<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        var list = AppPermissions.All.Select(p => new PermissionDto
        {
            Code = p.Code,
            Name = p.Name,
            Category = p.Category,
            Description = p.Description,
            IsScreenAccess = p.IsScreenAccess
        }).ToList();

        return Task.FromResult(ServiceResult<List<PermissionDto>>.Success(list));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<RolePermissionsDto>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return ServiceResult<RolePermissionsDto>.Failure("الدور غير موجود", ErrorCodes.ValidationError);
        }

        List<string> perms;
        if (role.Name != null && (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
        {
            perms = AppPermissions.GetAllPermissionCodes();
        }
        else
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            perms = claims
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .OrderBy(p => p)
                .ToList();
        }

        return ServiceResult<RolePermissionsDto>.Success(new RolePermissionsDto
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            Permissions = perms
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateRolePermissionsAsync(UpdateRolePermissionsDto dto, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null)
        {
            return ServiceResult.Failure("الدور غير موجود", ErrorCodes.ValidationError);
        }

        if (role.Name != null && (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
        {
            return ServiceResult.Failure("دور المسؤول الرئيسي يمتلك كافة الصلاحيات دائماً ولا يمكن تقييده", ErrorCodes.ValidationError);
        }

        var existingClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var c in existingClaims.Where(c => c.Type == Permissions.ClaimType))
        {
            await _roleManager.RemoveClaimAsync(role, c);
        }

        var validPermissionCodes = AppPermissions.GetAllPermissionCodes().ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var p in dto.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (validPermissionCodes.Contains(p))
            {
                await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, p));
            }
        }

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UserPermissionsDto>> GetUserPermissionsAsync(string userId, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, ct);
        if (user == null)
        {
            return ServiceResult<UserPermissionsDto>.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var directClaims = await _userManager.GetClaimsAsync(user);
        bool isCustomized = directClaims.Any(c => c.Type == "Permissions.Customized" && c.Value == "true");

        var directPerms = directClaims
            .Where(c => c.Type == Permissions.ClaimType)
            .Select(c => c.Value)
            .OrderBy(p => p)
            .ToList();

        var rolePerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var rClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var c in rClaims.Where(rc => rc.Type == Permissions.ClaimType))
                {
                    if (!string.IsNullOrWhiteSpace(c.Value))
                    {
                        rolePerms.Add(c.Value);
                    }
                }
            }
        }

        var effectivePerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool isAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));

        if (isAdmin)
        {
            foreach (var code in AppPermissions.GetAllPermissionCodes())
            {
                effectivePerms.Add(code);
            }
        }
        else if (isCustomized)
        {
            // المستخدم لديه صلاحيات مخصصة مستقلة
            foreach (var p in directPerms)
            {
                effectivePerms.Add(p);
            }
        }
        else
        {
            // المستخدم يرث صلاحيات أدواره المسندة
            foreach (var p in rolePerms)
            {
                effectivePerms.Add(p);
            }
            foreach (var p in directPerms)
            {
                effectivePerms.Add(p);
            }
        }

        return ServiceResult<UserPermissionsDto>.Success(new UserPermissionsDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Roles = roles.ToList(),
            IsCustomized = isCustomized,
            RolePermissions = rolePerms.OrderBy(p => p).ToList(),
            DirectPermissions = directPerms,
            EffectivePermissions = effectivePerms.OrderBy(p => p).ToList()
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateUserPermissionsAsync(UpdateUserPermissionsDto dto, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId && u.TenantId == tenantId, ct);
        if (user == null)
        {
            return ServiceResult.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        var existingClaims = await _userManager.GetClaimsAsync(user);
        foreach (var c in existingClaims.Where(c => c.Type == Permissions.ClaimType || c.Type == "Permissions.Customized"))
        {
            await _userManager.RemoveClaimAsync(user, c);
        }

        if (dto.IsCustomized)
        {
            // تسجيل شارة أن المستخدم يمتلك صلاحيات مخصصة ومستقلة عن الأدوار
            await _userManager.AddClaimAsync(user, new Claim("Permissions.Customized", "true"));

            var validCodes = AppPermissions.GetAllPermissionCodes().ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var p in dto.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (validCodes.Contains(p))
                {
                    await _userManager.AddClaimAsync(user, new Claim(Permissions.ClaimType, p));
                }
            }
        }

        return ServiceResult.Success();
    }
}
