namespace RetalSystemAPI.DataAccess.Queries;
public sealed record SupplierQuery : PageRequest
{
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
    public bool HasBalance { get; init; }
}
