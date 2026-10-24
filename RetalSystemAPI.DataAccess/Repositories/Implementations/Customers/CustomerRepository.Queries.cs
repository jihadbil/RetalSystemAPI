using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Customers;

using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class CustomerRepository
{
    public Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default) => HasFinancialHistoryAsync(id, ct);

    public async Task<bool> HasFinancialHistoryAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _context.CurrentTenantId;
        return await _context.SalesInvoices.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.CustomerId == id, ct)
            || await _context.SalesReturns.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.CustomerId == id, ct)
            || await _context.CustomerTransactions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.CustomerId == id, ct);
    }

    public async Task<Customer?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Customer>.GetQuery(_context.Set<Customer>(), new CustomerWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> ListAsync(CustomerQuery request, CancellationToken ct = default)
    {
        return await BuildList(request).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    private IQueryable<Customer> BuildList(CustomerQuery request)
    {
        var query = SpecificationEvaluator<Customer>.GetQuery(_context.Set<Customer>(), new CustomerWithDetailsSpec(request.Type, request.IsActive, request.Search));
        if (request.HasDebt.HasValue) query = query.Where(x => (x.CurrentBalance > 0) == request.HasDebt.Value);
        return query;
    }

    public async Task<BalanceSummary> GetBalanceSummaryAsync(CustomerQuery request, CancellationToken ct = default)
        => await BuildList(request).AsNoTracking().GroupBy(x => 1).Select(g => new BalanceSummary
        {
            TotalDebt = g.Sum(x => x.CurrentBalance > 0 ? x.CurrentBalance : 0),
            TotalCredit = g.Sum(x => x.CurrentBalance < 0 ? -x.CurrentBalance : 0),
            WithDebt = g.Count(x => x.CurrentBalance > 0)
        }).FirstOrDefaultAsync(ct) ?? new();

    public async Task<PageResult<Customer>> SearchAsync(CustomerQuery request, CancellationToken ct = default)
    {
        var query = BuildList(request);
        if (request.HasBalance) query = query.Where(customer => customer.CurrentBalance > 0);
        return await QueryPaging.ReadAsync(query, request, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        return await _context.Set<Customer>().AsNoTracking().AnyAsync(e => e.Code != null && e.Code.Trim() == code, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Customer>().AsNoTracking().AnyAsync(e => e.Code != null && e.Code.Trim() == code && e.Id != id, ct);
    }
}
