using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Queries;
internal static class ImportQueryBatch
{
    internal const int Size = 500;
    public static async Task<IReadOnlyList<T>> ReadAsync<T>(ImportKeys keys, Func<string[], Guid[], IQueryable<T>> createQuery, CancellationToken ct)
        where T : BaseEntity
    {
        var values = keys.Values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim().ToUpperInvariant()).Distinct().ToArray();
        var ids = keys.Ids.Distinct().ToArray();
        var rows = new Dictionary<Guid, T>();
        for (var offset = 0; offset < Math.Max(values.Length, ids.Length); offset += Size)
        {
            var batch = await createQuery(values.Skip(offset).Take(Size).ToArray(), ids.Skip(offset).Take(Size).ToArray()).ToListAsync(ct);
            foreach (var row in batch)
                rows[row.Id] = row;
        }

        return rows.Values.ToArray();
    }

    public static Task<IReadOnlyList<T>> ByIdsAsync<T>(IEnumerable<Guid> ids, Func<Guid[], IQueryable<T>> createQuery, CancellationToken ct)
        where T : BaseEntity => ReadAsync(new ImportKeys(ids.Distinct().ToArray(), []), (_, batch) => createQuery(batch), ct);
}
