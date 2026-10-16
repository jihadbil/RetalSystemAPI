namespace RetalSystemAPI.DataAccess.Queries;
public sealed record ExpenseCategoryQuery : PageRequest
{
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
}
