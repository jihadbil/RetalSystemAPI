namespace RetalSystemAPI.Models;

/// <summary>A username and an email share one global, normalized login namespace.</summary>
public sealed class UserLoginIdentifier
{
    public required string NormalizedValue { get; set; }
    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
