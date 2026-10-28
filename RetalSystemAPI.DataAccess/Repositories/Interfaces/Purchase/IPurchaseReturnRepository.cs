using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات فواتير مرتجع المشتريات (PurchaseReturns) وبنودها كوحدة متكاملة.
/// </summary>
public partial interface IPurchaseReturnRepository : IRepository<PurchaseReturn>
{
    /// <summary>
    /// إضافة بند لمرتجع المشتريات.
    /// </summary>
    /// <param name="item">كيان بند مرتجع المشتريات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemAsync(PurchaseReturnItem item, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة بنود لمرتجع المشتريات دفعة واحدة.
    /// </summary>
    /// <param name="items">قائمة بنود مرتجع المشتريات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemsAsync(IEnumerable<PurchaseReturnItem> items, CancellationToken ct = default);

    /// <summary>
    /// إزالة بند من السياق وقاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه.</param>
    void RemoveItem(PurchaseReturnItem item);

    /// <summary>
    /// حذف نهائي وفيزيائي لبند مرتجع مشتريات من قاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه نهائياً.</param>
    void HardDeleteItem(PurchaseReturnItem item);

    /// <summary>
    /// جلب بنود مرتجع مشتريات محدد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="returnId">معرف مرتجع المشتريات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بنود مرتجع المشتريات.</returns>
    Task<IReadOnlyList<PurchaseReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default);
}
