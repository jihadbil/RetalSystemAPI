using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class StockTransferRepository
{
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> barcodeIds, CancellationToken ct = default)
    {
        var ids = barcodeIds.Distinct().ToArray();
        return await _context.Set<StockTransferItem>().AsNoTracking().AnyAsync(i => i.ProductBarCodeId.HasValue && ids.Contains(i.ProductBarCodeId.Value), ct);
    }

    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<StockTransferItem>().AsNoTracking().AnyAsync(e => e.ProductId == id, ct);
    }

    public async Task<StockTransfer?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StockTransfer>.GetQuery(_context.Set<StockTransfer>(), new StockTransferWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<StockTransfer?> GetByNumberAsync(string transferNumber, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StockTransfer>.GetQuery(_context.Set<StockTransfer>(), new StockTransferWithDetailsSpec(transferNumber)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<StockTransfer>> ListAsync(StockTransferQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StockTransfer>.GetQuery(_context.Set<StockTransfer>(), new StockTransferListSpec(request.FromWarehouseId, request.ToWarehouseId, request.Status, request.FromDate, request.ToDate, request.Search)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<StockTransfer>> SearchAsync(StockTransferQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<StockTransfer>.GetQuery(_context.Set<StockTransfer>(), new StockTransferListSpec(request.FromWarehouseId, request.ToWarehouseId, request.Status, request.FromDate, request.ToDate, request.Search)), request, ct);
    }

    public async Task<bool> NumberExistsAsync(string transferNumber, CancellationToken ct = default)
    {
        return await _context.Set<StockTransfer>().AsNoTracking().AnyAsync(e => e.TransferNumber == transferNumber, ct);
    }

    public async Task<StockTransfer?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StockTransfer>.GetQuery(_context.Set<StockTransfer>(), new StockTransferWithDetailsSpec(id)).FirstOrDefaultAsync(ct);
    }
}
