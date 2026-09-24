using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات فواتير مرتجع المشتريات (PurchaseReturns) وبنودها كوحدة متكاملة.
/// </summary>
public interface IPurchaseReturnRepository : IRepository<PurchaseReturn>
{
    /// <summary>إضافة بند لمرتجع المشتريات</summary>
    Task AddItemAsync(PurchaseReturnItem item, CancellationToken ct = default);

    /// <summary>إضافة مجموعة بنود لمرتجع المشتريات</summary>
    Task AddItemsAsync(IEnumerable<PurchaseReturnItem> items, CancellationToken ct = default);

    /// <summary>إزالة بند من السياق المحلي</summary>
    void RemoveItem(PurchaseReturnItem item);

    /// <summary>حذف نهائي لبند مرتجع مشتريات</summary>
    void HardDeleteItem(PurchaseReturnItem item);

    /// <summary>جلب بنود مرتجع مشتريات محدد</summary>
    Task<IReadOnlyList<PurchaseReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default);

    /// <summary>البحث عن بنود مرتجعات المشتريات بشرط منطقي</summary>
    Task<IReadOnlyList<PurchaseReturnItem>> FindItemsAsync(Expression<Func<PurchaseReturnItem, bool>> predicate, CancellationToken ct = default);

    /// <summary>البحث عن بنود مرتجعات المشتريات مع تتبع التغييرات</summary>
    Task<IReadOnlyList<PurchaseReturnItem>> FindItemsTrackedAsync(Expression<Func<PurchaseReturnItem, bool>> predicate, CancellationToken ct = default);

    /// <summary>التحقق من وجود بند مرتجع مشتريات مطابق لشرط</summary>
    Task<bool> ItemExistsAsync(Expression<Func<PurchaseReturnItem, bool>> predicate, CancellationToken ct = default);
}
