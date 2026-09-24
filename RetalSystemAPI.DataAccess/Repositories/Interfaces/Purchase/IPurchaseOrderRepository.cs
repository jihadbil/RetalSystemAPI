using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات طلبات الشراء (PurchaseOrders) وبنودها كوحدة متكاملة.
/// </summary>
public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    /// <summary>إضافة بند لطلب الشراء</summary>
    Task AddItemAsync(PurchaseOrderItem item, CancellationToken ct = default);

    /// <summary>إضافة مجموعة بنود لطلب الشراء</summary>
    Task AddItemsAsync(IEnumerable<PurchaseOrderItem> items, CancellationToken ct = default);

    /// <summary>تحديث بيانات بند طلب الشراء</summary>
    void UpdateItem(PurchaseOrderItem item);

    /// <summary>إزالة بند من السياق المحلي</summary>
    void RemoveItem(PurchaseOrderItem item);

    /// <summary>حذف نهائي لبند طلب الشراء</summary>
    void HardDeleteItem(PurchaseOrderItem item);

    /// <summary>جلب بنود طلب شراء محدد</summary>
    Task<IReadOnlyList<PurchaseOrderItem>> GetItemsAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>البحث عن بنود طلبات الشراء مع تتبع التغييرات</summary>
    Task<IReadOnlyList<PurchaseOrderItem>> FindItemsTrackedAsync(Expression<Func<PurchaseOrderItem, bool>> predicate, CancellationToken ct = default);

    /// <summary>التحقق من وجود بند طلب شراء مطابق لشرط</summary>
    Task<bool> ItemExistsAsync(Expression<Func<PurchaseOrderItem, bool>> predicate, CancellationToken ct = default);
}
