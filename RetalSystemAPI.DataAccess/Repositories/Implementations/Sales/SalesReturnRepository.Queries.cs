using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class SalesReturnRepository
{
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturnItem>().AsNoTracking().AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarCodeId!.Value), ct);
    }

    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturnItem>().AsNoTracking().AnyAsync(e => e.ProductId == id, ct);
    }

    public async Task<bool> HasInvoiceReferencesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturn>().AsNoTracking().AnyAsync(e => e.OriginalInvoiceId == id, ct);
    }

    public async Task<SalesReturn?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<SalesReturn>.GetQuery(_context.Set<SalesReturn>(), new SalesReturnWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<SalesReturn?> GetByNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<SalesReturn>.GetQuery(_context.Set<SalesReturn>(), new SalesReturnWithDetailsSpec(returnNumber)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<SalesReturn>> ListAsync(SalesReturnQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<SalesReturn>.GetQuery(_context.Set<SalesReturn>(), new SalesReturnListSpec(request.BranchId, request.WarehouseId, request.CustomerId, request.Reason, request.FromDate, request.ToDate, request.Search)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<SalesReturn>> SearchAsync(SalesReturnQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<SalesReturn>.GetQuery(_context.Set<SalesReturn>(), new SalesReturnListSpec(request.BranchId, request.WarehouseId, request.CustomerId, request.Reason, request.FromDate, request.ToDate, request.Search)), request, ct);
    }

    public async Task<bool> NumberExistsAsync(string returnNumber, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturn>().AsNoTracking().AnyAsync(e => e.ReturnNumber == returnNumber, ct);
    }

    public async Task<IReadOnlyList<SalesReturnItem>> GetReturnedItemsByInvoiceAndProductsAsync(Guid originalInvoiceId, IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturnItem>().AsNoTracking().Where(e => e.SalesReturn != null && e.SalesReturn.OriginalInvoiceId == originalInvoiceId && productIds.Contains(e.ProductId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SalesReturn>> GetByInvoiceAsync(Guid originalInvoiceId, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturn>().AsNoTracking().Where(e => e.OriginalInvoiceId == originalInvoiceId).ToListAsync(ct);
    }

    public async Task<SalesReturn?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<SalesReturn>.GetQuery(_context.Set<SalesReturn>(), new SalesReturnWithDetailsSpec(id)).FirstOrDefaultAsync(ct);
    }
}
