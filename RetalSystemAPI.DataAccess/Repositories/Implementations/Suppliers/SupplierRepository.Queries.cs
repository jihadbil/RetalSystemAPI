using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Suppliers;

using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class SupplierRepository
{
    public async Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _context.CurrentTenantId;
        return await _context.PurchaseInvoices.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.SupplierId == id, ct)
            || await _context.PurchaseOrders.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.SupplierId == id, ct)
            || await _context.PurchaseReturns.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.SupplierId == id, ct)
            || await _context.SupplierTransactions.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.SupplierId == id, ct);
    }

    public async Task<Supplier?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Supplier>.GetQuery(_context.Set<Supplier>(), new SupplierWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Supplier>> ListAsync(SupplierQuery request, CancellationToken ct = default)
    {
        return await BuildList(request).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    private IQueryable<Supplier> BuildList(SupplierQuery request)
    {
        var query = SpecificationEvaluator<Supplier>.GetQuery(_context.Set<Supplier>(), new SupplierWithDetailsSpec(request.Search));
        if (request.HasDebt.HasValue) query = query.Where(x => (x.CurrentBalance > 0) == request.HasDebt.Value);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        return query;
    }

    public async Task<BalanceSummary> GetBalanceSummaryAsync(SupplierQuery request, CancellationToken ct = default)
        => await BuildList(request).AsNoTracking().GroupBy(x => 1).Select(g => new BalanceSummary
        {
            TotalDebt = g.Sum(x => x.CurrentBalance > 0 ? x.CurrentBalance : 0),
            TotalCredit = g.Sum(x => x.CurrentBalance < 0 ? -x.CurrentBalance : 0),
            WithDebt = g.Count(x => x.CurrentBalance > 0)
        }).FirstOrDefaultAsync(ct) ?? new();

    public async Task<PageResult<Supplier>> SearchAsync(SupplierQuery request, CancellationToken ct = default)
    {
        var query = SpecificationEvaluator<Supplier>.GetQuery(_context.Set<Supplier>(), new SupplierWithDetailsSpec(request.Search));
        if (request.IsActive.HasValue) query = query.Where(supplier => supplier.IsActive == request.IsActive.Value);
        if (request.HasBalance) query = query.Where(supplier => supplier.CurrentBalance > 0);
        return await QueryPaging.ReadAsync(query, request, ct);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.Set<Supplier>().AsNoTracking().AnyAsync(e => e.Name == name, ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Supplier>().AsNoTracking().AnyAsync(e => e.Name == name && e.Id != id, ct);
    }

    public async Task<bool> HasFinancialHistoryAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _context.CurrentTenantId;
        return await _context.PurchaseInvoices.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.SupplierId == id, ct)
            || await _context.PurchaseOrders.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.SupplierId == id, ct)
            || await _context.PurchaseReturns.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.SupplierId == id, ct)
            || await _context.SupplierTransactions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.SupplierId == id, ct);
    }
}
