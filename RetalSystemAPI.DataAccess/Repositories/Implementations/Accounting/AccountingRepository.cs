using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات الحسابات المالية وإدارة الخزائن والمصروفات وحركاتها.
/// </summary>
public class AccountingRepository : IAccountingRepository
{
    private readonly AppDbContext _context;
    public AccountingRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── الخزائن والبنوك (Safes) ─────────────────────────────────
    /// <inheritdoc/>
    public async Task<Safe?> GetSafeByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await EntityPersistence.GetByIdAsync<Safe>(_context, id, ct);
    }

    /// <inheritdoc/>
    public async Task<Safe?> GetSafeWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Safe>().Include(s => s.Branch).FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Safe>> GetAllSafesAsync(CancellationToken ct = default)
        => GetAllSafesAsync(null, null, null, ct);

    public async Task<IReadOnlyList<Safe>> GetAllSafesAsync(Guid? branchId, SafeType? type, bool? isActive, CancellationToken ct = default)
    {
        var query = _context.Set<Safe>().AsNoTracking().Include(s => s.Branch).AsQueryable();
        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId.Value);
        if (type.HasValue) query = query.Where(s => s.Type == type.Value);
        if (isActive.HasValue) query = query.Where(s => s.IsActive == isActive.Value);
        return await query.OrderBy(s => s.Name).ThenBy(s => s.Id).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<Safe>> SearchSafesAsync(SafeQuery request, CancellationToken ct = default)
    {
        var branchId = request.BranchId;
        var isActive = request.IsActive;
        var searchTerm = request.SearchTerm;
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;
        Pagination.Normalize(ref pageNumber, ref pageSize);
        var query = _context.Set<Safe>().AsNoTracking().Include(s => s.Branch).AsQueryable();
        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);
        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(s => s.Name.Contains(term) || (s.BankAccountNumber != null && s.BankAccountNumber.Contains(term)) || (s.BankName != null && s.BankName.Contains(term)));
        }

        int totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(s => s.Name).ThenBy(s => s.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PageResult<Safe>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<bool> SafeNameExistsAsync(string name, Guid branchId, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Set<Safe>().AnyAsync(s => s.BranchId == branchId && s.Name == name && (!excludeId.HasValue || s.Id != excludeId.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> SafeHasTransactionsAsync(Guid safeId, CancellationToken ct = default)
    {
        return await _context.Set<SafeTransaction>().AnyAsync(t => t.SafeId == safeId, ct);
    }

    /// <inheritdoc/>
    public async Task AddSafeAsync(Safe safe, CancellationToken ct = default)
    {
        await _context.Set<Safe>().AddAsync(safe, ct);
    }

    /// <inheritdoc/>
    public void UpdateSafe(Safe safe)
    {
        EntityPersistence.Update(_context, safe);
    }

    /// <inheritdoc/>
    public void SoftDeleteSafe(Safe safe)
    {
        EntityPersistence.SoftDelete(_context, safe);
    }

    // ── حركات الخزائن (Safe Transactions) ───────────────────────
    /// <inheritdoc/>
    public async Task AddSafeTransactionAsync(SafeTransaction transaction, CancellationToken ct = default)
    {
        await _context.Set<SafeTransaction>().AddAsync(transaction, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafeTransaction>> GetSafeTransactionsAsync(Guid safeId, CancellationToken ct = default)
    {
        return await _context.Set<SafeTransaction>().AsNoTracking().Include(t => t.Safe).Where(t => t.SafeId == safeId).OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedAt).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<SafeTransaction>> SearchSafeTransactionsAsync(SafeTransactionQuery request, CancellationToken ct = default)
    {
        var safeId = request.SafeId;
        var fromDate = request.FromDate;
        var toDate = request.ToDate;
        var direction = request.Direction;
        var documentType = request.DocumentType;
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;
        Pagination.Normalize(ref pageNumber, ref pageSize);
        var query = _context.Set<SafeTransaction>().AsNoTracking().Include(t => t.Safe).AsQueryable();
        if (safeId.HasValue)
            query = query.Where(t => t.SafeId == safeId.Value);
        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);
        if (direction.HasValue)
            query = query.Where(t => t.Direction == direction.Value);
        if (documentType.HasValue)
            query = query.Where(t => t.DocumentType == documentType.Value);
        int totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedAt).ThenBy(t => t.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PageResult<SafeTransaction>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<string> GenerateSafeTransactionNumberAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return $"TRX-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..38];
    }

    // ── تصنيفات المصروفات (Expense Categories) ───────────────────
    /// <inheritdoc/>
    public async Task<ExpenseCategory?> GetExpenseCategoryByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await EntityPersistence.GetByIdAsync<ExpenseCategory>(_context, id, ct);
    }

    /// <inheritdoc/>
    public async Task<ExpenseCategory?> GetExpenseCategoryWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ExpenseCategory>().Include(c => c.Expenses).FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExpenseCategory>> GetAllExpenseCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.Set<ExpenseCategory>().AsNoTracking().Include(c => c.Expenses).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<ExpenseCategory>> SearchExpenseCategoriesAsync(ExpenseCategoryQuery request, CancellationToken ct = default)
    {
        var isActive = request.IsActive;
        var searchTerm = request.SearchTerm;
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;
        Pagination.Normalize(ref pageNumber, ref pageSize);
        var query = _context.Set<ExpenseCategory>().AsNoTracking().Include(c => c.Expenses).AsQueryable();
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(c => c.Name.Contains(term));
        }

        int totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(c => c.Name).ThenBy(c => c.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PageResult<ExpenseCategory>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<bool> ExpenseCategoryNameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Set<ExpenseCategory>().AnyAsync(c => c.Name == name && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> ExpenseCategoryHasExpensesAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _context.Set<Expense>().AnyAsync(e => e.ExpenseCategoryId == categoryId, ct);
    }

    /// <inheritdoc/>
    public async Task AddExpenseCategoryAsync(ExpenseCategory category, CancellationToken ct = default)
    {
        await _context.Set<ExpenseCategory>().AddAsync(category, ct);
    }

    /// <inheritdoc/>
    public void UpdateExpenseCategory(ExpenseCategory category)
    {
        EntityPersistence.Update(_context, category);
    }

    /// <inheritdoc/>
    public void SoftDeleteExpenseCategory(ExpenseCategory category)
    {
        EntityPersistence.SoftDelete(_context, category);
    }

    // ── المصروفات (Expenses) ────────────────────────────────────
    /// <inheritdoc/>
    public async Task<Expense?> GetExpenseByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await EntityPersistence.GetByIdAsync<Expense>(_context, id, ct);
    }

    /// <inheritdoc/>
    public async Task<Expense?> GetExpenseWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Expense>().Include(e => e.Branch).Include(e => e.ExpenseCategory).Include(e => e.Safe).FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Expense>> GetAllExpensesAsync(CancellationToken ct = default)
    {
        return await _context.Set<Expense>().AsNoTracking().Include(e => e.Branch).Include(e => e.ExpenseCategory).Include(e => e.Safe).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<Expense>> SearchExpensesAsync(ExpenseQuery request, CancellationToken ct = default)
    {
        var branchId = request.BranchId;
        var categoryId = request.CategoryId;
        var safeId = request.SafeId;
        var paymentMethod = request.PaymentMethod;
        var fromDate = request.FromDate;
        var toDate = request.ToDate;
        var searchTerm = request.SearchTerm;
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;
        Pagination.Normalize(ref pageNumber, ref pageSize);
        var query = _context.Set<Expense>().AsNoTracking().Include(e => e.Branch).Include(e => e.ExpenseCategory).Include(e => e.Safe).AsQueryable();
        if (branchId.HasValue)
            query = query.Where(e => e.BranchId == branchId.Value);
        if (categoryId.HasValue)
            query = query.Where(e => e.ExpenseCategoryId == categoryId.Value);
        if (safeId.HasValue)
            query = query.Where(e => e.SafeId == safeId.Value);
        if (paymentMethod.HasValue)
            query = query.Where(e => e.PaymentMethod == paymentMethod.Value);
        if (fromDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(e => e.ExpenseNumber.Contains(term) || e.Description.Contains(term) || (e.BeneficiaryName != null && e.BeneficiaryName.Contains(term)));
        }

        int totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.CreatedAt).ThenBy(e => e.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PageResult<Expense>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<string> GenerateExpenseNumberAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var count = await _context.Set<Expense>().CountAsync(e => e.CreatedAt >= today, ct);
        var randomSuffix = Random.Shared.Next(100, 999);
        return $"EXP-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}-{randomSuffix}";
    }

    /// <inheritdoc/>
    public async Task<bool> ExpenseNumberExistsAsync(string expenseNumber, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Set<Expense>().AnyAsync(e => e.ExpenseNumber == expenseNumber && (!excludeId.HasValue || e.Id != excludeId.Value), ct);
    }

    /// <inheritdoc/>
    public async Task AddExpenseAsync(Expense expense, CancellationToken ct = default)
    {
        await _context.Set<Expense>().AddAsync(expense, ct);
    }

    /// <inheritdoc/>
    public void UpdateExpense(Expense expense)
    {
        EntityPersistence.Update(_context, expense);
    }

    /// <inheritdoc/>
    public void SoftDeleteExpense(Expense expense)
    {
        EntityPersistence.SoftDelete(_context, expense);
    }
}
