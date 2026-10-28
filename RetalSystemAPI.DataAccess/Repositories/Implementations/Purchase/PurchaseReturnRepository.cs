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
    /// <summary>
    /// تهيئة مستودع مرتجعات المشتريات وربطه بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public PurchaseReturnRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(PurchaseReturnItem item, CancellationToken ct = default)
    {
        // إضافة بند جديد في جدول بنود مرتجعات المشتريات
        await _context.Set<PurchaseReturnItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<PurchaseReturnItem> items, CancellationToken ct = default)
    {
        // إضافة قائمة بنود المرتجع دفعة واحدة
        await _context.Set<PurchaseReturnItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(PurchaseReturnItem item)
    {
        // إزالة بند المرتجع نهائياً من السياق
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(PurchaseReturnItem item)
    {
        // حذف بند المرتجع نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default)
    {
        // استعلام بنود المرتجع كقراءة فقط بدون تتبع
        return await _context.Set<PurchaseReturnItem>()
            .AsNoTracking()
            .Where(i => i.PurchaseReturnId == returnId)
            .ToListAsync(ct);
    }
}
