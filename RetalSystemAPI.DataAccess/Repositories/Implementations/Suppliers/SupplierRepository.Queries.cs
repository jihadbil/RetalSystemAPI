using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class SupplierRepository
{
    public async Task<Supplier?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Supplier>.GetQuery(_context.Set<Supplier>(), new SupplierWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Supplier>> ListAsync(SupplierQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Supplier>.GetQuery(_context.Set<Supplier>(), new SupplierWithDetailsSpec(request.Search)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<Supplier>> SearchAsync(SupplierQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<Supplier>.GetQuery(_context.Set<Supplier>(), new SupplierWithDetailsSpec(request.Search)), request, ct);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.Set<Supplier>().AsNoTracking().AnyAsync(e => e.Name == name, ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Supplier>().AsNoTracking().AnyAsync(e => e.Name == name && e.Id != id, ct);
    }
}
