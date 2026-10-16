namespace RetalSystemAPI.DataAccess.QueryResults;
public sealed record PageResult<T>(IReadOnlyList<T> Items, int TotalCount);
