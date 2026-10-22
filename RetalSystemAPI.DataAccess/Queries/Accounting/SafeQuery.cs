namespace RetalSystemAPI.DataAccess.Queries;
public sealed record SafeQuery : PageRequest
{
    public RetalSystemAPI.Models.Enums.SafeType? Type { get; init; }
    public Guid? BranchId { get; init; }
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
}
