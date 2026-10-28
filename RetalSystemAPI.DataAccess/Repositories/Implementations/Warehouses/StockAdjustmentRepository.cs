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
    /// <summary>
    /// تهيئة نسخة جديدة من مستودع تسويات الجرد المخزني.
    /// </summary>
    /// <param name="context">سياق قاعدة بيانات النظام للوصول إلى الجداول وحفظ الحركات.</param>
    public StockAdjustmentRepository(AppDbContext context) : base(context)
    {
        // تمرير سياق البيانات إلى الفئة الأساسية
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(StockAdjustmentItem item, CancellationToken ct = default)
    {
        // إضافة بند جديد إلى تسوية الجرد المخزني
        await _context.Set<StockAdjustmentItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<StockAdjustmentItem> items, CancellationToken ct = default)
    {
        // إضافة دفعة من البنود إلى تسوية الجرد دفعة واحدة
        await _context.Set<StockAdjustmentItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(StockAdjustmentItem item)
    {
        // إزالة بند التسوية نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(StockAdjustmentItem item)
    {
        // الحذف الفيزيائي لبند التسوية من قاعدة البيانات
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockAdjustmentItem>> GetItemsAsync(Guid adjustmentId, CancellationToken ct = default)
    {
        // استرجاع بنود تسوية الجرد كقراءة فقط دون تتبع التغييرات
        return await _context.Set<StockAdjustmentItem>()
            .AsNoTracking()
            .Where(i => i.StockAdjustmentId == adjustmentId)
            .ToListAsync(ct);
    }
}
