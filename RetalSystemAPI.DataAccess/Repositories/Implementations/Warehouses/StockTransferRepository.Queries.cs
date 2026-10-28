using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات وبحوثات أوامر التحويل المخزني وفحص مراجع الأصناف والباركودات.
/// </summary>
public partial class StockTransferRepository
{
    /// <inheritdoc/>
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> barcodeIds, CancellationToken ct = default)
    {
        // استخراج معرفات الباركودات المميزة لتفادي التكرار
        var ids = barcodeIds.Distinct().ToArray();

        // فحص وجود أي بند تحويل يشير إلى أحد الباركودات المحددة
        return await _context.Set<StockTransferItem>()
            .AsNoTracking()
            .AnyAsync(i => i.ProductBarCodeId.HasValue && ids.Contains(i.ProductBarCodeId.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود أي بند تحويل مخزني مرتبط بالمنتج
        return await _context.Set<StockTransferItem>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<StockTransfer?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع تفاصيل أمر التحويل وبنوده مع بيانات المستودعين كقراءة فقط
        return await SpecificationEvaluator<StockTransfer>
            .GetQuery(_context.Set<StockTransfer>(), new StockTransferWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<StockTransfer?> GetByNumberAsync(string transferNumber, CancellationToken ct = default)
    {
        // البحث عن أمر تحويل برقم السند مع تحميل كامل التفاصيل دون تتبع
        return await SpecificationEvaluator<StockTransfer>
            .GetQuery(_context.Set<StockTransfer>(), new StockTransferWithDetailsSpec(transferNumber))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockTransfer>> ListAsync(StockTransferQuery request, CancellationToken ct = default)
    {
        // استرجاع قائمة أوامر التحويل المطابقة لمعايير الفلترة مع تجزئة الاستعلام
        return await SpecificationEvaluator<StockTransfer>
            .GetQuery(_context.Set<StockTransfer>(), new StockTransferListSpec(request.FromWarehouseId, request.ToWarehouseId, request.Status, request.FromDate, request.ToDate, request.Search))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<StockTransfer>> SearchAsync(StockTransferQuery request, CancellationToken ct = default)
    {
        // البحث المقسم لصفحات لأوامر التحويل وفق المعايير المدخلة
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<StockTransfer>.GetQuery(_context.Set<StockTransfer>(), new StockTransferListSpec(request.FromWarehouseId, request.ToWarehouseId, request.Status, request.FromDate, request.ToDate, request.Search)),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NumberExistsAsync(string transferNumber, CancellationToken ct = default)
    {
        // التحقق من تكرار رقم أمر التحويل في قاعدة البيانات
        return await _context.Set<StockTransfer>()
            .AsNoTracking()
            .AnyAsync(e => e.TransferNumber == transferNumber, ct);
    }

    /// <inheritdoc/>
    public async Task<StockTransfer?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع تفاصيل أمر التحويل وبنوده مع تتبع الكيانات لإجراء التحديثات
        return await SpecificationEvaluator<StockTransfer>
            .GetQuery(_context.Set<StockTransfer>(), new StockTransferWithDetailsSpec(id))
            .FirstOrDefaultAsync(ct);
    }
}
