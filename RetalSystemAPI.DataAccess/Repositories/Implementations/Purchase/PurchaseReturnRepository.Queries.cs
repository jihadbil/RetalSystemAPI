using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات وبحوثات مرتجعات المشتريات وفحص التبعيات واحتساب الكميات المرتجعة سابقاً.
/// </summary>
public partial class PurchaseReturnRepository
{
    /// <inheritdoc/>
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        // التحقق من وجود أي بند مرتجع مشتريات يشير إلى أي من معرفات الباركود المحددة
        return await _context.Set<PurchaseReturnItem>()
            .AsNoTracking()
            .AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarCodeId!.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // فحص وجود أي بند مرتجع مشتريات مرتبط بهذا المنتج
        return await _context.Set<PurchaseReturnItem>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasInvoiceReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // فحص وجود أي مرتجع مشتريات صادر استناداً إلى فاتورة الشراء الأصلية
        return await _context.Set<PurchaseReturn>()
            .AsNoTracking()
            .AnyAsync(e => e.PurchaseInvoiceId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<PurchaseReturn?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // جلب تفاصيل مرتجع المشتريات وبنوده للعرض دون تتبع تغييرات الكيانات
        return await SpecificationEvaluator<PurchaseReturn>
            .GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PurchaseReturn?> GetByNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        // البحث عن مرتجع المشتريات برقم الإشعار الفريد مع تضمين تفاصيله كقراءة فقط
        return await SpecificationEvaluator<PurchaseReturn>
            .GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnWithDetailsSpec(returnNumber))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseReturn>> ListAsync(PurchaseReturnQuery request, CancellationToken ct = default)
    {
        // استرجاع قائمة بمرتجعات المشتريات وفق معايير الفلترة المحددة مع تجزئة الاستعلام
        return await SpecificationEvaluator<PurchaseReturn>
            .GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnListSpec(request.BranchId, request.WarehouseId, request.SupplierId, request.Reason, request.PaymentMethod, request.FromDate, request.ToDate, request.Search))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<PurchaseReturn>> SearchAsync(PurchaseReturnQuery request, CancellationToken ct = default)
    {
        // البحث في مرتجعات المشتريات مع دعم تقسيم الصفحات والفرز
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<PurchaseReturn>.GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnListSpec(request.BranchId, request.WarehouseId, request.SupplierId, request.Reason, request.PaymentMethod, request.FromDate, request.ToDate, request.Search)),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NumberExistsAsync(string returnNumber, CancellationToken ct = default)
    {
        // التحقق من تكرار رقم إشعار مرتجع المشتريات
        return await _context.Set<PurchaseReturn>()
            .AsNoTracking()
            .AnyAsync(e => e.ReturnNumber == returnNumber, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseReturnItem>> GetReturnedItemsByInvoiceAndBarcodeAsync(Guid originalInvoiceId, Guid barcodeId, CancellationToken ct = default)
    {
        // استرجاع كافة بنود الإرجاع لفاتورة معينة المرتبطة بباركود صنف محدد
        return await _context.Set<PurchaseReturnItem>()
            .AsNoTracking()
            .Where(e => e.PurchaseReturn != null && e.PurchaseReturn.PurchaseInvoiceId == originalInvoiceId && e.ProductBarCodeId == barcodeId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseReturnItem>> GetReturnedItemsByInvoiceAndBarcodesAsync(Guid originalInvoiceId, IEnumerable<Guid> returnBarcodeIds, CancellationToken ct = default)
    {
        // استرجاع كافة بنود الإرجاع لفاتورة معينة لمجموعة من الباركودات المحددة
        return await _context.Set<PurchaseReturnItem>()
            .AsNoTracking()
            .Where(e => e.PurchaseReturn != null && e.PurchaseReturn.PurchaseInvoiceId == originalInvoiceId && e.ProductBarCodeId.HasValue && returnBarcodeIds.Contains(e.ProductBarCodeId.Value))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PurchaseReturn?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // جلب تفاصيل المرتجع مع تتبع الكيانات وبنودها لإجراء التعديل
        return await SpecificationEvaluator<PurchaseReturn>
            .GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnWithDetailsSpec(id))
            .FirstOrDefaultAsync(ct);
    }
}
