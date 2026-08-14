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
}

public class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid BranchId { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
