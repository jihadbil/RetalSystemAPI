using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface IStockRepository
{
    Task<IReadOnlyDictionary<Guid, long>> GetQuantitiesByProductAsync(IEnumerable<Guid> productIds, CancellationToken ct = default);
    Task<IReadOnlyList<StockShowroomExportRow>> GetShowroomExportRowsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<StockStorageExportRow>> GetStorageExportRowsAsync(CancellationToken ct = default);
    Task<bool> HasNonzeroStorageByBarcodesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);
    Task<IReadOnlyList<StorgeStock>> GetStorageByBarcodesForUpdateAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);
    Task<bool> HasNonzeroShowroomByProductAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ShowroomStock>> GetShowroomByProductForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<StorgeStock>> GetStorageForUpdateAsync(Guid warehouseId, IEnumerable<Guid> barcodeIdList, CancellationToken ct = default);
    Task<IReadOnlyList<ShowroomStock>> GetShowroomForUpdateAsync(Guid warehouseId, IEnumerable<Guid> productIds, CancellationToken ct = default);
    Task<StorgeStock?> GetStorageDetailsAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);
    Task<IReadOnlyList<StockStorageKeysRow>> GetStorageKeysAsync(Guid warehouseId, CancellationToken ct = default);
    Task<IReadOnlyList<StorgeStock>> ListStorageAsync(StorageStockQuery request, CancellationToken ct = default);
    Task<StorgeStock?> GetStorageAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);
    Task<IReadOnlyList<StorgeStock>> GetLowStorageAsync(Guid? warehouseId, CancellationToken ct = default);
    Task<ShowroomStock?> GetShowroomDetailsAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<StockShowroomKeysRow>> GetShowroomKeysAsync(Guid warehouseId, CancellationToken ct = default);
    Task<IReadOnlyList<ShowroomStock>> ListShowroomAsync(ShowroomStockQuery request, CancellationToken ct = default);
    Task<ShowroomStock?> GetShowroomAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<ShowroomStock>> GetLowShowroomAsync(Guid? warehouseId, CancellationToken ct = default);
    Task<bool> HasPositiveStorageByWarehouseAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasPositiveShowroomByWarehouseAsync(Guid id, CancellationToken ct = default);
    Task<PageResult<StorgeStock>> SearchStorageAsync(StorageStockQuery request, CancellationToken ct = default);
    Task<PageResult<ShowroomStock>> SearchShowroomAsync(ShowroomStockQuery request, CancellationToken ct = default);
}
