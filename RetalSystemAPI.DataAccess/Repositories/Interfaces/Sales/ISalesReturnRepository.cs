using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
/// <summary>
/// واجهة مستودع بيانات فواتير مرتجع المبيعات (SalesReturns) وبنودها كوحدة متكاملة.
/// </summary>
public partial interface ISalesReturnRepository : IRepository<SalesReturn>
{
    /// <summary>إضافة بند لمرتجع المبيعات</summary>
    Task AddItemAsync(SalesReturnItem item, CancellationToken ct = default);
    /// <summary>إضافة مجموعة بنود لمرتجع المبيعات</summary>
    Task AddItemsAsync(IEnumerable<SalesReturnItem> items, CancellationToken ct = default);
    /// <summary>إزالة بند من السياق المحلي</summary>
    void RemoveItem(SalesReturnItem item);
    /// <summary>حذف نهائي لبند مرتجع مبيعات</summary>
    void HardDeleteItem(SalesReturnItem item);
    /// <summary>جلب بنود مرتجع مبيعات محدد</summary>
    Task<IReadOnlyList<SalesReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default);
}
