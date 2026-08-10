using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع عامة توفر العمليات الأساسية لقراءة وكتابة البيانات في قاعدة البيانات.
/// </summary>
/// <typeparam name="T">نوع الكيان المراد التعامل معه، ويجب أن يرث من BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    // ── Queries ──────────────────────────────────────────────
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllTrackedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);

    // ── Pagination ────────────────────────────────────────────
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken ct = default);

    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        ISpecification<T> spec,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);


    // ── Commands ──────────────────────────────────────────────
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void SoftDelete(T entity);
    void HardDelete(T entity);
}
