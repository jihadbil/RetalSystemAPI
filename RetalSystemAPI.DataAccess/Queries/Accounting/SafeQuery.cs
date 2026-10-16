namespace RetalSystemAPI.DataAccess.Queries;
public sealed record SafeQuery : PageRequest
{
    public Guid? BranchId { get; init; }
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
}
