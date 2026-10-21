using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class BranchRepository
{
    public async Task<bool> HasOperationalHistoryAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _context.CurrentTenantId;
        return await _context.Warehouses.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.Safes.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.PurchaseOrders.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.Expenses.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.CustomerTransactions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.SupplierTransactions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.PosTerminals.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.CashSessions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct);
    }

    public async Task<bool> IsActiveForTenantAsync(Guid branchId, Guid currentTenantId, CancellationToken ct = default)
    {
        return await _context.Set<Branch>().AsNoTracking().AnyAsync(e => e.Id == branchId && e.TenantId == currentTenantId && e.IsActive, ct);
    }

    public async Task<Branch?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Branch>.GetQuery(_context.Set<Branch>(), new BranchWithPhonesSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Branch>> ListAsync(BranchQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Branch>.GetQuery(_context.Set<Branch>(), new BranchWithPhonesSpec()).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<Branch>> SearchAsync(BranchQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<Branch>.GetQuery(_context.Set<Branch>(), new BranchWithPhonesSpec()), request, ct);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.Set<Branch>().AsNoTracking().AnyAsync(e => e.Name == name, ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Branch>().AsNoTracking().AnyAsync(e => e.Name == name && e.Id != id, ct);
    }

    public async Task<Branch?> GetDetailsForDeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Branch>.GetQuery(_context.Set<Branch>(), new BranchWithPhonesAndUsersSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }
}
