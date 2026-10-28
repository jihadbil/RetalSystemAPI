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
    /// <summary>
    /// تهيئة مستودع مرتجعات المبيعات وربطه بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public SalesReturnRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(SalesReturnItem item, CancellationToken ct = default)
    {
        // إضافة بند جديد إلى جدول بنود مرتجعات المبيعات
        await _context.Set<SalesReturnItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<SalesReturnItem> items, CancellationToken ct = default)
    {
        // إضافة قائمة بنود المرتجع دفعة واحدة
        await _context.Set<SalesReturnItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(SalesReturnItem item)
    {
        // إزالة بند المرتجع نهائياً من السياق
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(SalesReturnItem item)
    {
        // حذف بند المرتجع نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SalesReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default)
    {
        // استعلام بنود المرتجع كقراءة فقط بدون تتبع
        return await _context.Set<SalesReturnItem>()
            .AsNoTracking()
            .Where(i => i.SalesReturnId == returnId)
            .ToListAsync(ct);
    }
}
