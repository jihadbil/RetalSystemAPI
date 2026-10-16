using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات طلبات الشراء وبنودها.
/// </summary>
public partial class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(PurchaseOrderItem item, CancellationToken ct = default)
    {
        await _context.Set<PurchaseOrderItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<PurchaseOrderItem> items, CancellationToken ct = default)
    {
        await _context.Set<PurchaseOrderItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void UpdateItem(PurchaseOrderItem item)
    {
        EntityPersistence.Update(_context, item);
    }

    /// <inheritdoc/>
    public void RemoveItem(PurchaseOrderItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(PurchaseOrderItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseOrderItem>> GetItemsAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseOrderItem>().AsNoTracking().Where(i => i.PurchaseOrderId == orderId).ToListAsync(ct);
    }
}
