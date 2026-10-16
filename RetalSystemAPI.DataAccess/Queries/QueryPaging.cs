using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Queries;
internal static class QueryPaging
{
    public static async Task<PageResult<T>> ReadAsync<T>(IQueryable<T> orderedQuery, PageRequest request, CancellationToken ct)
        where T : class
    {
        var page = request.PageNumber;
        var size = request.PageSize;
        Pagination.Normalize(ref page, ref size);
        var count = await orderedQuery.CountAsync(ct);
        var items = await orderedQuery.AsNoTracking().AsSplitQuery().Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return new PageResult<T>(items, count);
    }
}
