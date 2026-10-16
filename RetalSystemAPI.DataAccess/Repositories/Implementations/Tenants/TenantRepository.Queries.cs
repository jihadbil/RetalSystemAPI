using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class TenantRepository
{
    public async Task<IReadOnlyList<Tenant>> ListLookupAsync(CancellationToken ct = default)
    {
        return await _context.Set<Tenant>().AsNoTracking().OrderBy(e => e.Id).ToListAsync(ct);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.Set<Tenant>().AsNoTracking().AnyAsync(e => e.Name == name, ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Tenant>().AsNoTracking().AnyAsync(e => e.Name == name && e.Id != id, ct);
    }

    public Task<PageResult<Tenant>> SearchAsync(TenantQuery request, CancellationToken ct = default) => QueryPaging.ReadAsync(_context.Tenants.OrderByDescending(t => t.CreatedAt).ThenBy(t => t.Id), request, ct);
}
