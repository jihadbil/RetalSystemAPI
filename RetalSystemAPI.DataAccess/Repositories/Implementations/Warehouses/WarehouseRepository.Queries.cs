using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class WarehouseRepository
{
    public async Task<bool> HasDocumentReferencesAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _context.CurrentTenantId;
        // Historical documents, including soft-deleted ones, must keep their required navigation visible.
        return await _context.SalesInvoices.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.SalesReturns.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.PurchaseInvoices.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.PurchaseReturns.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.PurchaseOrders.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.StockAdjustments.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.StockTransfers.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && (x.FromWarehouseId == id || x.ToWarehouseId == id), ct);
    }

    public async Task<IReadOnlyList<Warehouse>> GetByTypeAsync(WarehouseType type, CancellationToken ct = default)
    {
        return await _context.Warehouses.AsNoTracking().Where(e => e.Type == type).OrderBy(e => e.Id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Warehouse>> ListLookupAsync(CancellationToken ct = default)
    {
        return await _context.Set<Warehouse>().AsNoTracking().OrderBy(e => e.Id).ToListAsync(ct);
    }

    public async Task<Warehouse?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Warehouse>.GetQuery(_context.Set<Warehouse>(), new WarehouseWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Warehouse>> ListAsync(WarehouseQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Warehouse>.GetQuery(_context.Set<Warehouse>(), new WarehouseWithDetailsSpec(request.BranchId, request.Type)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<Warehouse>> SearchAsync(WarehouseQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<Warehouse>.GetQuery(_context.Set<Warehouse>(), new WarehouseWithDetailsSpec(request.BranchId, request.Type)), request, ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid branchId, CancellationToken ct = default)
    {
        return await _context.Set<Warehouse>().AsNoTracking().AnyAsync(e => e.Name == name && e.BranchId == branchId, ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid branchId, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Warehouse>().AsNoTracking().AnyAsync(e => e.Name == name && e.BranchId == branchId && e.Id != id, ct);
    }

}
