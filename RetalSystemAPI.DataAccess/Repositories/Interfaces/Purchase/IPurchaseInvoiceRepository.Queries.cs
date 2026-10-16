using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface IPurchaseInvoiceRepository
{
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);
    Task<PageResult<PurchaseInvoice>> SearchAsync(PurchaseInvoiceQuery request, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string invoiceNumber, CancellationToken ct = default);
    Task<PurchaseInvoice?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseInvoiceItemBreakdown>> GetBreakdownsByItemIdsForUpdateAsync(IEnumerable<Guid> oldItemIds, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseInvoiceItem>> GetItemsByIdsForUpdateAsync(IEnumerable<Guid> oldItemIds, CancellationToken ct = default);
    Task<PurchaseInvoice?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseInvoiceReadResult?> GetReadResultAsync(Guid id, CancellationToken ct = default);
}
