using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
/// <summary>
/// واجهة مستودع بيانات فواتير المشتريات (PurchaseInvoices) وبنودها وتفاصيل استلام النكهات كوحدة متكاملة.
/// </summary>
public partial interface IPurchaseInvoiceRepository : IRepository<PurchaseInvoice>
{
    // ── بنود الفاتورة المجمعة (Invoice Items) ────────────────────
    /// <summary>إضافة بند لفاتورة الشراء</summary>
    Task AddItemAsync(PurchaseInvoiceItem item, CancellationToken ct = default);
    /// <summary>إضافة مجموعة بنود لفاتورة الشراء</summary>
    Task AddItemsAsync(IEnumerable<PurchaseInvoiceItem> items, CancellationToken ct = default);
    /// <summary>إزالة بند من السياق المحلي</summary>
    void RemoveItem(PurchaseInvoiceItem item);
    /// <summary>حذف نهائي لبند فاتورة شراء</summary>
    void HardDeleteItem(PurchaseInvoiceItem item);
    /// <summary>جلب بنود فاتورة شراء محددة</summary>
    Task<IReadOnlyList<PurchaseInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default);
    // ── تفاصيل استلام النكهات والباركودات (Item Breakdowns) ──────
    /// <summary>إضافة تفصيل استلام نكهة/باركود لبند الفاتورة</summary>
    Task AddBreakdownAsync(PurchaseInvoiceItemBreakdown breakdown, CancellationToken ct = default);
    /// <summary>إضافة مجموعة تفاصيل استلام نكهات/باركودات</summary>
    Task AddBreakdownsAsync(IEnumerable<PurchaseInvoiceItemBreakdown> breakdowns, CancellationToken ct = default);
    /// <summary>إزالة تفصيل استلام من السياق المحلي</summary>
    void RemoveBreakdown(PurchaseInvoiceItemBreakdown breakdown);
    /// <summary>حذف نهائي لتفصيل استلام نكهة/باركود</summary>
    void HardDeleteBreakdown(PurchaseInvoiceItemBreakdown breakdown);
}
