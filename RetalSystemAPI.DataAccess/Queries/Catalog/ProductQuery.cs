namespace RetalSystemAPI.DataAccess.Queries;
public sealed record ProductQuery : PageRequest
{
    public Guid? CategoryId { get; init; }
    public string? Search { get; init; }
}
