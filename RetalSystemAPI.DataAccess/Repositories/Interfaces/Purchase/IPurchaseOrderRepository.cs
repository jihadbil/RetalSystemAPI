using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات طلبات وأوامر الشراء (PurchaseOrders) وبنودها كوحدة متكاملة.
/// </summary>
public partial interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    /// <summary>
    /// إضافة بند جديد لطلب الشراء.
    /// </summary>
    /// <param name="item">كيان بند طلب الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemAsync(PurchaseOrderItem item, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة بنود لطلب الشراء دفعة واحدة.
    /// </summary>
    /// <param name="items">قائمة بنود طلب الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemsAsync(IEnumerable<PurchaseOrderItem> items, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات بند طلب الشراء والكميات والأسعار التقديرية.
    /// </summary>
    /// <param name="item">كيان البند المحدث.</param>
    void UpdateItem(PurchaseOrderItem item);

    /// <summary>
    /// إزالة بند طلب شراء من السياق وقاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه.</param>
    void RemoveItem(PurchaseOrderItem item);

    /// <summary>
    /// حذف نهائي وفيزيائي لبند طلب الشراء من قاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه نهائياً.</param>
    void HardDeleteItem(PurchaseOrderItem item);

    /// <summary>
    /// جلب بنود طلب شراء محدد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="orderId">معرف طلب الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بنود طلب الشراء.</returns>
    Task<IReadOnlyList<PurchaseOrderItem>> GetItemsAsync(Guid orderId, CancellationToken ct = default);
}
