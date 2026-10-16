using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
/// <summary>
/// واجهة مستودع بيانات فواتير المبيعات (SalesInvoices) وبنودها كوحدة متكاملة.
/// </summary>
public partial interface ISalesInvoiceRepository : IRepository<SalesInvoice>
{
    /// <summary>إضافة بند لفاتورة المبيعات</summary>
    Task AddItemAsync(SalesInvoiceItem item, CancellationToken ct = default);
    /// <summary>إضافة مجموعة بنود لفاتورة المبيعات</summary>
    Task AddItemsAsync(IEnumerable<SalesInvoiceItem> items, CancellationToken ct = default);
    /// <summary>إزالة بند من السياق المحلي</summary>
    void RemoveItem(SalesInvoiceItem item);
    /// <summary>حذف نهائي لبند فاتورة مبيعات</summary>
    void HardDeleteItem(SalesInvoiceItem item);
    /// <summary>جلب بنود فاتورة مبيعات محددة</summary>
    Task<IReadOnlyList<SalesInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default);
}
