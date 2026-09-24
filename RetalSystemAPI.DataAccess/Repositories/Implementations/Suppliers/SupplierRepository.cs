using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات الموردين وهواتفهم وحركات كشف حساباتهم.
/// </summary>
public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(AppDbContext context) : base(context)
    {
    }

    // ── هواتف المورد (Supplier Phones) ──────────────────────────

    /// <inheritdoc />
    public async Task AddPhoneAsync(SupplierPhone phone, CancellationToken ct = default)
    {
        await _context.Set<SupplierPhone>().AddAsync(phone, ct);
    }

    /// <inheritdoc />
    public async Task AddPhonesAsync(IEnumerable<SupplierPhone> phones, CancellationToken ct = default)
    {
        await _context.Set<SupplierPhone>().AddRangeAsync(phones, ct);
    }

    /// <inheritdoc />
    public void UpdatePhone(SupplierPhone phone)
    {
        var local = _context.Set<SupplierPhone>().Local.FirstOrDefault(e => e.Id == phone.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, phone))
                _context.Entry(local).CurrentValues.SetValues(phone);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(phone);
        if (entry.State == EntityState.Detached)
            _context.Set<SupplierPhone>().Attach(phone);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void RemovePhone(SupplierPhone phone)
    {
        _context.Set<SupplierPhone>().Remove(phone);
    }

    /// <inheritdoc />
    public void SoftDeletePhone(SupplierPhone phone)
    {
        phone.IsDeleted = true;
        var local = _context.Set<SupplierPhone>().Local.FirstOrDefault(e => e.Id == phone.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, phone))
                _context.Entry(local).CurrentValues.SetValues(phone);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(phone);
        if (entry.State == EntityState.Detached)
            _context.Set<SupplierPhone>().Attach(phone);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void HardDeletePhone(SupplierPhone phone)
    {
        var local = _context.Set<SupplierPhone>().Local.FirstOrDefault(e => e.Id == phone.Id);
        if (local != null)
        {
            _context.Set<SupplierPhone>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(phone);
            if (entry.State != EntityState.Detached)
                _context.Set<SupplierPhone>().Remove(phone);
            else
            {
                _context.Attach(phone);
                _context.Set<SupplierPhone>().Remove(phone);
            }
        }
    }

    /// <inheritdoc />
    public async Task<SupplierPhone?> GetPhoneByIdAsync(Guid phoneId, CancellationToken ct = default)
    {
        return await _context.Set<SupplierPhone>().FindAsync(new object[] { phoneId }, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SupplierPhone>> GetPhonesAsync(Guid supplierId, CancellationToken ct = default)
    {
        return await _context.Set<SupplierPhone>()
            .AsNoTracking()
            .Where(p => p.SupplierId == supplierId)
            .ToListAsync(ct);
    }


    // ── حركات كشف حساب المورد (Supplier Transactions) ───────────

    /// <inheritdoc />
    public async Task AddTransactionAsync(SupplierTransaction transaction, CancellationToken ct = default)
    {
        await _context.Set<SupplierTransaction>().AddAsync(transaction, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SupplierTransaction>> GetTransactionsAsync(Guid supplierId, CancellationToken ct = default)
    {
        return await _context.Set<SupplierTransaction>()
            .AsNoTracking()
            .Where(t => t.SupplierId == supplierId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SupplierTransaction>> GetTransactionsByDateRangeAsync(
        Guid supplierId,
        DateTime? fromDate,
        DateTime? toDate,
        Guid? branchId = null,
        CancellationToken ct = default)
    {
        var query = _context.Set<SupplierTransaction>()
            .AsNoTracking()
            .Include(t => t.Branch)
            .Where(t => t.SupplierId == supplierId);

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        if (branchId.HasValue)
            query = query.Where(t => t.BranchId == branchId.Value);

        return await query
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<decimal> GetBalanceBeforeDateAsync(Guid supplierId, DateTime fromDate, CancellationToken ct = default)
    {
        var lastTx = await _context.Set<SupplierTransaction>()
            .AsNoTracking()
            .Where(t => t.SupplierId == supplierId && t.TransactionDate < fromDate)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (lastTx != null)
            return lastTx.BalanceAfter;

        var supplier = await _context.Set<Supplier>()
            .AsNoTracking()
            .Where(s => s.Id == supplierId)
            .Select(s => new { s.OpeningBalance })
            .FirstOrDefaultAsync(ct);

        return supplier?.OpeningBalance ?? 0m;
    }
}
