using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات وبحوثات أرصدة المستودعات وصالات العرض وتصدير وحساب الإجماليات والكميات المنخفضة.
/// </summary>
public partial class StockRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<Guid, long>> GetQuantitiesByProductAsync(IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        // استخراج قائمة معرفات المنتجات المميزة لتجنب التكرار في الاستعلام
        var ids = productIds.Distinct().ToArray();

        // تجميع كميات المنتجات من كافة صالات العرض
        var showroom = await _context.Set<ShowroomStock>()
            .AsNoTracking()
            .Where(s => ids.Contains(s.ProductId))
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(s => (long)s.Quantity) })
            .ToListAsync(ct);

        // تجميع كميات الأصناف من كافة المستودعات الرئيسية بحسب معرف المنتج في الباركود
        var storage = await _context.Set<StorgeStock>()
            .AsNoTracking()
            .Where(s => ids.Contains(s.ProductBarcode.ProductId))
            .GroupBy(s => s.ProductBarcode.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(s => (long)s.Quantity) })
            .ToListAsync(ct);

        // دمج كميات الصالات والمستودعات معاً وإرجاعها في قاموس نهائي
        return showroom.Concat(storage)
            .GroupBy(s => s.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockShowroomExportRow>> GetShowroomExportRowsAsync(CancellationToken ct = default)
    {
        // إسقاط بيانات أرصدة صالات العرض إلى سجلات مبسطة مهيأة للتصدير
        return await _context.Set<ShowroomStock>()
            .AsNoTracking()
            .Select(e => new StockShowroomExportRow(e.ProductId, e.Quantity))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockStorageExportRow>> GetStorageExportRowsAsync(CancellationToken ct = default)
    {
        // إسقاط بيانات أرصدة المستودعات الرئيسية إلى سجلات مبسطة مهيأة للتصدير
        return await _context.Set<StorgeStock>()
            .AsNoTracking()
            .Select(e => new StockStorageExportRow(e.ProductBarcodeId, e.Quantity))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasNonzeroStorageByBarcodesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        // التحقق من وجود أي كمية غير صفرية في المستودعات الرئيسية للباركودات المحددة
        return await _context.Set<StorgeStock>()
            .AsNoTracking()
            .AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarcodeId) && e.Quantity != 0, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StorgeStock>> GetStorageByBarcodesForUpdateAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        // جلب أرصدة المستودعات الرئيسية للباركودات المحددة مع التتبع لتعديلها
        return await _context.Set<StorgeStock>()
            .Where(e => removedBarcodeIds.Contains(e.ProductBarcodeId))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasNonzeroShowroomByProductAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود رصيد غير صفري في صالات العرض للمنتج
        return await _context.Set<ShowroomStock>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == id && e.Quantity != 0, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ShowroomStock>> GetShowroomByProductForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // جلب أرصدة صالات العرض للمنتج مع التتبع لإجراء التحديثات المخزنية
        return await _context.Set<ShowroomStock>()
            .Where(e => e.ProductId == id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StorgeStock>> GetStorageForUpdateAsync(Guid warehouseId, IEnumerable<Guid> barcodeIdList, CancellationToken ct = default)
    {
        // جلب أرصدة المستودع للباركودات المحددة بالدفعات لتفادي قيود الحجم مع التتبع
        return await ImportQueryBatch.ByIdsAsync(
            barcodeIdList,
            ids => _context.Set<StorgeStock>().Where(e => e.WarehouseId == warehouseId && ids.Contains(e.ProductBarcodeId)),
            ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ShowroomStock>> GetShowroomForUpdateAsync(Guid warehouseId, IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        // جلب أرصدة الصالة للمنتجات المحددة بالدفعات مع التتبع لتعديل الكميات
        return await ImportQueryBatch.ByIdsAsync(
            productIds,
            ids => _context.Set<ShowroomStock>().Where(e => e.WarehouseId == warehouseId && ids.Contains(e.ProductId)),
            ct);
    }

    /// <inheritdoc/>
    public async Task<StorgeStock?> GetStorageDetailsAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        // استرجاع رصيد المستودع مع تفاصيل المنتج والباركود كقراءة فقط
        return await SpecificationEvaluator<StorgeStock>
            .GetQuery(_context.Set<StorgeStock>(), new StorgeStockWithDetailsSpec(warehouseId, productBarcodeId))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockStorageKeysRow>> GetStorageKeysAsync(Guid warehouseId, CancellationToken ct = default)
    {
        // استرجاع أزواج مفاتيح المستودع والباركود لأرصدة المستودع دون تتبع
        return await _context.Set<StorgeStock>()
            .AsNoTracking()
            .Where(e => e.WarehouseId == warehouseId)
            .Select(e => new StockStorageKeysRow(e.WarehouseId, e.ProductBarcodeId))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StorgeStock>> ListStorageAsync(StorageStockQuery request, CancellationToken ct = default)
    {
        // استرجاع قائمة أرصدة المستودع المفصلة وفق معايير الاستعلام مع تجزئة الاستعلام
        return await SpecificationEvaluator<StorgeStock>
            .GetQuery(_context.Set<StorgeStock>(), new StorgeStockWithDetailsSpec(request.WarehouseId, request.SearchTerm, request.ExactBarcode))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<StorgeStock?> GetStorageAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        // جلب رصيد مستودع بسيط لباركود ومخزن محدد كقراءة فقط
        return await _context.Set<StorgeStock>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.WarehouseId == warehouseId && e.ProductBarcodeId == productBarcodeId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StorgeStock>> GetLowStorageAsync(Guid? warehouseId, CancellationToken ct = default)
    {
        // استرجاع أرصدة المستودع التي بلغت حد الطلب أو أقل عبر مواصفة LowStorgeStockSpec
        return await SpecificationEvaluator<StorgeStock>
            .GetQuery(_context.Set<StorgeStock>(), new LowStorgeStockSpec(warehouseId))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ShowroomStock?> GetShowroomDetailsAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        // استرجاع رصيد صالة العرض مع تفاصيل المنتج والفرع كقراءة فقط
        return await SpecificationEvaluator<ShowroomStock>
            .GetQuery(_context.Set<ShowroomStock>(), new ShowroomStockWithDetailsSpec(warehouseId, productId))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockShowroomKeysRow>> GetShowroomKeysAsync(Guid warehouseId, CancellationToken ct = default)
    {
        // استرجاع مفاتيح الصالة والمنتجات لأرصدة المعرض
        return await _context.Set<ShowroomStock>()
            .AsNoTracking()
            .Where(e => e.WarehouseId == warehouseId)
            .Select(e => new StockShowroomKeysRow(e.WarehouseId, e.ProductId))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ShowroomStock>> ListShowroomAsync(ShowroomStockQuery request, CancellationToken ct = default)
    {
        // استرجاع قائمة أرصدة صالات العرض وفق الفلترة مع تجزئة الاستعلام
        return await SpecificationEvaluator<ShowroomStock>
            .GetQuery(_context.Set<ShowroomStock>(), new ShowroomStockWithDetailsSpec(request.WarehouseId, request.SearchTerm, request.ExactBarcode))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ShowroomStock?> GetShowroomAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        // جلب رصيد صالة عرض بسيط لصنف ومخزن محدد دون تتبع
        return await _context.Set<ShowroomStock>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.WarehouseId == warehouseId && e.ProductId == productId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ShowroomStock>> GetLowShowroomAsync(Guid? warehouseId, CancellationToken ct = default)
    {
        // استرجاع أصناف صالة العرض التي وصلت إلى حد الطلب الأدنى لتنبيه إعادة التغذية
        return await SpecificationEvaluator<ShowroomStock>
            .GetQuery(_context.Set<ShowroomStock>(), new LowShowroomStockSpec(warehouseId))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasPositiveStorageByWarehouseAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود أي كميات بضاعة موجبة بالمستودع تمنع حذفه
        return await _context.Set<StorgeStock>()
            .AsNoTracking()
            .AnyAsync(e => e.WarehouseId == id && e.Quantity > 0, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasPositiveShowroomByWarehouseAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود أي كميات بضاعة موجبة بالصالة تمنع حذفها
        return await _context.Set<ShowroomStock>()
            .AsNoTracking()
            .AnyAsync(e => e.WarehouseId == id && e.Quantity > 0, ct);
    }

    /// <inheritdoc/>
    public Task<PageResult<StorgeStock>> SearchStorageAsync(StorageStockQuery request, CancellationToken ct = default)
    {
        // إعداد مواصفة تفاصيل أرصدة المستودع مع الفلترة والبحث
        var spec = new StorgeStockWithDetailsSpec(request.WarehouseId, request.SearchTerm, request.ExactBarcode);

        // قراءة نتائج البحث المنسقة بصفحات وفرز
        return QueryPaging.ReadAsync(
            SpecificationEvaluator<StorgeStock>.GetQuery(_context.Set<StorgeStock>(), spec),
            request,
            ct);
    }

    /// <inheritdoc/>
    public Task<PageResult<ShowroomStock>> SearchShowroomAsync(ShowroomStockQuery request, CancellationToken ct = default)
    {
        // إعداد مواصفة تفاصيل أرصدة صالات العرض مع معايير البحث
        var spec = new ShowroomStockWithDetailsSpec(request.WarehouseId, request.SearchTerm, request.ExactBarcode);

        // قراءة نتائج البحث المقسمة لصفحات لصالات العرض
        return QueryPaging.ReadAsync(
            SpecificationEvaluator<ShowroomStock>.GetQuery(_context.Set<ShowroomStock>(), spec),
            request,
            ct);
    }
}
