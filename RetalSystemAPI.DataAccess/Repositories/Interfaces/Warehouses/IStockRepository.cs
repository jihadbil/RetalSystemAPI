using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
/// <summary>
/// واجهة مستودع بيانات أرصدة المخازن الرئيسية (StorgeStock) وصالات العرض (ShowroomStock) المترابطة.
/// </summary>
public partial interface IStockRepository
{
    // ── أرصدة المستودعات الرئيسية (StorgeStock) ─────────────────
    /// <summary>جلب رصيد المستودع لنكهة/باركود محدد في مخزن محدد</summary>
    Task<StorgeStock?> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);
    /// <summary>إضافة رصيد مستودع جديد</summary>
    Task AddStorgeStockAsync(StorgeStock stock, CancellationToken ct = default);
    /// <summary>إضافة مجموعة أرصدة مستودع</summary>
    Task AddStorgeStocksAsync(IEnumerable<StorgeStock> stocks, CancellationToken ct = default);
    /// <summary>تحديث رصيد مستودع</summary>
    void UpdateStorgeStock(StorgeStock stock);
    /// <summary>حذف ناعم لرصيد مستودع</summary>
    void SoftDeleteStorgeStock(StorgeStock stock);
    /// <summary>حذف نهائي لرصيد مستودع</summary>
    void HardDeleteStorgeStock(StorgeStock stock);
    // ── أرصدة صالات العرض (ShowroomStock) ────────────────────────
    /// <summary>جلب رصيد صالة العرض لصنف محدد في فرع/مخزن محدد</summary>
    Task<ShowroomStock?> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);
    /// <summary>إضافة رصيد صالة عرض جديد</summary>
    Task AddShowroomStockAsync(ShowroomStock stock, CancellationToken ct = default);
    /// <summary>إضافة مجموعة أرصدة صالة عرض</summary>
    Task AddShowroomStocksAsync(IEnumerable<ShowroomStock> stocks, CancellationToken ct = default);
    /// <summary>تحديث رصيد صالة عرض</summary>
    void UpdateShowroomStock(ShowroomStock stock);
    /// <summary>حذف ناعم لرصيد صالة عرض</summary>
    void SoftDeleteShowroomStock(ShowroomStock stock);
    /// <summary>حذف نهائي لرصيد صالة عرض</summary>
    void HardDeleteShowroomStock(ShowroomStock stock);
}
