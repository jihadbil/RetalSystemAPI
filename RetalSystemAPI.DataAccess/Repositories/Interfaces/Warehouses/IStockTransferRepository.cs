using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
/// <summary>
/// واجهة مستودع بيانات أوامر التحويل المخزني (StockTransfers) وبنودها كوحدة متكاملة.
/// </summary>
public partial interface IStockTransferRepository : IRepository<StockTransfer>
{
    /// <summary>إضافة بند لأمر التحويل المخزني</summary>
    Task AddItemAsync(StockTransferItem item, CancellationToken ct = default);
    /// <summary>إضافة مجموعة بنود لأمر التحويل المخزني</summary>
    Task AddItemsAsync(IEnumerable<StockTransferItem> items, CancellationToken ct = default);
    /// <summary>إزالة بند من السياق المحلي</summary>
    void RemoveItem(StockTransferItem item);
    /// <summary>حذف نهائي لبند تحويل مخزني</summary>
    void HardDeleteItem(StockTransferItem item);
    /// <summary>جلب بنود أمر تحويل مخزني محدد</summary>
    Task<IReadOnlyList<StockTransferItem>> GetItemsAsync(Guid transferId, CancellationToken ct = default);
}
