using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات مرتجعات المبيعات وبحوثاتها واحتساب سقوف الإرجاع.
/// </summary>
public partial class SalesReturnRepository
{
    /// <inheritdoc/>
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        // التحقق من وجود أي بند مرتجع مبيعات يشير لأي من الباركودات المعطاة
        return await _context.Set<SalesReturnItem>()
            .AsNoTracking()
            .AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarCodeId!.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود مرتجعات سابقة للمنتج
        return await _context.Set<SalesReturnItem>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasInvoiceReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود أي إشعار مرتجع صادر للفاتورة الأصلية
        return await _context.Set<SalesReturn>()
            .AsNoTracking()
            .AnyAsync(e => e.OriginalInvoiceId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<SalesReturn?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع تفاصيل المرتجع وبنوده والفاتورة الأصلية كقراءة فقط عبر المواصفة
        return await SpecificationEvaluator<SalesReturn>
            .GetQuery(_context.Set<SalesReturn>(), new SalesReturnWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<SalesReturn?> GetByNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        // استرجاع المرتجع برقم الإشعار كقراءة فقط دون تتبع
        return await SpecificationEvaluator<SalesReturn>
            .GetQuery(_context.Set<SalesReturn>(), new SalesReturnWithDetailsSpec(returnNumber))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SalesReturn>> ListAsync(SalesReturnQuery request, CancellationToken ct = default)
    {
        // تنفيذ استعلام قائمة مرتجعات المبيعات باستخدام SplitQuery لتعظيم كفاءة العلاقات
        return await SpecificationEvaluator<SalesReturn>
            .GetQuery(_context.Set<SalesReturn>(), new SalesReturnListSpec(request.BranchId, request.WarehouseId, request.CustomerId, request.Reason, request.FromDate, request.ToDate, request.Search))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<SalesReturn>> SearchAsync(SalesReturnQuery request, CancellationToken ct = default)
    {
        // تصفح وبحث مرتجعات المبيعات بنظام الصفحات
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<SalesReturn>.GetQuery(_context.Set<SalesReturn>(), new SalesReturnListSpec(request.BranchId, request.WarehouseId, request.CustomerId, request.Reason, request.FromDate, request.ToDate, request.Search)),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NumberExistsAsync(string returnNumber, CancellationToken ct = default)
    {
        // فحص وجود إشعار مرتجع يحمل نفس الرقم
        return await _context.Set<SalesReturn>()
            .AsNoTracking()
            .AnyAsync(e => e.ReturnNumber == returnNumber, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SalesReturnItem>> GetReturnedItemsByInvoiceAndProductsAsync(Guid originalInvoiceId, IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        // استرجاع كافة البنود المرتجعة سابقاً لفاتورة ومنتجات معينة لحساب سقف الإرجاع المتبقي
        return await _context.Set<SalesReturnItem>()
            .AsNoTracking()
            .Where(e => e.SalesReturn != null && e.SalesReturn.OriginalInvoiceId == originalInvoiceId && productIds.Contains(e.ProductId))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SalesReturn>> GetByInvoiceAsync(Guid originalInvoiceId, CancellationToken ct = default)
    {
        // استرجاع قائمة كافة إشعارات الإرجاع المرتبطة بالفاتورة الأصلية
        return await _context.Set<SalesReturn>()
            .AsNoTracking()
            .Where(e => e.OriginalInvoiceId == originalInvoiceId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<SalesReturn?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل المرتجع مع بنوده متتبعاً للتعديل ضمن وحدة العمل
        return await SpecificationEvaluator<SalesReturn>
            .GetQuery(_context.Set<SalesReturn>(), new SalesReturnWithDetailsSpec(id))
            .FirstOrDefaultAsync(ct);
    }
}
