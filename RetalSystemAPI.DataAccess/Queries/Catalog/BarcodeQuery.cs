namespace RetalSystemAPI.DataAccess.Queries;
public sealed record BarcodeQuery : PageRequest
{
    public string? Search { get; init; }
}
