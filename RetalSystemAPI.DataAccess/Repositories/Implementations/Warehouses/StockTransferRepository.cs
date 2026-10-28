using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات أوامر التحويل المخزني وبنودها.
/// </summary>
public partial class StockTransferRepository : Repository<StockTransfer>, IStockTransferRepository
{
    /// <summary>
    /// تهيئة نسخة جديدة من مستودع أوامر التحويل المخزني.
    /// </summary>
    /// <param name="context">سياق قاعدة بيانات النظام للوصول إلى الجداول وحفظ الحركات.</param>
    public StockTransferRepository(AppDbContext context) : base(context)
    {
        // تمرير سياق البيانات إلى الفئة الأساسية
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(StockTransferItem item, CancellationToken ct = default)
    {
        // إضافة بند جديد إلى أمر التحويل المخزني
        await _context.Set<StockTransferItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<StockTransferItem> items, CancellationToken ct = default)
    {
        // إضافة مجموعة بنود دفعة واحدة إلى أمر التحويل المخزني
        await _context.Set<StockTransferItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void RemoveItem(StockTransferItem item)
    {
        // إزالة بند التحويل نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(StockTransferItem item)
    {
        // الحذف الفيزيائي لبند التحويل من قاعدة البيانات
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockTransferItem>> GetItemsAsync(Guid transferId, CancellationToken ct = default)
    {
        // استرجاع بنود أمر التحويل المخزني كقراءة فقط دون تتبع للتغييرات
        return await _context.Set<StockTransferItem>()
            .AsNoTracking()
            .Where(i => i.StockTransferId == transferId)
            .ToListAsync(ct);
    }
}
