using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات فواتير المبيعات وبحوثاتها والتحقق من التبعيات وأرقام الفواتير.
/// </summary>
public partial class SalesInvoiceRepository
{
    /// <inheritdoc/>
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        // فحص وجود أي بند فاتورة مبيعات يشير إلى أي من الباركودات المعطاة لمنع حذفها
        return await _context.Set<SalesInvoiceItem>()
            .AsNoTracking()
            .AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarCodeId!.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود أي مبيعات مسجلة للمنتج
        return await _context.Set<SalesInvoiceItem>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<SalesInvoice?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع تفاصيل الفاتورة وبنودها والعميل والفرع كقراءة فقط عبر مواصفة SalesInvoiceWithDetailsSpec
        return await SpecificationEvaluator<SalesInvoice>
            .GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<SalesInvoice?> GetByNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        // استرجاع الفاتورة بالرقم الفريد عبر مواصفة تفاصيل الفاتورة كقراءة فقط
        return await SpecificationEvaluator<SalesInvoice>
            .GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceWithDetailsSpec(invoiceNumber))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SalesInvoice>> ListAsync(SalesInvoiceQuery request, CancellationToken ct = default)
    {
        // استعلام قائمة فواتير المبيعات المفلترة باستخدام SplitQuery لتعظيم كفاءة استرجاع البنود
        return await SpecificationEvaluator<SalesInvoice>
            .GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceListSpec(request.BranchId, request.WarehouseId, request.CustomerId, request.Status, request.PaymentMethod, request.FromDate, request.ToDate, request.Search))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<SalesInvoice>> SearchAsync(SalesInvoiceQuery request, CancellationToken ct = default)
    {
        // البحث في فواتير المبيعات وتصفح النتائج بالصفحات والفرز
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<SalesInvoice>.GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceListSpec(request.BranchId, request.WarehouseId, request.CustomerId, request.Status, request.PaymentMethod, request.FromDate, request.ToDate, request.Search)),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NumberExistsAsync(string invoiceNumber, CancellationToken ct = default)
    {
        // فحص وجود فاتورة مبيعات تحمل نفس الرقم
        return await _context.Set<SalesInvoice>()
            .AsNoTracking()
            .AnyAsync(e => e.InvoiceNumber == invoiceNumber, ct);
    }

    /// <inheritdoc/>
    public async Task<SalesInvoice?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل الفاتورة وبنودها متتبعة للتعديل ضمن وحدة العمل
        return await SpecificationEvaluator<SalesInvoice>
            .GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceWithDetailsSpec(id))
            .FirstOrDefaultAsync(ct);
    }
}
