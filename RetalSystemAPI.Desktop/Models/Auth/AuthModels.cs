using System.Text.Json.Serialization;

namespace RetalSystemAPI.Desktop.Models.Auth;

public class LoginRequest
{
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("tenantId")]
    public Guid TenantId { get; set; }

    [JsonPropertyName("roles")]
    public System.Collections.Generic.List<string> Roles { get; set; } = new();

    [JsonPropertyName("permissions")]
    public System.Collections.Generic.List<string> Permissions { get; set; } = new();
}
