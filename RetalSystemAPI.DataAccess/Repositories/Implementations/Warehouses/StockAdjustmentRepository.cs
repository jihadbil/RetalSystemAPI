using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات تسويات الجرد المخزني وبنودها.
/// </summary>
public class StockAdjustmentRepository : Repository<StockAdjustment>, IStockAdjustmentRepository
{
    public StockAdjustmentRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task AddItemAsync(StockAdjustmentItem item, CancellationToken ct = default)
    {
        await _context.Set<StockAdjustmentItem>().AddAsync(item, ct);
    }

    /// <inheritdoc />
    public async Task AddItemsAsync(IEnumerable<StockAdjustmentItem> items, CancellationToken ct = default)
    {
        await _context.Set<StockAdjustmentItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc />
    public void RemoveItem(StockAdjustmentItem item)
    {
        _context.Set<StockAdjustmentItem>().Remove(item);
    }

    /// <inheritdoc />
    public void HardDeleteItem(StockAdjustmentItem item)
    {
        var local = _context.Set<StockAdjustmentItem>().Local.FirstOrDefault(e => e.Id == item.Id);
        if (local != null)
        {
            _context.Set<StockAdjustmentItem>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(item);
            if (entry.State != EntityState.Detached)
                _context.Set<StockAdjustmentItem>().Remove(item);
            else
            {
                _context.Attach(item);
                _context.Set<StockAdjustmentItem>().Remove(item);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StockAdjustmentItem>> GetItemsAsync(Guid adjustmentId, CancellationToken ct = default)
    {
        return await _context.Set<StockAdjustmentItem>()
            .AsNoTracking()
            .Where(i => i.StockAdjustmentId == adjustmentId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StockAdjustmentItem>> FindItemsTrackedAsync(Expression<Func<StockAdjustmentItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<StockAdjustmentItem>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ItemExistsAsync(Expression<Func<StockAdjustmentItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<StockAdjustmentItem>().AnyAsync(predicate, ct);
    }
}
