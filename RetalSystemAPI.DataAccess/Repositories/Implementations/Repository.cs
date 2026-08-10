using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// التنفيذ العام المستند إلى EF Core لواجهة IRepository.
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ── Queries ──────────────────────────────────────────────
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, ct);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<T>> GetAllTrackedAsync(CancellationToken ct = default)
    {
        return await _dbSet.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<T>> FindAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).AsNoTracking().ToListAsync(ct);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, ct);
    }

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(predicate, ct);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        return predicate is null
            ? await _dbSet.CountAsync(ct)
            : await _dbSet.CountAsync(predicate, ct);
    }

    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }
        return await query.CountAsync(ct);
    }

    // ── Pagination ────────────────────────────────────────────
    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        int totalCount = await query.CountAsync(ct);

        if (orderBy is not null)
        {
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        ISpecification<T> spec,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        int totalCount = await CountAsync(spec, ct);

        var query = ApplySpecification(spec).AsNoTracking().AsSplitQuery();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
    }


    // ── Commands ──────────────────────────────────────────────
    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        _dbSet.Update(entity);
    }

    public void HardDelete(T entity)
    {
        _dbSet.Remove(entity);
    }
}
