using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات وبحوثات فواتير المشتريات والتحقق من التبعيات وتفاصيل استلام النكهات.
/// </summary>
public partial class PurchaseInvoiceRepository
{
    /// <inheritdoc/>
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        // استخراج معرفات الباركودات المميزة المطلوب فحصها
        var ids = removedBarcodeIds.Distinct().ToArray();

        // فحص وجود أي بند فاتورة أو تفصيل استلام يشير لأي من الباركودات المعطاة
        return await _context.Set<PurchaseInvoiceItem>().AsNoTracking().AnyAsync(e => e.ProductBarCodeId.HasValue && ids.Contains(e.ProductBarCodeId.Value), ct)
            || await _context.Set<PurchaseInvoiceItemBreakdown>().AsNoTracking().AnyAsync(e => ids.Contains(e.ProductBarCodeId), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود أي مشتريات مسجلة للمنتج
        return await _context.Set<PurchaseInvoiceItem>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<PurchaseInvoice>> SearchAsync(PurchaseInvoiceQuery request, CancellationToken ct = default)
    {
        // البحث في فواتير المشتريات بالمعايير والصفحات عبر مواصفة الفلترة المخصصة
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<PurchaseInvoice>.GetQuery(_context.Set<PurchaseInvoice>(), new PurchaseInvoiceFilterSpec(request.SupplierId, request.BranchId, request.WarehouseId, request.Status, request.PaymentMethod, request.FromDate, request.ToDate, request.SearchTerm)),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NumberExistsAsync(string invoiceNumber, CancellationToken ct = default)
    {
        // فحص وجود فاتورة مشتريات تحمل نفس الرقم للمستأجر
        return await _context.Set<PurchaseInvoice>()
            .AsNoTracking()
            .AnyAsync(e => e.InvoiceNumber == invoiceNumber, ct);
    }

    /// <inheritdoc/>
    public async Task<PurchaseInvoice?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع تفاصيل فاتورة المشتريات وبنودها كقراءة فقط عبر مواصفة PurchaseInvoiceWithDetailsSpec
        return await SpecificationEvaluator<PurchaseInvoice>
            .GetQuery(_context.Set<PurchaseInvoice>(), new PurchaseInvoiceWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseInvoiceItemBreakdown>> GetBreakdownsByItemIdsForUpdateAsync(IEnumerable<Guid> oldItemIds, CancellationToken ct = default)
    {
        // جلب تفاصيل الاستلام التابعة لبنود معينة ككيانات متتبعة للتعديل أو الحذف
        return await _context.Set<PurchaseInvoiceItemBreakdown>()
            .Where(e => oldItemIds.Contains(e.PurchaseInvoiceItemId))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseInvoiceItem>> GetItemsByIdsForUpdateAsync(IEnumerable<Guid> oldItemIds, CancellationToken ct = default)
    {
        // جلب بنود الفاتورة بالمعرفات ككيانات متتبعة للتعديل
        return await _context.Set<PurchaseInvoiceItem>()
            .Where(e => oldItemIds.Contains(e.Id))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PurchaseInvoice?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل فاتورة المشتريات مع بنودها متتبعة للتعديل ضمن وحدة العمل
        return await SpecificationEvaluator<PurchaseInvoice>
            .GetQuery(_context.Set<PurchaseInvoice>(), new PurchaseInvoiceWithDetailsSpec(id))
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// تعبير إسقاط قراءة فاتورة المشتريات المحسّن لتجنب تحميل الكيانات وتوفير استهلاك الذاكرة.
    /// </summary>
    private static readonly System.Linq.Expressions.Expression<Func<PurchaseInvoice, PurchaseInvoiceReadResult>> InvoiceReadProjection = p => new PurchaseInvoiceReadResult
    {
        Id = p.Id,
        InvoiceNumber = p.InvoiceNumber,
        InvoiceDate = p.InvoiceDate,
        SupplierId = p.SupplierId,
        SupplierName = p.Supplier != null ? p.Supplier.Name : string.Empty,
        BranchId = p.BranchId,
        BranchName = p.Branch != null ? p.Branch.Name : string.Empty,
        WarehouseId = p.WarehouseId,
        WarehouseName = p.Warehouse != null ? p.Warehouse.Name : string.Empty,
        Status = p.Status,
        PaymentMethod = p.PaymentMethod,
        SubTotal = p.SubTotal,
        DiscountAmount = p.DiscountAmount,
        TaxAmount = p.TaxAmount,
        TotalAmount = p.TotalAmount,
        PaidAmount = p.PaidAmount,
        PaymentSafeId = p.PaymentSafeId,
        RemainingAmount = p.RemainingAmount,
        PurchaseOrderId = p.PurchaseOrderId,
        PurchaseOrderNumber = p.PurchaseOrder != null ? p.PurchaseOrder.OrderNumber : null,
        Notes = p.Notes,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Items = p.Items.Select(i => new PurchaseInvoiceItemReadResult
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product != null ? i.Product.Name : string.Empty,
            ProductCode = i.ProductBarCode != null ? i.ProductBarCode.BarCode : null,
            ProductBarCodeId = i.ProductBarCodeId,
            BarCode = i.ProductBarCode != null ? i.ProductBarCode.BarCode : null,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            DiscountAmount = i.DiscountAmount,
            LineTotal = i.LineTotal,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt,
            Breakdowns = i.Breakdowns.Select(b => new PurchaseInvoiceBreakdownReadResult
            {
                Id = b.Id,
                ProductBarCodeId = b.ProductBarCodeId,
                BarCode = b.ProductBarCode != null ? b.ProductBarCode.BarCode : null,
                Title = b.ProductBarCode != null ? b.ProductBarCode.Title : null,
                PackageQuantity = b.PackageQuantity,
                UnitsPerPackage = b.UnitsPerPackage,
                Quantity = b.Quantity,
                UnitPrice = b.UnitPrice,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToList()
        }).ToList()
    };

    /// <inheritdoc/>
    public async Task<PurchaseInvoiceReadResult?> GetReadResultAsync(Guid id, CancellationToken ct = default)
    {
        // تنفيذ استعلام الإسقاط المباشر لفاتورة المشتريات وبنودها كقراءة فقط
        return await _context.PurchaseInvoices
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(InvoiceReadProjection)
            .FirstOrDefaultAsync(ct);
    }
}
