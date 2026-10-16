using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات فواتير المبيعات وبنودها.
/// </summary>
public partial class SalesInvoiceRepository : Repository<SalesInvoice>, ISalesInvoiceRepository
{
    public SalesInvoiceRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(SalesInvoiceItem item, CancellationToken ct = default)
    {
        await _context.Set<SalesInvoiceItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<SalesInvoiceItem> items, CancellationToken ct = default)
    {
        await _context.Set<SalesInvoiceItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(SalesInvoiceItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(SalesInvoiceItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SalesInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default)
    {
        return await _context.Set<SalesInvoiceItem>().AsNoTracking().Where(i => i.SalesInvoiceId == invoiceId).ToListAsync(ct);
    }
}
