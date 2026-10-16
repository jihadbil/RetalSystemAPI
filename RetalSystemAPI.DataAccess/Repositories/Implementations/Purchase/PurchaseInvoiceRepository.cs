using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات فواتير المشتريات وبنودها وتفاصيل النكهات المستلمة.
/// </summary>
public partial class PurchaseInvoiceRepository : Repository<PurchaseInvoice>, IPurchaseInvoiceRepository
{
    public PurchaseInvoiceRepository(AppDbContext context) : base(context)
    {
    }

    // ── بنود الفاتورة المجمعة (Invoice Items) ────────────────────
    /// <inheritdoc/>
    public async Task AddItemAsync(PurchaseInvoiceItem item, CancellationToken ct = default)
    {
        await _context.Set<PurchaseInvoiceItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<PurchaseInvoiceItem> items, CancellationToken ct = default)
    {
        await _context.Set<PurchaseInvoiceItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(PurchaseInvoiceItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(PurchaseInvoiceItem item)
    {
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoiceItem>().AsNoTracking().Where(i => i.PurchaseInvoiceId == invoiceId).ToListAsync(ct);
    }

    // ── تفاصيل استلام النكهات والباركودات (Item Breakdowns) ──────
    /// <inheritdoc/>
    public async Task AddBreakdownAsync(PurchaseInvoiceItemBreakdown breakdown, CancellationToken ct = default)
    {
        await _context.Set<PurchaseInvoiceItemBreakdown>().AddAsync(breakdown, ct);
    }

    /// <inheritdoc/>
    public async Task AddBreakdownsAsync(IEnumerable<PurchaseInvoiceItemBreakdown> breakdowns, CancellationToken ct = default)
    {
        await _context.Set<PurchaseInvoiceItemBreakdown>().AddRangeAsync(breakdowns, ct);
    }

    /// <inheritdoc/>
    public void RemoveBreakdown(PurchaseInvoiceItemBreakdown breakdown)
    {
        EntityPersistence.HardDelete(_context, breakdown);
    }

    /// <inheritdoc/>
    public void HardDeleteBreakdown(PurchaseInvoiceItemBreakdown breakdown)
    {
        EntityPersistence.HardDelete(_context, breakdown);
    }
}
