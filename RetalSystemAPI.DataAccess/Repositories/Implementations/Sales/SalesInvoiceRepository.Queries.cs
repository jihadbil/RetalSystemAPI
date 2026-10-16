using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class SalesInvoiceRepository
{
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<SalesInvoiceItem>().AsNoTracking().AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarCodeId!.Value), ct);
    }

    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<SalesInvoiceItem>().AsNoTracking().AnyAsync(e => e.ProductId == id, ct);
    }

    public async Task<SalesInvoice?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<SalesInvoice>.GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<SalesInvoice?> GetByNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<SalesInvoice>.GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceWithDetailsSpec(invoiceNumber)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<SalesInvoice>> ListAsync(SalesInvoiceQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<SalesInvoice>.GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceListSpec(request.BranchId, request.WarehouseId, request.CustomerId, request.Status, request.PaymentMethod, request.FromDate, request.ToDate, request.Search)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<SalesInvoice>> SearchAsync(SalesInvoiceQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<SalesInvoice>.GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceListSpec(request.BranchId, request.WarehouseId, request.CustomerId, request.Status, request.PaymentMethod, request.FromDate, request.ToDate, request.Search)), request, ct);
    }

    public async Task<bool> NumberExistsAsync(string invoiceNumber, CancellationToken ct = default)
    {
        return await _context.Set<SalesInvoice>().AsNoTracking().AnyAsync(e => e.InvoiceNumber == invoiceNumber, ct);
    }

    public async Task<SalesInvoice?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<SalesInvoice>.GetQuery(_context.Set<SalesInvoice>(), new SalesInvoiceWithDetailsSpec(id)).FirstOrDefaultAsync(ct);
    }
}
