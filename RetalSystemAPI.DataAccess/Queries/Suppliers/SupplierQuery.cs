namespace RetalSystemAPI.DataAccess.Queries;
public sealed record SupplierQuery : PageRequest
{
    public string? Search { get; init; }
}
