using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class StockRepository
{
    public async Task<IReadOnlyDictionary<Guid, long>> GetQuantitiesByProductAsync(IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        var ids = productIds.Distinct().ToArray();
        var showroom = await _context.Set<ShowroomStock>().AsNoTracking().Where(s => ids.Contains(s.ProductId))
            .GroupBy(s => s.ProductId).Select(g => new { ProductId = g.Key, Quantity = g.Sum(s => (long)s.Quantity) }).ToListAsync(ct);
        var storage = await _context.Set<StorgeStock>().AsNoTracking().Where(s => ids.Contains(s.ProductBarcode.ProductId))
            .GroupBy(s => s.ProductBarcode.ProductId).Select(g => new { ProductId = g.Key, Quantity = g.Sum(s => (long)s.Quantity) }).ToListAsync(ct);
        return showroom.Concat(storage).GroupBy(s => s.ProductId).ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity));
    }

    public async Task<IReadOnlyList<StockShowroomExportRow>> GetShowroomExportRowsAsync(CancellationToken ct = default)
    {
        return await _context.Set<ShowroomStock>().AsNoTracking().Select(e => new StockShowroomExportRow(e.ProductId, e.Quantity)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StockStorageExportRow>> GetStorageExportRowsAsync(CancellationToken ct = default)
    {
        return await _context.Set<StorgeStock>().AsNoTracking().Select(e => new StockStorageExportRow(e.ProductBarcodeId, e.Quantity)).ToListAsync(ct);
    }

    public async Task<bool> HasNonzeroStorageByBarcodesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<StorgeStock>().AsNoTracking().AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarcodeId) && e.Quantity != 0, ct);
    }

    public async Task<IReadOnlyList<StorgeStock>> GetStorageByBarcodesForUpdateAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<StorgeStock>().Where(e => removedBarcodeIds.Contains(e.ProductBarcodeId)).ToListAsync(ct);
    }

    public async Task<bool> HasNonzeroShowroomByProductAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ShowroomStock>().AsNoTracking().AnyAsync(e => e.ProductId == id && e.Quantity != 0, ct);
    }

    public async Task<IReadOnlyList<ShowroomStock>> GetShowroomByProductForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ShowroomStock>().Where(e => e.ProductId == id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StorgeStock>> GetStorageForUpdateAsync(Guid warehouseId, IEnumerable<Guid> barcodeIdList, CancellationToken ct = default)
    {
        return await ImportQueryBatch.ByIdsAsync(barcodeIdList, ids => _context.Set<StorgeStock>().Where(e => e.WarehouseId == warehouseId && ids.Contains(e.ProductBarcodeId)), ct);
    }

    public async Task<IReadOnlyList<ShowroomStock>> GetShowroomForUpdateAsync(Guid warehouseId, IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        return await ImportQueryBatch.ByIdsAsync(productIds, ids => _context.Set<ShowroomStock>().Where(e => e.WarehouseId == warehouseId && ids.Contains(e.ProductId)), ct);
    }

    public async Task<StorgeStock?> GetStorageDetailsAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StorgeStock>.GetQuery(_context.Set<StorgeStock>(), new StorgeStockWithDetailsSpec(warehouseId, productBarcodeId)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<StockStorageKeysRow>> GetStorageKeysAsync(Guid warehouseId, CancellationToken ct = default)
    {
        return await _context.Set<StorgeStock>().AsNoTracking().Where(e => e.WarehouseId == warehouseId).Select(e => new StockStorageKeysRow(e.WarehouseId, e.ProductBarcodeId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StorgeStock>> ListStorageAsync(StorageStockQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StorgeStock>.GetQuery(_context.Set<StorgeStock>(), new StorgeStockWithDetailsSpec(request.WarehouseId, request.SearchTerm, request.ExactBarcode)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<StorgeStock?> GetStorageAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        return await _context.Set<StorgeStock>().AsNoTracking().FirstOrDefaultAsync(e => e.WarehouseId == warehouseId && e.ProductBarcodeId == productBarcodeId, ct);
    }

    public async Task<IReadOnlyList<StorgeStock>> GetLowStorageAsync(Guid? warehouseId, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StorgeStock>.GetQuery(_context.Set<StorgeStock>(), new LowStorgeStockSpec(warehouseId)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<ShowroomStock?> GetShowroomDetailsAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ShowroomStock>.GetQuery(_context.Set<ShowroomStock>(), new ShowroomStockWithDetailsSpec(warehouseId, productId)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<StockShowroomKeysRow>> GetShowroomKeysAsync(Guid warehouseId, CancellationToken ct = default)
    {
        return await _context.Set<ShowroomStock>().AsNoTracking().Where(e => e.WarehouseId == warehouseId).Select(e => new StockShowroomKeysRow(e.WarehouseId, e.ProductId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ShowroomStock>> ListShowroomAsync(ShowroomStockQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ShowroomStock>.GetQuery(_context.Set<ShowroomStock>(), new ShowroomStockWithDetailsSpec(request.WarehouseId, request.SearchTerm, request.ExactBarcode)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<ShowroomStock?> GetShowroomAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ShowroomStock>().AsNoTracking().FirstOrDefaultAsync(e => e.WarehouseId == warehouseId && e.ProductId == productId, ct);
    }

    public async Task<IReadOnlyList<ShowroomStock>> GetLowShowroomAsync(Guid? warehouseId, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ShowroomStock>.GetQuery(_context.Set<ShowroomStock>(), new LowShowroomStockSpec(warehouseId)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<bool> HasPositiveStorageByWarehouseAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<StorgeStock>().AsNoTracking().AnyAsync(e => e.WarehouseId == id && e.Quantity > 0, ct);
    }

    public async Task<bool> HasPositiveShowroomByWarehouseAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ShowroomStock>().AsNoTracking().AnyAsync(e => e.WarehouseId == id && e.Quantity > 0, ct);
    }

    public Task<PageResult<StorgeStock>> SearchStorageAsync(StorageStockQuery request, CancellationToken ct = default)
    {
        var spec = new StorgeStockWithDetailsSpec(request.WarehouseId, request.SearchTerm, request.ExactBarcode);
        return QueryPaging.ReadAsync(SpecificationEvaluator<StorgeStock>.GetQuery(_context.Set<StorgeStock>(), spec), request, ct);
    }

    public Task<PageResult<ShowroomStock>> SearchShowroomAsync(ShowroomStockQuery request, CancellationToken ct = default)
    {
        var spec = new ShowroomStockWithDetailsSpec(request.WarehouseId, request.SearchTerm, request.ExactBarcode);
        return QueryPaging.ReadAsync(SpecificationEvaluator<ShowroomStock>.GetQuery(_context.Set<ShowroomStock>(), spec), request, ct);
    }
}
