using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Suppliers;

using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface ISupplierRepository
{
    Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default);
    Task<Supplier?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> ListAsync(SupplierQuery request, CancellationToken ct = default);
    Task<BalanceSummary> GetBalanceSummaryAsync(SupplierQuery request, CancellationToken ct = default);
    Task<PageResult<Supplier>> SearchAsync(SupplierQuery request, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);
    Task<bool> HasFinancialHistoryAsync(Guid id, CancellationToken ct = default);
}
