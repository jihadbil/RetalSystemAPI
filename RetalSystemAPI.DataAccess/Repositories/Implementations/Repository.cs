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
/// التنفيذ العام المستند إلى Entity Framework Core لواجهة المستودع IRepository.
/// يوفر عمليات القراءة السريعة غير المتتبعة وعمليات الكتابة الآمنة مع معالجة الكيانات المحلية والمفصولة.
/// </summary>
/// <typeparam name="T">نوع الكيان الذي يرث من BaseEntity</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// بناء كائن المستودع وربطه بسياق قاعدة البيانات AppDbContext.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات</param>
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ── Queries ──────────────────────────────────────────────

    /// <summary>
    /// جلب كيان محدد بواسطة المعرف الأساسي (Primary Key).
    /// </summary>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, ct);
    }

    /// <summary>
    /// جلب جميع الكيانات غير المحذوفة كقائمة غير متتبعة للقراءة السريعة.
    /// </summary>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(ct);
    }

    /// <summary>
    /// جلب جميع الكيانات مع تفعيل التتبع في ChangeTracker لإجراء التعديلات.
    /// </summary>
    public async Task<IReadOnlyList<T>> GetAllTrackedAsync(CancellationToken ct = default)
    {
        return await _dbSet.ToListAsync(ct);
    }

    /// <summary>
    /// البحث عن الكيانات المطابقة لشرط الفلترة بدون تتبع.
    /// </summary>
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);
    }

    /// <summary>
    /// البحث عن الكيانات باستخدام مواصفة متقدمة (ISpecification) وتطبيق التضمينات والترتيب.
    /// </summary>
    public async Task<IReadOnlyList<T>> FindAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).AsNoTracking().ToListAsync(ct);
    }

    /// <summary>
    /// جلب أول كيان يطابق الشرط أو null.
    /// </summary>
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, ct);
    }

    /// <summary>
    /// جلب أول كيان يطابق المواصفة أو null.
    /// </summary>
    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// جلب أول كيان يطابق المواصفة مع تفعيل التتبع في ChangeTracker لإجراء التعديلات وحفظها بأمان.
    /// </summary>
    public async Task<T?> FirstOrDefaultTrackedAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// جلب أول كيان يطابق شرطاً مع تفعيل التتبع في ChangeTracker.
    /// </summary>
    public async Task<T?> FirstOrDefaultTrackedAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, ct);
    }

    /// <summary>
    /// البحث عن الكيانات المطابقة لشرط الفلترة مع تفعيل التتبع، لجلب دفعة كيانات
    /// سيتم تعديلها في استعلام واحد بدلاً من استعلام لكل كيان.
    /// </summary>
    public async Task<IReadOnlyList<T>> FindTrackedAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(ct);
    }

    /// <summary>
    /// تحميل أعمدة محددة فقط (Projection) دون الكيانات الكاملة لتفادي تحميل
    /// جداول كاملة في الذاكرة عند الحاجة لحقول قليلة.
    /// </summary>
    public async Task<IReadOnlyList<TResult>> SelectAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().Select(selector).ToListAsync(ct);
    }

    /// <summary>
    /// التحقق من وجود أي سجل يطابق الشرط المنطقي.
    /// </summary>
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(predicate, ct);
    }

    /// <summary>
    /// حساب عدد السجلات المطابقة لشرط اختياري.
    /// </summary>
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        return predicate is null
            ? await _dbSet.CountAsync(ct)
            : await _dbSet.CountAsync(predicate, ct);
    }

    /// <summary>
    /// حساب عدد السجلات المطابقة لمعايير المواصفة.
    /// </summary>
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

    /// <summary>
    /// جلب صفحة بيانات مجزأة مع الفلترة والترتيب التلقائي.
    /// </summary>
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

    /// <summary>
    /// جلب صفحة بيانات مجزأة بالاعتماد على مواصفة متقدمة (ISpecification).
    /// </summary>
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

    /// <summary>
    /// تطبيق معايير وتضمينات المواصفة على استعلام IQueryable عبر SpecificationEvaluator.
    /// </summary>
    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
    }

    // ── Commands ──────────────────────────────────────────────

    /// <summary>
    /// إضافة كيان جديد إلى سياق قاعدة البيانات.
    /// </summary>
    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    /// <summary>
    /// إضافة مجموعة من الكيانات الجديدة إلى سياق قاعدة البيانات.
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    /// <summary>
    /// تحديث بيانات الكيان مع معالجة الكيانات المتتبعة محلياً في الذاكرة لمنع أخطاء تضارب التتبع.
    /// </summary>
    public void Update(T entity)
    {
        var local = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, entity))
            {
                _context.Entry(local).CurrentValues.SetValues(entity);
            }
            else
            {
                _context.Entry(local).State = EntityState.Modified;
            }
            return;
        }

        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        entry.State = EntityState.Modified;
    }

    /// <summary>
    /// الحذف المنطقي للكيان بتعيين IsDeleted = true وتحديث حالته في سياق قاعدة البيانات.
    /// </summary>
    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        var local = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, entity))
            {
                _context.Entry(local).CurrentValues.SetValues(entity);
            }
            else
            {
                _context.Entry(local).State = EntityState.Modified;
            }
            return;
        }

        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        entry.State = EntityState.Modified;
    }

    /// <summary>
    /// الحذف الفعلي والنهائي للكيان من جدول قاعدة البيانات.
    /// </summary>
    public void HardDelete(T entity)
    {
        var local = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        if (local != null)
        {
            _dbSet.Remove(local);
        }
        else
        {
            var entry = _context.Entry(entity);
            if (entry.State != EntityState.Detached)
            {
                _dbSet.Remove(entity);
            }
            else
            {
                _context.Attach(entity);
                _dbSet.Remove(entity);
            }
        }
    }
}
