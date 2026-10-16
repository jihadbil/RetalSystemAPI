using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class StockAdjustmentRepository
{
    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<StockAdjustmentItem>().AsNoTracking().AnyAsync(e => e.ProductId == id, ct);
    }

    public async Task<StockAdjustment?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StockAdjustment>.GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<StockAdjustment?> GetByNumberAsync(string adjustmentNumber, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StockAdjustment>.GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(adjustmentNumber)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<StockAdjustment>> ListAsync(StockAdjustmentQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StockAdjustment>.GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(request.WarehouseId, request.Reason, request.FromDate, request.ToDate, request.Search)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<StockAdjustment>> SearchAsync(StockAdjustmentQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<StockAdjustment>.GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(request.WarehouseId, request.Reason, request.FromDate, request.ToDate, request.Search)), request, ct);
    }

    public async Task<bool> NumberExistsAsync(string adjustmentNumber, CancellationToken ct = default)
    {
        return await _context.Set<StockAdjustment>().AsNoTracking().AnyAsync(e => e.AdjustmentNumber == adjustmentNumber, ct);
    }

    public async Task<StockAdjustment?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StockAdjustment>.GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(id)).FirstOrDefaultAsync(ct);
    }
}
