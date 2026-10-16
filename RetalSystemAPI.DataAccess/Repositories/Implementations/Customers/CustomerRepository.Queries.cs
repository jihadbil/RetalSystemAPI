using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class CustomerRepository
{
    public async Task<Customer?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Customer>.GetQuery(_context.Set<Customer>(), new CustomerWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> ListAsync(CustomerQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Customer>.GetQuery(_context.Set<Customer>(), new CustomerWithDetailsSpec(request.Type, request.IsActive, request.Search)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<Customer>> SearchAsync(CustomerQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<Customer>.GetQuery(_context.Set<Customer>(), new CustomerWithDetailsSpec(request.Type, request.IsActive, request.Search)), request, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        return await _context.Set<Customer>().AsNoTracking().AnyAsync(e => e.Code == code, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Customer>().AsNoTracking().AnyAsync(e => e.Code == code && e.Id != id, ct);
    }
}
