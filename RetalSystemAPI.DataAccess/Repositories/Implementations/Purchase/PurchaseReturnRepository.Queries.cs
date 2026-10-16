using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class PurchaseReturnRepository
{
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturnItem>().AsNoTracking().AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarCodeId!.Value), ct);
    }

    public async Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturnItem>().AsNoTracking().AnyAsync(e => e.ProductId == id, ct);
    }

    public async Task<bool> HasInvoiceReferencesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturn>().AsNoTracking().AnyAsync(e => e.PurchaseInvoiceId == id, ct);
    }

    public async Task<PurchaseReturn?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<PurchaseReturn>.GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<PurchaseReturn?> GetByNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<PurchaseReturn>.GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnWithDetailsSpec(returnNumber)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<PurchaseReturn>> ListAsync(PurchaseReturnQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<PurchaseReturn>.GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnListSpec(request.BranchId, request.WarehouseId, request.SupplierId, request.Reason, request.PaymentMethod, request.FromDate, request.ToDate, request.Search)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<PurchaseReturn>> SearchAsync(PurchaseReturnQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<PurchaseReturn>.GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnListSpec(request.BranchId, request.WarehouseId, request.SupplierId, request.Reason, request.PaymentMethod, request.FromDate, request.ToDate, request.Search)), request, ct);
    }

    public async Task<bool> NumberExistsAsync(string returnNumber, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturn>().AsNoTracking().AnyAsync(e => e.ReturnNumber == returnNumber, ct);
    }

    public async Task<IReadOnlyList<PurchaseReturnItem>> GetReturnedItemsByInvoiceAndBarcodeAsync(Guid originalInvoiceId, Guid barcodeId, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturnItem>().AsNoTracking().Where(e => e.PurchaseReturn != null && e.PurchaseReturn.PurchaseInvoiceId == originalInvoiceId && e.ProductBarCodeId == barcodeId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PurchaseReturnItem>> GetReturnedItemsByInvoiceAndBarcodesAsync(Guid originalInvoiceId, IEnumerable<Guid> returnBarcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturnItem>().AsNoTracking().Where(e => e.PurchaseReturn != null && e.PurchaseReturn.PurchaseInvoiceId == originalInvoiceId && e.ProductBarCodeId.HasValue && returnBarcodeIds.Contains(e.ProductBarCodeId.Value)).ToListAsync(ct);
    }

    public async Task<PurchaseReturn?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<PurchaseReturn>.GetQuery(_context.Set<PurchaseReturn>(), new PurchaseReturnWithDetailsSpec(id)).FirstOrDefaultAsync(ct);
    }
}
