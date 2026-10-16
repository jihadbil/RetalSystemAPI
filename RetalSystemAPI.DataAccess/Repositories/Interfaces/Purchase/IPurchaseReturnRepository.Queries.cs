using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface IPurchaseReturnRepository
{
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasInvoiceReferencesAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseReturn?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseReturn?> GetByNumberAsync(string returnNumber, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseReturn>> ListAsync(PurchaseReturnQuery request, CancellationToken ct = default);
    Task<PageResult<PurchaseReturn>> SearchAsync(PurchaseReturnQuery request, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string returnNumber, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseReturnItem>> GetReturnedItemsByInvoiceAndBarcodeAsync(Guid originalInvoiceId, Guid barcodeId, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseReturnItem>> GetReturnedItemsByInvoiceAndBarcodesAsync(Guid originalInvoiceId, IEnumerable<Guid> returnBarcodeIds, CancellationToken ct = default);
    Task<PurchaseReturn?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
