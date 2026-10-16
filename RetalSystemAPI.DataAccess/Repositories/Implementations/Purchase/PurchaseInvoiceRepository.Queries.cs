using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class PurchaseInvoiceRepository
{
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoiceItem>().AsNoTracking().AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarCodeId!.Value), ct);
    }

    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoiceItem>().AsNoTracking().AnyAsync(e => e.ProductId == id, ct);
    }

    public async Task<PageResult<PurchaseInvoice>> SearchAsync(PurchaseInvoiceQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<PurchaseInvoice>.GetQuery(_context.Set<PurchaseInvoice>(), new PurchaseInvoiceFilterSpec(request.SupplierId, request.BranchId, request.WarehouseId, request.Status, request.PaymentMethod, request.FromDate, request.ToDate, request.SearchTerm)), request, ct);
    }

    public async Task<bool> NumberExistsAsync(string invoiceNumber, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoice>().AsNoTracking().AnyAsync(e => e.InvoiceNumber == invoiceNumber, ct);
    }

    public async Task<PurchaseInvoice?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<PurchaseInvoice>.GetQuery(_context.Set<PurchaseInvoice>(), new PurchaseInvoiceWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<PurchaseInvoiceItemBreakdown>> GetBreakdownsByItemIdsForUpdateAsync(IEnumerable<Guid> oldItemIds, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoiceItemBreakdown>().Where(e => oldItemIds.Contains(e.PurchaseInvoiceItemId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PurchaseInvoiceItem>> GetItemsByIdsForUpdateAsync(IEnumerable<Guid> oldItemIds, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoiceItem>().Where(e => oldItemIds.Contains(e.Id)).ToListAsync(ct);
    }

    public async Task<PurchaseInvoice?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<PurchaseInvoice>.GetQuery(_context.Set<PurchaseInvoice>(), new PurchaseInvoiceWithDetailsSpec(id)).FirstOrDefaultAsync(ct);
    }

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
        Items = p.Items.Select(i => new PurchaseInvoiceItemReadResult { Id = i.Id, ProductId = i.ProductId, ProductName = i.Product != null ? i.Product.Name : string.Empty, ProductCode = i.ProductBarCode != null ? i.ProductBarCode.BarCode : null, ProductBarCodeId = i.ProductBarCodeId, BarCode = i.ProductBarCode != null ? i.ProductBarCode.BarCode : null, Quantity = i.Quantity, UnitPrice = i.UnitPrice, DiscountAmount = i.DiscountAmount, LineTotal = i.LineTotal, CreatedAt = i.CreatedAt, UpdatedAt = i.UpdatedAt, Breakdowns = i.Breakdowns.Select(b => new PurchaseInvoiceBreakdownReadResult { Id = b.Id, ProductBarCodeId = b.ProductBarCodeId, BarCode = b.ProductBarCode != null ? b.ProductBarCode.BarCode : null, Title = b.ProductBarCode != null ? b.ProductBarCode.Title : null, PackageQuantity = b.PackageQuantity, UnitsPerPackage = b.UnitsPerPackage, Quantity = b.Quantity, UnitPrice = b.UnitPrice, CreatedAt = b.CreatedAt, UpdatedAt = b.UpdatedAt }).ToList() }).ToList()
    };
    public async Task<PurchaseInvoiceReadResult?> GetReadResultAsync(Guid id, CancellationToken ct = default) => await _context.PurchaseInvoices.AsNoTracking().Where(p => p.Id == id).Select(InvoiceReadProjection).FirstOrDefaultAsync(ct);
}
