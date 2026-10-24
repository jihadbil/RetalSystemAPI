namespace RetalSystemAPI.DataAccess.Queries;
public sealed record SupplierQuery : PageRequest
{
    public bool? HasDebt { get; init; }
    public bool? IsActive { get; init; }
    public string? Search { get; init; }
    public bool HasBalance { get; init; }
}
