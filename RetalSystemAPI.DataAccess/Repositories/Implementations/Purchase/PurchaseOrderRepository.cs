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
/// تنفيذ مستودع بيانات طلبات الشراء وبنودها.
/// </summary>
public class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task AddItemAsync(PurchaseOrderItem item, CancellationToken ct = default)
    {
        await _context.Set<PurchaseOrderItem>().AddAsync(item, ct);
    }

    /// <inheritdoc />
    public async Task AddItemsAsync(IEnumerable<PurchaseOrderItem> items, CancellationToken ct = default)
    {
        await _context.Set<PurchaseOrderItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc />
    public void UpdateItem(PurchaseOrderItem item)
    {
        var local = _context.Set<PurchaseOrderItem>().Local.FirstOrDefault(e => e.Id == item.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, item))
                _context.Entry(local).CurrentValues.SetValues(item);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(item);
        if (entry.State == EntityState.Detached)
            _context.Set<PurchaseOrderItem>().Attach(item);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void RemoveItem(PurchaseOrderItem item)
    {
        _context.Set<PurchaseOrderItem>().Remove(item);
    }

    /// <inheritdoc />
    public void HardDeleteItem(PurchaseOrderItem item)
    {
        var local = _context.Set<PurchaseOrderItem>().Local.FirstOrDefault(e => e.Id == item.Id);
        if (local != null)
        {
            _context.Set<PurchaseOrderItem>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(item);
            if (entry.State != EntityState.Detached)
                _context.Set<PurchaseOrderItem>().Remove(item);
            else
            {
                _context.Attach(item);
                _context.Set<PurchaseOrderItem>().Remove(item);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PurchaseOrderItem>> GetItemsAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseOrderItem>()
            .AsNoTracking()
            .Where(i => i.PurchaseOrderId == orderId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PurchaseOrderItem>> FindItemsTrackedAsync(Expression<Func<PurchaseOrderItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseOrderItem>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ItemExistsAsync(Expression<Func<PurchaseOrderItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseOrderItem>().AnyAsync(predicate, ct);
    }
}
