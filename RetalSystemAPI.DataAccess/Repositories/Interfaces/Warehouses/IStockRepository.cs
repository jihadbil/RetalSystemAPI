using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات أرصدة المخازن الرئيسية (StorgeStock) وصالات العرض (ShowroomStock) المترابطة.
/// </summary>
public interface IStockRepository
{
    // ── أرصدة المستودعات الرئيسية (StorgeStock) ─────────────────

    /// <summary>جلب رصيد المستودع لنكهة/باركود محدد في مخزن محدد</summary>
    Task<StorgeStock?> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);

    /// <summary>جلب أرصدة المستودعات بمواصفة (Specification)</summary>
    Task<IReadOnlyList<StorgeStock>> GetStorgeStocksAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default);

    /// <summary>البحث عن أرصدة مستودعات بشرط منطقي</summary>
    Task<IReadOnlyList<StorgeStock>> FindStorgeStocksAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default);

    /// <summary>البحث عن أرصدة مستودعات بمواصفة (Specification)</summary>
    Task<IReadOnlyList<StorgeStock>> FindStorgeStocksAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default);

    /// <summary>البحث عن أرصدة مستودعات مع تتبع التغييرات</summary>
    Task<IReadOnlyList<StorgeStock>> FindStorgeStockTrackedAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default);

    /// <summary>جلب كافة أرصدة المستودعات مع التتبع</summary>
    Task<IReadOnlyList<StorgeStock>> GetAllStorgeStocksTrackedAsync(CancellationToken ct = default);

    /// <summary>جلب أول رصيد مستودع مطابق لمواصفة</summary>
    Task<StorgeStock?> FirstOrDefaultStorgeStockAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default);

    /// <summary>جلب أول رصيد مستودع مطابق لشرط</summary>
    Task<StorgeStock?> FirstOrDefaultStorgeStockAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default);

    /// <summary>جلب أول رصيد مستودع مطابق لمواصفة مع التتبع</summary>
    Task<StorgeStock?> FirstOrDefaultStorgeStockTrackedAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default);

    /// <summary>جلب أول رصيد مستودع مطابق لشرط مع التتبع</summary>
    Task<StorgeStock?> FirstOrDefaultStorgeStockTrackedAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default);

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

    /// <summary>التحقق من وجود رصيد مستودع مطابق لشرط</summary>
    Task<bool> StorgeStockExistsAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default);

    /// <summary>حساب عدد سجلات أرصدة المستودع المطابقة لمواصفة</summary>
    Task<int> CountStorgeStockAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default);

    /// <summary>إسقاط وتحديد حقول من أرصدة المستودع (Projection)</summary>
    Task<IReadOnlyList<TResult>> SelectStorgeStocksAsync<TResult>(Expression<Func<StorgeStock, TResult>> selector, CancellationToken ct = default);

    /// <summary>جلب صفحة من أرصدة المستودع مع العدد الكلي</summary>
    Task<(IReadOnlyList<StorgeStock> Items, int TotalCount)> GetPagedStorgeStockAsync(ISpecification<StorgeStock> spec, int pageNumber, int pageSize, CancellationToken ct = default);


    // ── أرصدة صالات العرض (ShowroomStock) ────────────────────────

    /// <summary>جلب رصيد صالة العرض لصنف محدد في فرع/مخزن محدد</summary>
    Task<ShowroomStock?> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);

    /// <summary>جلب أرصدة صالات العرض بمواصفة (Specification)</summary>
    Task<IReadOnlyList<ShowroomStock>> GetShowroomStocksAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default);

    /// <summary>البحث عن أرصدة صالات العرض بشرط منطقي</summary>
    Task<IReadOnlyList<ShowroomStock>> FindShowroomStocksAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default);

    /// <summary>البحث عن أرصدة صالات العرض بمواصفة (Specification)</summary>
    Task<IReadOnlyList<ShowroomStock>> FindShowroomStocksAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default);

    /// <summary>البحث عن أرصدة صالات العرض مع تتبع التغييرات</summary>
    Task<IReadOnlyList<ShowroomStock>> FindShowroomStockTrackedAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default);

    /// <summary>جلب كافة أرصدة صالات العرض مع التتبع</summary>
    Task<IReadOnlyList<ShowroomStock>> GetAllShowroomStocksTrackedAsync(CancellationToken ct = default);

    /// <summary>جلب أول رصيد صالة عرض مطابق لمواصفة</summary>
    Task<ShowroomStock?> FirstOrDefaultShowroomStockAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default);

    /// <summary>جلب أول رصيد صالة عرض مطابق لشرط</summary>
    Task<ShowroomStock?> FirstOrDefaultShowroomStockAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default);

    /// <summary>جلب أول رصيد صالة عرض مطابق لمواصفة مع التتبع</summary>
    Task<ShowroomStock?> FirstOrDefaultShowroomStockTrackedAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default);

    /// <summary>جلب أول رصيد صالة عرض مطابق لشرط مع التتبع</summary>
    Task<ShowroomStock?> FirstOrDefaultShowroomStockTrackedAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default);

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

    /// <summary>التحقق من وجود رصيد صالة عرض مطابق لشرط</summary>
    Task<bool> ShowroomStockExistsAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default);

    /// <summary>حساب عدد سجلات أرصدة صالة العرض المطابقة لمواصفة</summary>
    Task<int> CountShowroomStockAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default);

    /// <summary>إسقاط وتحديد حقول من أرصدة صالة العرض (Projection)</summary>
    Task<IReadOnlyList<TResult>> SelectShowroomStocksAsync<TResult>(Expression<Func<ShowroomStock, TResult>> selector, CancellationToken ct = default);

    /// <summary>جلب صفحة من أرصدة صالة العرض مع العدد الكلي</summary>
    Task<(IReadOnlyList<ShowroomStock> Items, int TotalCount)> GetPagedShowroomStockAsync(ISpecification<ShowroomStock> spec, int pageNumber, int pageSize, CancellationToken ct = default);
}
