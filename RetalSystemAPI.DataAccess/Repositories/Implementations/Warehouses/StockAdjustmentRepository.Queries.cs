using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات وبحوثات تسويات الجرد المخزني وفحص مراجع الأصناف والباركودات.
/// </summary>
public partial class StockAdjustmentRepository
{
    /// <inheritdoc/>
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> barcodeIds, CancellationToken ct = default)
    {
        // استخراج معرفات الباركودات المميزة لتفادي التكرار
        var ids = barcodeIds.Distinct().ToArray();

        // فحص وجود أي بند تسوية يشير إلى أحد الباركودات المحددة
        return await _context.Set<StockAdjustmentItem>()
            .AsNoTracking()
            .AnyAsync(i => i.ProductBarCodeId.HasValue && ids.Contains(i.ProductBarCodeId.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود أي بند تسوية مخزنية مرتبط بالمنتج
        return await _context.Set<StockAdjustmentItem>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<StockAdjustment?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع تفاصيل تسوية الجرد وبنودها مع بيانات المستودع كقراءة فقط
        return await SpecificationEvaluator<StockAdjustment>
            .GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<StockAdjustment?> GetByNumberAsync(string adjustmentNumber, CancellationToken ct = default)
    {
        // البحث عن تسوية الجرد برقم السند مع تضمين كامل التفاصيل كقراءة فقط
        return await SpecificationEvaluator<StockAdjustment>
            .GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(adjustmentNumber))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StockAdjustment>> ListAsync(StockAdjustmentQuery request, CancellationToken ct = default)
    {
        // استرجاع قائمة تسويات الجرد المطابقة لمعايير الفلترة مع تجزئة الاستعلام
        return await SpecificationEvaluator<StockAdjustment>
            .GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(request.WarehouseId, request.Reason, request.FromDate, request.ToDate, request.Search))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<StockAdjustment>> SearchAsync(StockAdjustmentQuery request, CancellationToken ct = default)
    {
        // البحث المقسم لصفحات لتسويات الجرد وفق المعايير المدخلة
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<StockAdjustment>.GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(request.WarehouseId, request.Reason, request.FromDate, request.ToDate, request.Search)),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NumberExistsAsync(string adjustmentNumber, CancellationToken ct = default)
    {
        // التحقق من تكرار رقم تسوية الجرد في قاعدة البيانات
        return await _context.Set<StockAdjustment>()
            .AsNoTracking()
            .AnyAsync(e => e.AdjustmentNumber == adjustmentNumber, ct);
    }

    /// <inheritdoc/>
    public async Task<StockAdjustment?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع تفاصيل تسوية الجرد وبنودها مع تتبع الكيانات لإجراء التعديلات
        return await SpecificationEvaluator<StockAdjustment>
            .GetQuery(_context.Set<StockAdjustment>(), new StockAdjustmentWithDetailsSpec(id))
            .FirstOrDefaultAsync(ct);
    }
}
