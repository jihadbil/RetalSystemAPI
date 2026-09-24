using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات فواتير المبيعات (SalesInvoices) وبنودها كوحدة متكاملة.
/// </summary>
public interface ISalesInvoiceRepository : IRepository<SalesInvoice>
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

    /// <summary>البحث عن بنود فواتير مبيعات مع تتبع التغييرات</summary>
    Task<IReadOnlyList<SalesInvoiceItem>> FindItemsTrackedAsync(Expression<Func<SalesInvoiceItem, bool>> predicate, CancellationToken ct = default);

    /// <summary>التحقق من وجود بند فاتورة مبيعات مطابق لشرط</summary>
    Task<bool> ItemExistsAsync(Expression<Func<SalesInvoiceItem, bool>> predicate, CancellationToken ct = default);
}
