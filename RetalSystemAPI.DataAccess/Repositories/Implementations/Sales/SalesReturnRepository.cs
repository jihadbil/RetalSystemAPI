using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات فواتير مرتجع المبيعات وبنودها.
/// </summary>
public class SalesReturnRepository : Repository<SalesReturn>, ISalesReturnRepository
{
    public SalesReturnRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task AddItemAsync(SalesReturnItem item, CancellationToken ct = default)
    {
        await _context.Set<SalesReturnItem>().AddAsync(item, ct);
    }

    /// <inheritdoc />
    public async Task AddItemsAsync(IEnumerable<SalesReturnItem> items, CancellationToken ct = default)
    {
        await _context.Set<SalesReturnItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc />
    public void RemoveItem(SalesReturnItem item)
    {
        _context.Set<SalesReturnItem>().Remove(item);
    }

    /// <inheritdoc />
    public void HardDeleteItem(SalesReturnItem item)
    {
        var local = _context.Set<SalesReturnItem>().Local.FirstOrDefault(e => e.Id == item.Id);
        if (local != null)
        {
            _context.Set<SalesReturnItem>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(item);
            if (entry.State != EntityState.Detached)
                _context.Set<SalesReturnItem>().Remove(item);
            else
            {
                _context.Attach(item);
                _context.Set<SalesReturnItem>().Remove(item);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SalesReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturnItem>()
            .AsNoTracking()
            .Where(i => i.SalesReturnId == returnId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SalesReturnItem>> FindItemsAsync(Expression<Func<SalesReturnItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturnItem>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SalesReturnItem>> FindItemsTrackedAsync(Expression<Func<SalesReturnItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturnItem>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ItemExistsAsync(Expression<Func<SalesReturnItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<SalesReturnItem>().AnyAsync(predicate, ct);
    }
}
