using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface ICustomerRepository
{
    Task<Customer?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> ListAsync(CustomerQuery request, CancellationToken ct = default);
    Task<PageResult<Customer>> SearchAsync(CustomerQuery request, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid id, CancellationToken ct = default);
    Task<bool> HasFinancialHistoryAsync(Guid id, CancellationToken ct = default);
}
