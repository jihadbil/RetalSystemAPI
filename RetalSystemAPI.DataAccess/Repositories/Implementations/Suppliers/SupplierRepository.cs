using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات الموردين وهواتفهم وحركات كشف حساباتهم.
/// </summary>
public partial class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(AppDbContext context) : base(context)
    {
    }

    // ── هواتف المورد (Supplier Phones) ──────────────────────────
    /// <inheritdoc/>
    public async Task AddPhoneAsync(SupplierPhone phone, CancellationToken ct = default)
    {
        await _context.Set<SupplierPhone>().AddAsync(phone, ct);
    }

    /// <inheritdoc/>
    public async Task AddPhonesAsync(IEnumerable<SupplierPhone> phones, CancellationToken ct = default)
    {
        await _context.Set<SupplierPhone>().AddRangeAsync(phones, ct);
    }

    /// <inheritdoc/>
    public void UpdatePhone(SupplierPhone phone)
    {
        EntityPersistence.Update(_context, phone);
    }

    /// <inheritdoc/>
    public void RemovePhone(SupplierPhone phone)
    {
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void SoftDeletePhone(SupplierPhone phone)
    {
        EntityPersistence.SoftDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void HardDeletePhone(SupplierPhone phone)
    {
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public async Task<SupplierPhone?> GetPhoneByIdAsync(Guid phoneId, CancellationToken ct = default)
    {
        return await EntityPersistence.GetByIdAsync<SupplierPhone>(_context, phoneId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SupplierPhone>> GetPhonesAsync(Guid supplierId, CancellationToken ct = default)
    {
        return await _context.Set<SupplierPhone>().AsNoTracking().Where(p => p.SupplierId == supplierId).ToListAsync(ct);
    }

    // ── حركات كشف حساب المورد (Supplier Transactions) ───────────
    /// <inheritdoc/>
    public async Task AddTransactionAsync(SupplierTransaction transaction, CancellationToken ct = default)
    {
        await _context.Set<SupplierTransaction>().AddAsync(transaction, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SupplierTransaction>> GetTransactionsAsync(Guid supplierId, CancellationToken ct = default)
    {
        return await _context.Set<SupplierTransaction>().AsNoTracking().Where(t => t.SupplierId == supplierId).OrderByDescending(t => t.TransactionDate).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SupplierTransaction>> GetTransactionsByDateRangeAsync(Guid supplierId, DateTime? fromDate, DateTime? toDate, Guid? branchId = null, CancellationToken ct = default)
    {
        var query = _context.Set<SupplierTransaction>().AsNoTracking().Include(t => t.Branch).Where(t => t.SupplierId == supplierId);
        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        if (toDate.HasValue)
        {
            var end = toDate.Value;
            if (end.TimeOfDay == TimeSpan.Zero && end.Date < DateTime.MaxValue.Date)
            {
                var exclusiveEnd = end.AddDays(1);
                query = query.Where(t => t.TransactionDate < exclusiveEnd);
            }
            else
            {
                if (end.TimeOfDay == TimeSpan.Zero) end = DateTime.MaxValue;
                query = query.Where(t => t.TransactionDate <= end);
            }
        }
        if (branchId.HasValue)
            query = query.Where(t => t.BranchId == branchId.Value);
        return await query.OrderBy(t => t.TransactionDate).ThenBy(t => t.CreatedAt).ThenBy(t => t.Id).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<decimal> GetBalanceBeforeDateAsync(Guid supplierId, DateTime fromDate, CancellationToken ct = default)
    {
        var supplier = await _context.Set<Supplier>().AsNoTracking().Where(s => s.Id == supplierId).Select(s => new { s.OpeningBalance }).FirstOrDefaultAsync(ct);
        var delta = await _context.Set<SupplierTransaction>().AsNoTracking()
            .Where(t => t.SupplierId == supplierId && t.TransactionDate < fromDate)
            .SumAsync(t => (decimal?)(t.CreditAmount - t.DebitAmount), ct) ?? 0m;
        return (supplier?.OpeningBalance ?? 0m) + delta;
    }
}
