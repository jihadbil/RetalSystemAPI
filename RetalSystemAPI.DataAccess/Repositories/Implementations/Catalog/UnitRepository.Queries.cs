using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class UnitRepository
{
    public async Task<IReadOnlyList<UnitLookupRow>> GetLookupAsync(CancellationToken ct = default)
    {
        return await _context.Set<Unit>().AsNoTracking().Select(e => new UnitLookupRow(e.Id, e.Name)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Unit>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default)
    {
        return await ImportQueryBatch.ReadAsync(keys, (values, ids) => _context.Set<Unit>().Where(e => ids.Contains(e.Id) || values.Contains(e.Name.Trim().ToUpper())), ct);
    }

    public async Task<IReadOnlyList<Unit>> ListLookupAsync(CancellationToken ct = default)
    {
        return await _context.Set<Unit>().AsNoTracking().OrderBy(e => e.Id).ToListAsync(ct);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.Set<Unit>().AsNoTracking().AnyAsync(e => e.Name == name, ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Unit>().AsNoTracking().AnyAsync(e => e.Name == name && e.Id != id, ct);
    }
}
