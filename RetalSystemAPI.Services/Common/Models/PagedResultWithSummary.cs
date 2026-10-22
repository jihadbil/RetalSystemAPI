namespace RetalSystemAPI.Services.Common.Models;

public class PagedResult<T, TSummary> : PagedResult<T>
{
    public required TSummary Summary { get; init; }

    public static PagedResult<T, TSummary> Create(
        IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize, TSummary summary)
    {
        RetalSystemAPI.Models.Common.Pagination.Normalize(ref pageNumber, ref pageSize);
        return new()
        {
            Items = items, TotalCount = totalCount, PageNumber = pageNumber,
            PageSize = pageSize, Summary = summary
        };
    }
}
