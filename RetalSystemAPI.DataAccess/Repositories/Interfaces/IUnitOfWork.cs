using System;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة Unit of Work لإدارة المنيات كـ Transaction واحدة على كافة المستودعات.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    // ── Repositories ──────────────────────────────────────────
    IRepository<Tenant> Tenants { get; }
    IRepository<Branch> Branches { get; }
    IRepository<BranchPhone> BranchPhones { get; }
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<ProductUnit> ProductUnits { get; }
    IRepository<ProductBarCode> ProductBarCodes { get; }
    IRepository<ProductImage> ProductImages { get; }
    IRepository<Unit> Units { get; }

    // ── Transaction Control ───────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
