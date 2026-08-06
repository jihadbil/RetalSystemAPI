using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

public class TenantResponseDto : BaseDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string LogoUrl { get; set; } = null!;
    public bool IsActive { get; set; }
}
