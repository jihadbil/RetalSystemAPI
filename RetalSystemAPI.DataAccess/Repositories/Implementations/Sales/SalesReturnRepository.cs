using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات فواتير مرتجع المبيعات وبنودها.
/// </summary>
public partial class SalesReturnRepository : Repository<SalesReturn>, ISalesReturnRepository
{
    public SalesReturnRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(SalesReturnItem item, CancellationToken ct = default)
    {
        await _context.Set<SalesReturnItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<SalesReturnItem> items, CancellationToken ct = default)
    {
        await _context.Set<SalesReturnItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(SalesReturnItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(SalesReturnItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SalesReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturnItem>().AsNoTracking().Where(i => i.SalesReturnId == returnId).ToListAsync(ct);
    }
}
