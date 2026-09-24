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
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات أرصدة المخازن الرئيسية وصالات العرض المترابطة.
/// </summary>
public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<StorgeStock> _storageStocks;
    private readonly DbSet<ShowroomStock> _showroomStocks;

    public StockRepository(AppDbContext context)
    {
        _context = context;
        _storageStocks = context.Set<StorgeStock>();
        _showroomStocks = context.Set<ShowroomStock>();
    }

    // ── أرصدة المستودعات الرئيسية (StorgeStock) ─────────────────

    /// <inheritdoc />
    public async Task<StorgeStock?> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        return await _storageStocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductBarcodeId == productBarcodeId, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StorgeStock>> GetStorgeStocksAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StorgeStock>
            .GetQuery(_storageStocks.AsQueryable(), spec)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StorgeStock>> FindStorgeStocksAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _storageStocks
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StorgeStock>> FindStorgeStocksAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StorgeStock>
            .GetQuery(_storageStocks.AsQueryable(), spec)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StorgeStock>> FindStorgeStockTrackedAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _storageStocks
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StorgeStock>> GetAllStorgeStocksTrackedAsync(CancellationToken ct = default)
    {
        return await _storageStocks.ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<StorgeStock?> FirstOrDefaultStorgeStockAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StorgeStock>
            .GetQuery(_storageStocks.AsQueryable(), spec)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<StorgeStock?> FirstOrDefaultStorgeStockAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _storageStocks
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<StorgeStock?> FirstOrDefaultStorgeStockTrackedAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StorgeStock>
            .GetQuery(_storageStocks.AsQueryable(), spec)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<StorgeStock?> FirstOrDefaultStorgeStockTrackedAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _storageStocks.FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task AddStorgeStockAsync(StorgeStock stock, CancellationToken ct = default)
    {
        await _storageStocks.AddAsync(stock, ct);
    }

    /// <inheritdoc />
    public async Task AddStorgeStocksAsync(IEnumerable<StorgeStock> stocks, CancellationToken ct = default)
    {
        await _storageStocks.AddRangeAsync(stocks, ct);
    }

    /// <inheritdoc />
    public void UpdateStorgeStock(StorgeStock stock)
    {
        var local = _storageStocks.Local.FirstOrDefault(e => e.Id == stock.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, stock))
                _context.Entry(local).CurrentValues.SetValues(stock);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(stock);
        if (entry.State == EntityState.Detached)
            _storageStocks.Attach(stock);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void SoftDeleteStorgeStock(StorgeStock stock)
    {
        stock.IsDeleted = true;
        var local = _storageStocks.Local.FirstOrDefault(e => e.Id == stock.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, stock))
                _context.Entry(local).CurrentValues.SetValues(stock);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(stock);
        if (entry.State == EntityState.Detached)
            _storageStocks.Attach(stock);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void HardDeleteStorgeStock(StorgeStock stock)
    {
        var local = _storageStocks.Local.FirstOrDefault(e => e.Id == stock.Id);
        if (local != null)
        {
            _storageStocks.Remove(local);
        }
        else
        {
            var entry = _context.Entry(stock);
            if (entry.State != EntityState.Detached)
                _storageStocks.Remove(stock);
            else
            {
                _context.Attach(stock);
                _storageStocks.Remove(stock);
            }
        }
    }

    /// <inheritdoc />
    public async Task<bool> StorgeStockExistsAsync(Expression<Func<StorgeStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _storageStocks.AnyAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<int> CountStorgeStockAsync(ISpecification<StorgeStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<StorgeStock>
            .GetQuery(_storageStocks.AsQueryable(), spec)
            .CountAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TResult>> SelectStorgeStocksAsync<TResult>(Expression<Func<StorgeStock, TResult>> selector, CancellationToken ct = default)
    {
        return await _storageStocks
            .AsNoTracking()
            .Select(selector)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<StorgeStock> Items, int TotalCount)> GetPagedStorgeStockAsync(
        ISpecification<StorgeStock> spec,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        Pagination.Normalize(ref pageNumber, ref pageSize);

        int totalCount = await CountStorgeStockAsync(spec, ct);
        var query = SpecificationEvaluator<StorgeStock>
            .GetQuery(_storageStocks.AsQueryable(), spec)
            .AsNoTracking()
            .AsSplitQuery();

        if (!spec.IsPagingEnabled)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        var items = await query.ToListAsync(ct);

        return (items, totalCount);
    }


    // ── أرصدة صالات العرض (ShowroomStock) ────────────────────────

    /// <inheritdoc />
    public async Task<ShowroomStock?> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        return await _showroomStocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShowroomStock>> GetShowroomStocksAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ShowroomStock>
            .GetQuery(_showroomStocks.AsQueryable(), spec)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShowroomStock>> FindShowroomStocksAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _showroomStocks
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShowroomStock>> FindShowroomStocksAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ShowroomStock>
            .GetQuery(_showroomStocks.AsQueryable(), spec)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShowroomStock>> FindShowroomStockTrackedAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _showroomStocks
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShowroomStock>> GetAllShowroomStocksTrackedAsync(CancellationToken ct = default)
    {
        return await _showroomStocks.ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ShowroomStock?> FirstOrDefaultShowroomStockAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ShowroomStock>
            .GetQuery(_showroomStocks.AsQueryable(), spec)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ShowroomStock?> FirstOrDefaultShowroomStockAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _showroomStocks
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<ShowroomStock?> FirstOrDefaultShowroomStockTrackedAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ShowroomStock>
            .GetQuery(_showroomStocks.AsQueryable(), spec)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ShowroomStock?> FirstOrDefaultShowroomStockTrackedAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _showroomStocks.FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task AddShowroomStockAsync(ShowroomStock stock, CancellationToken ct = default)
    {
        await _showroomStocks.AddAsync(stock, ct);
    }

    /// <inheritdoc />
    public async Task AddShowroomStocksAsync(IEnumerable<ShowroomStock> stocks, CancellationToken ct = default)
    {
        await _showroomStocks.AddRangeAsync(stocks, ct);
    }

    /// <inheritdoc />
    public void UpdateShowroomStock(ShowroomStock stock)
    {
        var local = _showroomStocks.Local.FirstOrDefault(e => e.Id == stock.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, stock))
                _context.Entry(local).CurrentValues.SetValues(stock);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(stock);
        if (entry.State == EntityState.Detached)
            _showroomStocks.Attach(stock);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void SoftDeleteShowroomStock(ShowroomStock stock)
    {
        stock.IsDeleted = true;
        var local = _showroomStocks.Local.FirstOrDefault(e => e.Id == stock.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, stock))
                _context.Entry(local).CurrentValues.SetValues(stock);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(stock);
        if (entry.State == EntityState.Detached)
            _showroomStocks.Attach(stock);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void HardDeleteShowroomStock(ShowroomStock stock)
    {
        var local = _showroomStocks.Local.FirstOrDefault(e => e.Id == stock.Id);
        if (local != null)
        {
            _showroomStocks.Remove(local);
        }
        else
        {
            var entry = _context.Entry(stock);
            if (entry.State != EntityState.Detached)
                _showroomStocks.Remove(stock);
            else
            {
                _context.Attach(stock);
                _showroomStocks.Remove(stock);
            }
        }
    }

    /// <inheritdoc />
    public async Task<bool> ShowroomStockExistsAsync(Expression<Func<ShowroomStock, bool>> predicate, CancellationToken ct = default)
    {
        return await _showroomStocks.AnyAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<int> CountShowroomStockAsync(ISpecification<ShowroomStock> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ShowroomStock>
            .GetQuery(_showroomStocks.AsQueryable(), spec)
            .CountAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TResult>> SelectShowroomStocksAsync<TResult>(Expression<Func<ShowroomStock, TResult>> selector, CancellationToken ct = default)
    {
        return await _showroomStocks
            .AsNoTracking()
            .Select(selector)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<ShowroomStock> Items, int TotalCount)> GetPagedShowroomStockAsync(
        ISpecification<ShowroomStock> spec,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        Pagination.Normalize(ref pageNumber, ref pageSize);

        int totalCount = await CountShowroomStockAsync(spec, ct);
        var query = SpecificationEvaluator<ShowroomStock>
            .GetQuery(_showroomStocks.AsQueryable(), spec)
            .AsNoTracking()
            .AsSplitQuery();

        if (!spec.IsPagingEnabled)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        var items = await query.ToListAsync(ct);

        return (items, totalCount);
    }
}
