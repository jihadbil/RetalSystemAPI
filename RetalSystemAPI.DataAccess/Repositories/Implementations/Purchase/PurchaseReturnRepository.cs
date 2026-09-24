using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات فواتير مرتجع المشتريات وبنودها.
/// </summary>
public class PurchaseReturnRepository : Repository<PurchaseReturn>, IPurchaseReturnRepository
{
    public PurchaseReturnRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task AddItemAsync(PurchaseReturnItem item, CancellationToken ct = default)
    {
        await _context.Set<PurchaseReturnItem>().AddAsync(item, ct);
    }

    /// <inheritdoc />
    public async Task AddItemsAsync(IEnumerable<PurchaseReturnItem> items, CancellationToken ct = default)
    {
        await _context.Set<PurchaseReturnItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc />
    public void RemoveItem(PurchaseReturnItem item)
    {
        _context.Set<PurchaseReturnItem>().Remove(item);
    }

    /// <inheritdoc />
    public void HardDeleteItem(PurchaseReturnItem item)
    {
        var local = _context.Set<PurchaseReturnItem>().Local.FirstOrDefault(e => e.Id == item.Id);
        if (local != null)
        {
            _context.Set<PurchaseReturnItem>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(item);
            if (entry.State != EntityState.Detached)
                _context.Set<PurchaseReturnItem>().Remove(item);
            else
            {
                _context.Attach(item);
                _context.Set<PurchaseReturnItem>().Remove(item);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PurchaseReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturnItem>()
            .AsNoTracking()
            .Where(i => i.PurchaseReturnId == returnId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PurchaseReturnItem>> FindItemsAsync(Expression<Func<PurchaseReturnItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturnItem>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PurchaseReturnItem>> FindItemsTrackedAsync(Expression<Func<PurchaseReturnItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturnItem>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ItemExistsAsync(Expression<Func<PurchaseReturnItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseReturnItem>().AnyAsync(predicate, ct);
    }
}
