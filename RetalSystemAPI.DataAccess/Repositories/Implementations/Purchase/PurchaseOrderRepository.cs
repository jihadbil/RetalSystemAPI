using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات طلبات وأوامر الشراء وبنودها.
/// </summary>
public partial class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
{
    /// <summary>
    /// تهيئة مستودع أوامر الشراء وربطه بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public PurchaseOrderRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddItemAsync(PurchaseOrderItem item, CancellationToken ct = default)
    {
        // إضافة بند جديد إلى جدول بنود أوامر الشراء
        await _context.Set<PurchaseOrderItem>().AddAsync(item, ct);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(IEnumerable<PurchaseOrderItem> items, CancellationToken ct = default)
    {
        // إضافة قائمة بنود أمر الشراء دفعة واحدة
        await _context.Set<PurchaseOrderItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc/>
    public void UpdateItem(PurchaseOrderItem item)
    {
        // تحديث بند أمر الشراء مع تطبيق التحقق من التزامن
        EntityPersistence.Update(_context, item);
    }

    /// <inheritdoc/>
    public void RemoveItem(PurchaseOrderItem item)
    {
        // إزالة بند أمر الشراء نهائياً من السياق
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public void HardDeleteItem(PurchaseOrderItem item)
    {
        // حذف بند أمر الشراء نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, item);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseOrderItem>> GetItemsAsync(Guid orderId, CancellationToken ct = default)
    {
        // استعلام بنود أمر الشراء كقراءة فقط بدون تتبع
        return await _context.Set<PurchaseOrderItem>()
            .AsNoTracking()
            .Where(i => i.PurchaseOrderId == orderId)
            .ToListAsync(ct);
    }
}
