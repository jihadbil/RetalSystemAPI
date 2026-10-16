using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Queries;
public sealed record CustomerQuery : PageRequest
{
    public CustomerType? Type { get; init; }
    public bool? IsActive { get; init; }
    public string? Search { get; init; }
}
