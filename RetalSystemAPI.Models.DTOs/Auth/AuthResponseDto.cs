namespace RetalSystemAPI.Models.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
}
