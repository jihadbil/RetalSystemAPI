using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface IPurchaseOrderRepository
{
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);
    Task<PurchaseOrder?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseOrder>> ListAsync(PurchaseOrderQuery request, CancellationToken ct = default);
    Task<PageResult<PurchaseOrder>> SearchAsync(PurchaseOrderQuery request, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string orderNumber, CancellationToken ct = default);
    Task<PurchaseOrder?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
