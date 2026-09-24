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
/// تنفيذ مستودع بيانات أوامر التحويل المخزني وبنودها.
/// </summary>
public class StockTransferRepository : Repository<StockTransfer>, IStockTransferRepository
{
    public StockTransferRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task AddItemAsync(StockTransferItem item, CancellationToken ct = default)
    {
        await _context.Set<StockTransferItem>().AddAsync(item, ct);
    }

    /// <inheritdoc />
    public async Task AddItemsAsync(IEnumerable<StockTransferItem> items, CancellationToken ct = default)
    {
        await _context.Set<StockTransferItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc />
    public void RemoveItem(StockTransferItem item)
    {
        _context.Set<StockTransferItem>().Remove(item);
    }

    /// <inheritdoc />
    public void HardDeleteItem(StockTransferItem item)
    {
        var local = _context.Set<StockTransferItem>().Local.FirstOrDefault(e => e.Id == item.Id);
        if (local != null)
        {
            _context.Set<StockTransferItem>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(item);
            if (entry.State != EntityState.Detached)
                _context.Set<StockTransferItem>().Remove(item);
            else
            {
                _context.Attach(item);
                _context.Set<StockTransferItem>().Remove(item);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StockTransferItem>> GetItemsAsync(Guid transferId, CancellationToken ct = default)
    {
        return await _context.Set<StockTransferItem>()
            .AsNoTracking()
            .Where(i => i.StockTransferId == transferId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StockTransferItem>> FindItemsTrackedAsync(Expression<Func<StockTransferItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<StockTransferItem>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ItemExistsAsync(Expression<Func<StockTransferItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<StockTransferItem>().AnyAsync(predicate, ct);
    }
}
