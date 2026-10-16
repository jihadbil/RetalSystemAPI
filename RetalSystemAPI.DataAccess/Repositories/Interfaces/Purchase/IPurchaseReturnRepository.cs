using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
/// <summary>
/// واجهة مستودع بيانات فواتير مرتجع المشتريات (PurchaseReturns) وبنودها كوحدة متكاملة.
/// </summary>
public partial interface IPurchaseReturnRepository : IRepository<PurchaseReturn>
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
}
