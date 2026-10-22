using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface IBranchRepository
{
    Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsActiveForTenantAsync(Guid branchId, Guid currentTenantId, CancellationToken ct = default);
    Task<Branch?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Branch>> ListAsync(BranchQuery request, CancellationToken ct = default);
    Task<PageResult<Branch>> SearchAsync(BranchQuery request, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);
    Task<Branch?> GetDetailsForDeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasOperationalHistoryAsync(Guid id, CancellationToken ct = default);
}
