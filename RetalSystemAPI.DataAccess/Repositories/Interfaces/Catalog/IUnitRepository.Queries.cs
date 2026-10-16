using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface IUnitRepository
{
    Task<IReadOnlyList<UnitLookupRow>> GetLookupAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Unit>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default);
    Task<IReadOnlyList<Unit>> ListLookupAsync(CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);
}
