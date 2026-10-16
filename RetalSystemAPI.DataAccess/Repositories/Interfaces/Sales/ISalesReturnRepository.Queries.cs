using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface ISalesReturnRepository
{
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasInvoiceReferencesAsync(Guid id, CancellationToken ct = default);
    Task<SalesReturn?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<SalesReturn?> GetByNumberAsync(string returnNumber, CancellationToken ct = default);
    Task<IReadOnlyList<SalesReturn>> ListAsync(SalesReturnQuery request, CancellationToken ct = default);
    Task<PageResult<SalesReturn>> SearchAsync(SalesReturnQuery request, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string returnNumber, CancellationToken ct = default);
    Task<IReadOnlyList<SalesReturnItem>> GetReturnedItemsByInvoiceAndProductsAsync(Guid originalInvoiceId, IEnumerable<Guid> productIds, CancellationToken ct = default);
    Task<IReadOnlyList<SalesReturn>> GetByInvoiceAsync(Guid originalInvoiceId, CancellationToken ct = default);
    Task<SalesReturn?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
