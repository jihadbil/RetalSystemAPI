using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات تسويات الجرد المخزني (StockAdjustments) وبنودها كوحدة متكاملة.
/// </summary>
public interface IStockAdjustmentRepository : IRepository<StockAdjustment>
{
    /// <summary>إضافة بند لتسوية الجرد</summary>
    Task AddItemAsync(StockAdjustmentItem item, CancellationToken ct = default);

    /// <summary>إضافة مجموعة بنود لتسوية الجرد</summary>
    Task AddItemsAsync(IEnumerable<StockAdjustmentItem> items, CancellationToken ct = default);

    /// <summary>إزالة بند من السياق المحلي</summary>
    void RemoveItem(StockAdjustmentItem item);

    /// <summary>حذف نهائي لبند تسوية جرد</summary>
    void HardDeleteItem(StockAdjustmentItem item);

    /// <summary>جلب بنود تسوية جرد محددة</summary>
    Task<IReadOnlyList<StockAdjustmentItem>> GetItemsAsync(Guid adjustmentId, CancellationToken ct = default);

    /// <summary>البحث عن بنود تسوية جرد مع تتبع التغييرات</summary>
    Task<IReadOnlyList<StockAdjustmentItem>> FindItemsTrackedAsync(Expression<Func<StockAdjustmentItem, bool>> predicate, CancellationToken ct = default);

    /// <summary>التحقق من وجود بند تسوية جرد مطابق لشرط</summary>
    Task<bool> ItemExistsAsync(Expression<Func<StockAdjustmentItem, bool>> predicate, CancellationToken ct = default);
}
