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
/// تنفيذ مستودع بيانات فواتير المبيعات وبنودها.
/// </summary>
public class SalesInvoiceRepository : Repository<SalesInvoice>, ISalesInvoiceRepository
{
    public SalesInvoiceRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task AddItemAsync(SalesInvoiceItem item, CancellationToken ct = default)
    {
        await _context.Set<SalesInvoiceItem>().AddAsync(item, ct);
    }

    /// <inheritdoc />
    public async Task AddItemsAsync(IEnumerable<SalesInvoiceItem> items, CancellationToken ct = default)
    {
        await _context.Set<SalesInvoiceItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc />
    public void RemoveItem(SalesInvoiceItem item)
    {
        _context.Set<SalesInvoiceItem>().Remove(item);
    }

    /// <inheritdoc />
    public void HardDeleteItem(SalesInvoiceItem item)
    {
        var local = _context.Set<SalesInvoiceItem>().Local.FirstOrDefault(e => e.Id == item.Id);
        if (local != null)
        {
            _context.Set<SalesInvoiceItem>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(item);
            if (entry.State != EntityState.Detached)
                _context.Set<SalesInvoiceItem>().Remove(item);
            else
            {
                _context.Attach(item);
                _context.Set<SalesInvoiceItem>().Remove(item);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SalesInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default)
    {
        return await _context.Set<SalesInvoiceItem>()
            .AsNoTracking()
            .Where(i => i.SalesInvoiceId == invoiceId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SalesInvoiceItem>> FindItemsTrackedAsync(Expression<Func<SalesInvoiceItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<SalesInvoiceItem>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ItemExistsAsync(Expression<Func<SalesInvoiceItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<SalesInvoiceItem>().AnyAsync(predicate, ct);
    }
}
