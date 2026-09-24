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
/// تنفيذ مستودع بيانات فواتير المشتريات وبنودها وتفاصيل النكهات المستلمة.
/// </summary>
public class PurchaseInvoiceRepository : Repository<PurchaseInvoice>, IPurchaseInvoiceRepository
{
    public PurchaseInvoiceRepository(AppDbContext context) : base(context)
    {
    }

    // ── بنود الفاتورة المجمعة (Invoice Items) ────────────────────

    /// <inheritdoc />
    public async Task AddItemAsync(PurchaseInvoiceItem item, CancellationToken ct = default)
    {
        await _context.Set<PurchaseInvoiceItem>().AddAsync(item, ct);
    }

    /// <inheritdoc />
    public async Task AddItemsAsync(IEnumerable<PurchaseInvoiceItem> items, CancellationToken ct = default)
    {
        await _context.Set<PurchaseInvoiceItem>().AddRangeAsync(items, ct);
    }

    /// <inheritdoc />
    public void RemoveItem(PurchaseInvoiceItem item)
    {
        _context.Set<PurchaseInvoiceItem>().Remove(item);
    }

    /// <inheritdoc />
    public void HardDeleteItem(PurchaseInvoiceItem item)
    {
        var local = _context.Set<PurchaseInvoiceItem>().Local.FirstOrDefault(e => e.Id == item.Id);
        if (local != null)
        {
            _context.Set<PurchaseInvoiceItem>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(item);
            if (entry.State != EntityState.Detached)
                _context.Set<PurchaseInvoiceItem>().Remove(item);
            else
            {
                _context.Attach(item);
                _context.Set<PurchaseInvoiceItem>().Remove(item);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PurchaseInvoiceItem>> GetItemsAsync(Guid invoiceId, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoiceItem>()
            .AsNoTracking()
            .Where(i => i.PurchaseInvoiceId == invoiceId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PurchaseInvoiceItem>> FindItemsTrackedAsync(Expression<Func<PurchaseInvoiceItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoiceItem>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ItemExistsAsync(Expression<Func<PurchaseInvoiceItem, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoiceItem>().AnyAsync(predicate, ct);
    }


    // ── تفاصيل استلام النكهات والباركودات (Item Breakdowns) ──────

    /// <inheritdoc />
    public async Task AddBreakdownAsync(PurchaseInvoiceItemBreakdown breakdown, CancellationToken ct = default)
    {
        await _context.Set<PurchaseInvoiceItemBreakdown>().AddAsync(breakdown, ct);
    }

    /// <inheritdoc />
    public async Task AddBreakdownsAsync(IEnumerable<PurchaseInvoiceItemBreakdown> breakdowns, CancellationToken ct = default)
    {
        await _context.Set<PurchaseInvoiceItemBreakdown>().AddRangeAsync(breakdowns, ct);
    }

    /// <inheritdoc />
    public void RemoveBreakdown(PurchaseInvoiceItemBreakdown breakdown)
    {
        _context.Set<PurchaseInvoiceItemBreakdown>().Remove(breakdown);
    }

    /// <inheritdoc />
    public void HardDeleteBreakdown(PurchaseInvoiceItemBreakdown breakdown)
    {
        var local = _context.Set<PurchaseInvoiceItemBreakdown>().Local.FirstOrDefault(e => e.Id == breakdown.Id);
        if (local != null)
        {
            _context.Set<PurchaseInvoiceItemBreakdown>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(breakdown);
            if (entry.State != EntityState.Detached)
                _context.Set<PurchaseInvoiceItemBreakdown>().Remove(breakdown);
            else
            {
                _context.Attach(breakdown);
                _context.Set<PurchaseInvoiceItemBreakdown>().Remove(breakdown);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PurchaseInvoiceItemBreakdown>> FindBreakdownsTrackedAsync(Expression<Func<PurchaseInvoiceItemBreakdown, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<PurchaseInvoiceItemBreakdown>()
            .Where(predicate)
            .ToListAsync(ct);
    }
}
