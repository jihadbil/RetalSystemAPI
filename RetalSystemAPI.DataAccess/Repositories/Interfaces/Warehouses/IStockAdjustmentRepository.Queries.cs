using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface IStockAdjustmentRepository
{
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);
    Task<StockAdjustment?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<StockAdjustment?> GetByNumberAsync(string adjustmentNumber, CancellationToken ct = default);
    Task<IReadOnlyList<StockAdjustment>> ListAsync(StockAdjustmentQuery request, CancellationToken ct = default);
    Task<PageResult<StockAdjustment>> SearchAsync(StockAdjustmentQuery request, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string adjustmentNumber, CancellationToken ct = default);
    Task<StockAdjustment?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
