using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Models.DTOs.Users;

public class UserSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public string? BranchName { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class UserDetailsDto : UserSummaryDto
{
    public List<string> DirectPermissions { get; set; } = new();
    public List<string> EffectivePermissions { get; set; } = new();
}

public class CreateUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public string? Role { get; set; }
    public List<string>? CustomPermissions { get; set; }
}

public class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid BranchId { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string>? CustomPermissions { get; set; }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class PermissionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsScreenAccess { get; set; }
}

public class RolePermissionsDto
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class UpdateRolePermissionsDto
{
    public string RoleId { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class UserPermissionsDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsCustomized { get; set; }
    public List<string> RolePermissions { get; set; } = new();
    public List<string> DirectPermissions { get; set; } = new();
    public List<string> EffectivePermissions { get; set; } = new();
}

public class UpdateUserPermissionsDto
{
    public string UserId { get; set; } = string.Empty;
    public bool IsCustomized { get; set; }
    public List<string> Permissions { get; set; } = new();
}
