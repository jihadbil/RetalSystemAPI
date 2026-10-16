using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات فواتير مرتجع المشتريات وبنودها.
/// </summary>
public partial class PurchaseReturnRepository : Repository<PurchaseReturn>, IPurchaseReturnRepository
{
    public PurchaseReturnRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(PurchaseReturnItem item, CancellationToken ct = default)
    {
        await _context.Set<PurchaseReturnItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<PurchaseReturnItem> items, CancellationToken ct = default)
    {
        await _context.Set<PurchaseReturnItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(PurchaseReturnItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(PurchaseReturnItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturnItem>().AsNoTracking().Where(i => i.PurchaseReturnId == returnId).ToListAsync(ct);
    }
}
