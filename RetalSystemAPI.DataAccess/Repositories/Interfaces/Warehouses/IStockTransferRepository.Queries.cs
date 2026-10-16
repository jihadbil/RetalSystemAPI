using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface IStockTransferRepository
{
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);
    Task<StockTransfer?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<StockTransfer?> GetByNumberAsync(string transferNumber, CancellationToken ct = default);
    Task<IReadOnlyList<StockTransfer>> ListAsync(StockTransferQuery request, CancellationToken ct = default);
    Task<PageResult<StockTransfer>> SearchAsync(StockTransferQuery request, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string transferNumber, CancellationToken ct = default);
    Task<StockTransfer?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
