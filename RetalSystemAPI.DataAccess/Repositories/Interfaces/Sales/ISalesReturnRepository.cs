using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات فواتير مرتجع المبيعات (SalesReturns) وبنودها كوحدة متكاملة.
/// </summary>
public partial interface ISalesReturnRepository : IRepository<SalesReturn>
{
    /// <summary>
    /// إضافة بند لمرتجع المبيعات.
    /// </summary>
    /// <param name="item">كيان بند المرتجع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemAsync(SalesReturnItem item, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة بنود لمرتجع المبيعات دفعة واحدة.
    /// </summary>
    /// <param name="items">قائمة بنود المرتجع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemsAsync(IEnumerable<SalesReturnItem> items, CancellationToken ct = default);

    /// <summary>
    /// إزالة بند مرتجع من السياق المحلي وقاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه.</param>
    void RemoveItem(SalesReturnItem item);

    /// <summary>
    /// حذف نهائي وفيزيائي لبند مرتجع مبيعات من قاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه نهائياً.</param>
    void HardDeleteItem(SalesReturnItem item);

    /// <summary>
    /// جلب بنود مرتجع مبيعات محدد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="returnId">معرف المرتجع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بنود المرتجع.</returns>
    Task<IReadOnlyList<SalesReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default);
}
