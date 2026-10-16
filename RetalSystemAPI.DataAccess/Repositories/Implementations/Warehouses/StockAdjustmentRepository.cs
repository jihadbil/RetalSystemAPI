using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات تسويات الجرد المخزني وبنودها.
/// </summary>
public partial class StockAdjustmentRepository : Repository<StockAdjustment>, IStockAdjustmentRepository
{
    public StockAdjustmentRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(StockAdjustmentItem item, CancellationToken ct = default)
    {
        await _context.Set<StockAdjustmentItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<StockAdjustmentItem> items, CancellationToken ct = default)
    {
        await _context.Set<StockAdjustmentItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(StockAdjustmentItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(StockAdjustmentItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockAdjustmentItem>> GetItemsAsync(Guid adjustmentId, CancellationToken ct = default)
    {
        return await _context.Set<StockAdjustmentItem>().AsNoTracking().Where(i => i.StockAdjustmentId == adjustmentId).ToListAsync(ct);
    }
}
