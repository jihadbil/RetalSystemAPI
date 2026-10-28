using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات فواتير المشتريات وبنودها وتفاصيل استلام النكهات.
/// </summary>
public partial class PurchaseInvoiceRepository : Repository<PurchaseInvoice>, IPurchaseInvoiceRepository
{
    /// <summary>
    /// تهيئة مستودع فواتير المشتريات وربطه بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public PurchaseInvoiceRepository(AppDbContext context) : base(context)
    {
    }

    // ── بنود الفاتورة المجمعة (Invoice Items) ────────────────────

    /// <inheritdoc/>
    public async Task AddItemAsync(PurchaseInvoiceItem item, CancellationToken ct = default)
    {
        // إضافة بند الفاتورة في جدول بنود فواتير المشتريات
        await _context.Set<PurchaseInvoiceItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<PurchaseInvoiceItem> items, CancellationToken ct = default)
    {
        // إضافة قائمة بنود الفاتورة دفعة واحدة
        await _context.Set<PurchaseInvoiceItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(PurchaseInvoiceItem item)
    {
        // إزالة بند الفاتورة نهائياً من السياق
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(PurchaseInvoiceItem item)
    {
        // حذف بند الفاتورة نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default)
    {
        // استعلام بنود الفاتورة كقراءة فقط بدون تتبع
        return await _context.Set<PurchaseInvoiceItem>()
            .AsNoTracking()
            .Where(i => i.PurchaseInvoiceId == invoiceId)
            .ToListAsync(ct);
    }

    // ── تفاصيل استلام النكهات والباركودات (Item Breakdowns) ──────

    /// <inheritdoc/>
    public async Task AddBreakdownAsync(PurchaseInvoiceItemBreakdown breakdown, CancellationToken ct = default)
    {
        // إضافة تفصيل استلام النكهة/الباركود للبند
        await _context.Set<PurchaseInvoiceItemBreakdown>().AddAsync(breakdown, ct);
    }

    /// <inheritdoc/>
    public async Task AddBreakdownsAsync(IEnumerable<PurchaseInvoiceItemBreakdown> breakdowns, CancellationToken ct = default)
    {
        // إضافة قائمة تفاصيل استلام النكهات دفعة واحدة
        await _context.Set<PurchaseInvoiceItemBreakdown>().AddRangeAsync(breakdowns, ct);
    }

    /// <inheritdoc/>
    public void RemoveBreakdown(PurchaseInvoiceItemBreakdown breakdown)
    {
        // إزالة تفصيل الاستلام نهائياً من السياق
        EntityPersistence.HardDelete(_context, breakdown);
    }

    /// <inheritdoc/>
    public void HardDeleteBreakdown(PurchaseInvoiceItemBreakdown breakdown)
    {
        // حذف تفصيل الاستلام نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, breakdown);
    }
}
