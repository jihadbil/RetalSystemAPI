using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات فواتير المشتريات (PurchaseInvoices) وبنودها وتفاصيل استلام النكهات كوحدة متكاملة.
/// </summary>
public partial interface IPurchaseInvoiceRepository : IRepository<PurchaseInvoice>
{
    // ── بنود الفاتورة المجمعة (Invoice Items) ────────────────────

    /// <summary>
    /// إضافة بند جديد لفاتورة الشراء.
    /// </summary>
    /// <param name="item">كيان بند فاتورة الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemAsync(PurchaseInvoiceItem item, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة بنود لفاتورة الشراء دفعة واحدة.
    /// </summary>
    /// <param name="items">قائمة بنود فاتورة الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddItemsAsync(IEnumerable<PurchaseInvoiceItem> items, CancellationToken ct = default);

    /// <summary>
    /// إزالة بند فاتورة شراء من السياق المحلي وقاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه.</param>
    void RemoveItem(PurchaseInvoiceItem item);

    /// <summary>
    /// حذف نهائي وفيزيائي لبند فاتورة الشراء من قاعدة البيانات.
    /// </summary>
    /// <param name="item">كيان البند المراد حذفه نهائياً.</param>
    void HardDeleteItem(PurchaseInvoiceItem item);

    /// <summary>
    /// جلب بنود فاتورة شراء محددة كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="invoiceId">معرف فاتورة الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بنود فاتورة الشراء.</returns>
    Task<IReadOnlyList<PurchaseInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default);

    // ── تفاصيل استلام النكهات والباركودات (Item Breakdowns) ──────

    /// <summary>
    /// إضافة تفصيل استلام نكهة/باركود لبند فاتورة الشراء.
    /// </summary>
    /// <param name="breakdown">كيان تفصيل استلام البند.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddBreakdownAsync(PurchaseInvoiceItemBreakdown breakdown, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة تفاصيل استلام نكهات/باركودات دفعة واحدة.
    /// </summary>
    /// <param name="breakdowns">قائمة تفاصيل استلام البنود.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddBreakdownsAsync(IEnumerable<PurchaseInvoiceItemBreakdown> breakdowns, CancellationToken ct = default);

    /// <summary>
    /// إزالة تفصيل استلام من السياق المحلي وقاعدة البيانات.
    /// </summary>
    /// <param name="breakdown">كيان تفصيل الاستلام المراد حذفه.</param>
    void RemoveBreakdown(PurchaseInvoiceItemBreakdown breakdown);

    /// <summary>
    /// حذف نهائي وفيزيائي لتفصيل استلام نكهة/باركود من قاعدة البيانات.
    /// </summary>
    /// <param name="breakdown">كيان تفصيل الاستلام المراد حذفه نهائياً.</param>
    void HardDeleteBreakdown(PurchaseInvoiceItemBreakdown breakdown);
}
