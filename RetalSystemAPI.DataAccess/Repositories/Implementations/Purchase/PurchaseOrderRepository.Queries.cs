using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class PurchaseOrderRepository
{
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseOrderItem>().AsNoTracking().AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarCodeId), ct);
    }

    public async Task<PurchaseOrder?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<PurchaseOrder>.GetQuery(_context.Set<PurchaseOrder>(), new PurchaseOrderWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> ListAsync(PurchaseOrderQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<PurchaseOrder>.GetQuery(_context.Set<PurchaseOrder>(), new PurchaseOrderListSpec(request.BranchId, request.WarehouseId, request.Status, request.Search)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<PurchaseOrder>> SearchAsync(PurchaseOrderQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<PurchaseOrder>.GetQuery(_context.Set<PurchaseOrder>(), new PurchaseOrderListSpec(request.BranchId, request.WarehouseId, request.Status, request.Search)), request, ct);
    }

    public async Task<bool> NumberExistsAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseOrder>().AsNoTracking().AnyAsync(e => e.OrderNumber == orderNumber, ct);
    }

    public async Task<PurchaseOrder?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<PurchaseOrder>.GetQuery(_context.Set<PurchaseOrder>(), new PurchaseOrderWithDetailsSpec(id)).FirstOrDefaultAsync(ct);
    }
}
