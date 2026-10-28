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
    /// <summary>
    /// تهيئة مستودع فواتير المبيعات وربطه بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public SalesInvoiceRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(SalesInvoiceItem item, CancellationToken ct = default)
    {
        // إضافة بند الفاتورة إلى جدول بنود فواتير المبيعات
        await _context.Set<SalesInvoiceItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<SalesInvoiceItem> items, CancellationToken ct = default)
    {
        // إضافة قائمة بنود الفاتورة دفعة واحدة
        await _context.Set<SalesInvoiceItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(SalesInvoiceItem item)
    {
        // إزالة بند الفاتورة نهائياً من السياق
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(SalesInvoiceItem item)
    {
        // حذف بند الفاتورة نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SalesInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default)
    {
        // استعلام بنود الفاتورة كقراءة فقط بدون تتبع
        return await _context.Set<SalesInvoiceItem>()
            .AsNoTracking()
            .Where(i => i.SalesInvoiceId == invoiceId)
            .ToListAsync(ct);
    }
}
