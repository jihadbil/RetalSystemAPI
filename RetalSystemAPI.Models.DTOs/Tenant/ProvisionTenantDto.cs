using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Tenant;

public sealed class ProvisionTenantDto
{
    public CreateTenantDto Tenant { get; set; } = new();
    [Required, MaxLength(150)]
    public string BranchName { get; set; } = null!;
    [MaxLength(500)]
    public string? BranchAddress { get; set; }
    [Required, MaxLength(256)]
    public string AdminUserName { get; set; } = null!;
    [Required, EmailAddress, MaxLength(256)]
    public string AdminEmail { get; set; } = null!;
    [Required, MinLength(10)]
    public string AdminPassword { get; set; } = null!;
}

public sealed class ProvisionTenantResponseDto
{
    public Guid TenantId { get; init; }
    public Guid BranchId { get; init; }
    public string AdminUserId { get; init; } = null!;
}
