using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات أوامر التحويل المخزني وبنودها.
/// </summary>
public partial class StockTransferRepository : Repository<StockTransfer>, IStockTransferRepository
{
    public StockTransferRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(StockTransferItem item, CancellationToken ct = default)
    {
        await _context.Set<StockTransferItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<StockTransferItem> items, CancellationToken ct = default)
    {
        await _context.Set<StockTransferItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(StockTransferItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(StockTransferItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockTransferItem>> GetItemsAsync(Guid transferId, CancellationToken ct = default)
    {
        return await _context.Set<StockTransferItem>().AsNoTracking().Where(i => i.StockTransferId == transferId).ToListAsync(ct);
    }
}
