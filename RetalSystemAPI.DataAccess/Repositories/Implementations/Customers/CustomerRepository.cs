using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات العملاء والزبائن وهواتفهم وحركات كشف حساباتهم.
/// </summary>
public partial class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }

    // ── هواتف العميل (Customer Phones) ──────────────────────────
    /// <inheritdoc/>
    public async Task AddPhoneAsync(CustomerPhone phone, CancellationToken ct = default)
    {
        await _context.Set<CustomerPhone>().AddAsync(phone, ct);
    }

    /// <inheritdoc/>
    public async Task AddPhonesAsync(IEnumerable<CustomerPhone> phones, CancellationToken ct = default)
    {
        await _context.Set<CustomerPhone>().AddRangeAsync(phones, ct);
    }

    /// <inheritdoc/>
    public void UpdatePhone(CustomerPhone phone)
    {
        EntityPersistence.Update(_context, phone);
    }

    /// <inheritdoc/>
    public void RemovePhone(CustomerPhone phone)
    {
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void SoftDeletePhone(CustomerPhone phone)
    {
        EntityPersistence.SoftDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void HardDeletePhone(CustomerPhone phone)
    {
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public async Task<CustomerPhone?> GetPhoneByIdAsync(Guid phoneId, CancellationToken ct = default)
    {
        return await EntityPersistence.GetByIdAsync<CustomerPhone>(_context, phoneId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CustomerPhone>> GetPhonesAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Set<CustomerPhone>().AsNoTracking().Where(p => p.CustomerId == customerId).ToListAsync(ct);
    }

    // ── حركات كشف حساب العميل (Customer Transactions) ───────────
    /// <inheritdoc/>
    public async Task AddTransactionAsync(CustomerTransaction transaction, CancellationToken ct = default)
    {
        await _context.Set<CustomerTransaction>().AddAsync(transaction, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CustomerTransaction>> GetTransactionsAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Set<CustomerTransaction>().AsNoTracking().Where(t => t.CustomerId == customerId).OrderByDescending(t => t.TransactionDate).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CustomerTransaction>> GetTransactionsByDateRangeAsync(Guid customerId, DateTime? fromDate, DateTime? toDate, Guid? branchId = null, CancellationToken ct = default)
    {
        var query = _context.Set<CustomerTransaction>().AsNoTracking().Include(t => t.Branch).Where(t => t.CustomerId == customerId);
        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);
        if (branchId.HasValue)
            query = query.Where(t => t.BranchId == branchId.Value);
        return await query.OrderBy(t => t.TransactionDate).ThenBy(t => t.CreatedAt).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<decimal> GetBalanceBeforeDateAsync(Guid customerId, DateTime fromDate, CancellationToken ct = default)
    {
        var lastTx = await _context.Set<CustomerTransaction>().AsNoTracking().Where(t => t.CustomerId == customerId && t.TransactionDate < fromDate).OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedAt).FirstOrDefaultAsync(ct);
        if (lastTx != null)
            return lastTx.BalanceAfter;
        var customer = await _context.Set<Customer>().AsNoTracking().Where(c => c.Id == customerId).Select(c => new { c.OpeningBalance }).FirstOrDefaultAsync(ct);
        return customer?.OpeningBalance ?? 0m;
    }
}
