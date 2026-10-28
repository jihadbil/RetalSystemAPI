using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات فواتير المبيعات (SalesInvoices) وبنودها كوحدة متكاملة.
/// </summary>
public partial interface ISalesInvoiceRepository : IRepository<SalesInvoice>
{
    /// <summary>
    /// إضافة بند جديد لفاتورة المبيعات.
    /// </summary>
    /// <param name="item">كيان بند الفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemAsync(SalesInvoiceItem item, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة بنود لفاتورة المبيعات دفعة واحدة.
    /// </summary>
    /// <param name="items">قائمة بنود الفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemsAsync(IEnumerable<SalesInvoiceItem> items, CancellationToken ct = default);

    /// <summary>
    /// إزالة بند من السياق وقاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه.</param>
    void RemoveItem(SalesInvoiceItem item);

    /// <summary>
    /// حذف نهائي وفيزيائي لبند فاتورة المبيعات من قاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه نهائياً.</param>
    void HardDeleteItem(SalesInvoiceItem item);

    /// <summary>
    /// جلب بنود فاتورة مبيعات محددة كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="invoiceId">معرف الفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بنود الفاتورة.</returns>
    Task<IReadOnlyList<SalesInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default);
}
