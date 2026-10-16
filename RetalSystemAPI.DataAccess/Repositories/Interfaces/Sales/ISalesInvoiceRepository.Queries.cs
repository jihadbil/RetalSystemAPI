using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface ISalesInvoiceRepository
{
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);
    Task<SalesInvoice?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<SalesInvoice?> GetByNumberAsync(string invoiceNumber, CancellationToken ct = default);
    Task<IReadOnlyList<SalesInvoice>> ListAsync(SalesInvoiceQuery request, CancellationToken ct = default);
    Task<PageResult<SalesInvoice>> SearchAsync(SalesInvoiceQuery request, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string invoiceNumber, CancellationToken ct = default);
    Task<SalesInvoice?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
