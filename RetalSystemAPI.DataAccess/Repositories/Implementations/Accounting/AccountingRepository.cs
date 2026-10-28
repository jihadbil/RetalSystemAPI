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
/// تنفيذ مستودع بيانات الحسابات المالية وإدارة الخزائن والمصروفات وحركاتها المالية.
/// </summary>
public class AccountingRepository : IAccountingRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// تهيئة نسخة جديدة من مستودع الحسابات بحقن سياق قاعدة البيانات.
    /// </summary>
    /// <param name="context">سياق قاعدة بيانات النظام للوصول إلى جداول الخزائن والمصروفات.</param>
    public AccountingRepository(AppDbContext context)
    {
        // تعيين سياق قاعدة البيانات
        _context = context;
    }

    // ── الخزائن والبنوك (Safes) ─────────────────────────────────

    /// <inheritdoc/>
    public async Task<Safe?> GetSafeByIdAsync(Guid id, CancellationToken ct = default)
    {
        // جلب الخزينة بالمعرف عبر معالج استمرارية الكيانات
        return await EntityPersistence.GetByIdAsync<Safe>(_context, id, ct);
    }

    /// <inheritdoc/>
    public async Task<Safe?> GetSafeWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // جلب الخزينة متضمنة بيانات الفرع التابعة له
        return await _context.Set<Safe>()
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Safe>> GetAllSafesAsync(CancellationToken ct = default)
        => GetAllSafesAsync(null, null, null, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Safe>> GetAllSafesAsync(Guid? branchId, SafeType? type, bool? isActive, CancellationToken ct = default)
    {
        // تجهيز استعلام الخزائن مع بيانات الفرع ودون تتبع
        var query = _context.Set<Safe>().AsNoTracking().Include(s => s.Branch).AsQueryable();

        // فلترة بحسب الفرع إن تم تحديده
        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId.Value);

        // فلترة بحسب نوع الخزينة إن وجد
        if (type.HasValue) query = query.Where(s => s.Type == type.Value);

        // فلترة بحسب حالة التنشيط إن تم تحديدها
        if (isActive.HasValue) query = query.Where(s => s.IsActive == isActive.Value);

        // ترتيب الخزائن أبجدياً بحسب الاسم ثم المعرف
        return await query.OrderBy(s => s.Name).ThenBy(s => s.Id).ToListAsync(ct);
    }

    /// <summary>
    /// استرجاع قائمة الخزائن وفق كائن الاستعلام.
    /// </summary>
    /// <param name="request">معايير البحث في الخزائن.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الخزائن المطابقة.</returns>
    public async Task<IReadOnlyList<Safe>> ListSafesAsync(SafeQuery request, CancellationToken ct = default)
        => await BuildSafes(request).OrderBy(s => s.Name).ThenBy(s => s.Id).ToListAsync(ct);

    /// <summary>
    /// بناء استعلام الخزائن المفلتر المشترك بين التصفية والقوائم والبحث المصفح.
    /// </summary>
    /// <param name="request">معايير الاستعلام.</param>
    /// <returns>استعلام الخزائن القابل للتنفيذ.</returns>
    private IQueryable<Safe> BuildSafes(SafeQuery request)
    {
        // بدء الاستعلام كقراءة فقط وتضمين ملاحة الفرع
        var query = _context.Set<Safe>().AsNoTracking().Include(s => s.Branch).AsQueryable();

        // تطبيق شرط الفرع
        if (request.BranchId.HasValue) query = query.Where(s => s.BranchId == request.BranchId.Value);

        // تطبيق شرط حالة التنشيط
        if (request.IsActive.HasValue) query = query.Where(s => s.IsActive == request.IsActive.Value);

        // تطبيق شرط نوع الخزينة
        if (request.Type.HasValue) query = query.Where(s => s.Type == request.Type.Value);

        // تطبيق البحث النصي في الاسم ورقم الحساب واسم البنك
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(s => s.Name.Contains(term) || (s.BankAccountNumber != null && s.BankAccountNumber.Contains(term)) || (s.BankName != null && s.BankName.Contains(term)));
        }

        // إرجاع الاستعلام النهائي بعد بناء الشروط
        return query;
    }

    /// <inheritdoc/>
    public async Task<PageResult<Safe>> SearchSafesAsync(SafeQuery request, CancellationToken ct = default)
    {
        // استخراج أرقام الصفحات وأحجامها
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;

        // توحيد وضبط قيم الصفحات الافتراضية
        Pagination.Normalize(ref pageNumber, ref pageSize);

        // بناء استعلام الفلترة
        var query = BuildSafes(request);

        // احتساب إجمالي عدد السجلات المطابقة
        int totalCount = await query.CountAsync(ct);

        // تطبيق الترتيب وتخطي الصفحات وأخذ الحجم المحدد
        var items = await query.OrderBy(s => s.Name).ThenBy(s => s.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        // إرجاع كائن الصفحة المكتمل
        return new PageResult<Safe>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<bool> SafeNameExistsAsync(string name, Guid branchId, Guid? excludeId = null, CancellationToken ct = default)
    {
        // التحقق من تكرار اسم الخزينة لنفس الفرع مع استثناء المعرف الحالي إن وجد
        return await _context.Set<Safe>().AnyAsync(s => s.BranchId == branchId && s.Name == name && (!excludeId.HasValue || s.Id != excludeId.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> SafeHasTransactionsAsync(Guid safeId, CancellationToken ct = default)
    {
        // التحقق من وجود أي حركة مسجلة في الخزينة
        return await _context.Set<SafeTransaction>().AnyAsync(t => t.SafeId == safeId, ct);
    }

    /// <inheritdoc/>
    public async Task AddSafeAsync(Safe safe, CancellationToken ct = default)
    {
        // إضافة الخزينة الجديدة إلى سياق البيانات
        await _context.Set<Safe>().AddAsync(safe, ct);
    }

    /// <inheritdoc/>
    public void UpdateSafe(Safe safe)
    {
        // تحديث بيانات الخزينة مع التحقق من التزامن والمستأجر
        EntityPersistence.Update(_context, safe);
    }

    /// <inheritdoc/>
    public void SoftDeleteSafe(Safe safe)
    {
        // حذف الخزينة منطقياً ونقلها للمحذوفات
        EntityPersistence.SoftDelete(_context, safe);
    }

    // ── حركات الخزائن (Safe Transactions) ───────────────────────

    /// <inheritdoc/>
    public async Task AddSafeTransactionAsync(SafeTransaction transaction, CancellationToken ct = default)
    {
        // إضافة حركة الخزينة الجديدة إلى قاعدة البيانات
        await _context.Set<SafeTransaction>().AddAsync(transaction, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafeTransaction>> GetSafeTransactionsAsync(Guid safeId, CancellationToken ct = default)
    {
        // استرجاع حركات الخزينة مرتبة تنازلياً بحسب تاريخ الحركة ثم وقت الإنشاء دون تتبع
        return await _context.Set<SafeTransaction>()
            .AsNoTracking()
            .Include(t => t.Safe)
            .Where(t => t.SafeId == safeId)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<SafeTransaction>> SearchSafeTransactionsAsync(SafeTransactionQuery request, CancellationToken ct = default)
    {
        // استخراج معايير الفلترة
        var safeId = request.SafeId;
        var fromDate = request.FromDate;
        var toDate = request.ToDate;
        var direction = request.Direction;
        var documentType = request.DocumentType;
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;

        // ضبط وضمان صحة قيم أرقام وأحجام الصفحات
        Pagination.Normalize(ref pageNumber, ref pageSize);

        // إنشاء استعلام الحركات مع تضمين بيانات الخزينة ودون تتبع
        var query = _context.Set<SafeTransaction>().AsNoTracking().Include(t => t.Safe).AsQueryable();

        // تصفية حسب الخزينة إن حددت
        if (safeId.HasValue)
            query = query.Where(t => t.SafeId == safeId.Value);

        // تصفية من تاريخ
        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        // تصفية إلى تاريخ
        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        // تصفية باتجاه الحركة (إيداع / سحب)
        if (direction.HasValue)
            query = query.Where(t => t.Direction == direction.Value);

        // تصفية بنوع المستند المالي المرتبط
        if (documentType.HasValue)
            query = query.Where(t => t.DocumentType == documentType.Value);

        // احتساب إجمالي عدد الحركات المطابقة
        int totalCount = await query.CountAsync(ct);

        // تطبيق الترتيب التنازلي المزدوج وتخطي وأخذ عناصر الصفحة
        var items = await query.OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // إرجاع النتيجة المقسمة لصفحات
        return new PageResult<SafeTransaction>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<string> GenerateSafeTransactionNumberAsync(CancellationToken ct = default)
    {
        // استكمال مهمة غير متزامنة للتوافق مع الواجهة
        await Task.CompletedTask;

        // توليد رقم حركة فريد مكون من البادئة والتاريخ والأجزاء العشوائية بطول لا يتجاوز 38 حرفاً
        return $"TRX-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..38];
    }

    // ── تصنيفات المصروفات (Expense Categories) ───────────────────

    /// <inheritdoc/>
    public async Task<ExpenseCategory?> GetExpenseCategoryByIdAsync(Guid id, CancellationToken ct = default)
    {
        // جلب تصنيف المصروفات بالمعرف
        return await EntityPersistence.GetByIdAsync<ExpenseCategory>(_context, id, ct);
    }

    /// <inheritdoc/>
    public async Task<ExpenseCategory?> GetExpenseCategoryWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // جلب تصنيف المصروفات متضمناً قائمة المصروفات التابعة له
        return await _context.Set<ExpenseCategory>()
            .Include(c => c.Expenses)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExpenseCategory>> GetAllExpenseCategoriesAsync(CancellationToken ct = default)
    {
        // استرجاع كافة تصنيفات المصروفات مع بيانات مصروفاتها كقراءة فقط
        return await _context.Set<ExpenseCategory>()
            .AsNoTracking()
            .Include(c => c.Expenses)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<ExpenseCategory>> SearchExpenseCategoriesAsync(ExpenseCategoryQuery request, CancellationToken ct = default)
    {
        // استخراج بارامترات البحث
        var isActive = request.IsActive;
        var searchTerm = request.SearchTerm;
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;

        // ضبط ترقيم الصفحات
        Pagination.Normalize(ref pageNumber, ref pageSize);

        // بناء استعلام تصنيفات المصروفات مع تضمين قائمة المصروفات
        var query = _context.Set<ExpenseCategory>().AsNoTracking().Include(c => c.Expenses).AsQueryable();

        // فلترة بحسب حالة التنشيط
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        // فلترة بالبحث النصي في الاسم
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(c => c.Name.Contains(term));
        }

        // حساب إجمالي السجلات المطابقة
        int totalCount = await query.CountAsync(ct);

        // جلب عناصر الصفحة المرتبة بالاسم ثم المعرف
        var items = await query.OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // إرجاع كائن الصفحة
        return new PageResult<ExpenseCategory>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<bool> ExpenseCategoryNameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        // التحقق من تكرار اسم تصنيف المصروفات مع استبعاد المعرف الحالي إن وجد
        return await _context.Set<ExpenseCategory>().AnyAsync(c => c.Name == name && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> ExpenseCategoryHasExpensesAsync(Guid categoryId, CancellationToken ct = default)
    {
        // التحقق من وجود أي مصروف مسجل تحت هذا التصنيف لمنع حذفه
        return await _context.Set<Expense>().AnyAsync(e => e.ExpenseCategoryId == categoryId, ct);
    }

    /// <inheritdoc/>
    public async Task AddExpenseCategoryAsync(ExpenseCategory category, CancellationToken ct = default)
    {
        // إضافة تصنيف المصروفات الجديد
        await _context.Set<ExpenseCategory>().AddAsync(category, ct);
    }

    /// <inheritdoc/>
    public void UpdateExpenseCategory(ExpenseCategory category)
    {
        // تحديث تصنيف المصروفات في قاعدة البيانات
        EntityPersistence.Update(_context, category);
    }

    /// <inheritdoc/>
    public void SoftDeleteExpenseCategory(ExpenseCategory category)
    {
        // الحذف المنطقي لتصنيف المصروفات
        EntityPersistence.SoftDelete(_context, category);
    }

    // ── المصروفات (Expenses) ────────────────────────────────────

    /// <inheritdoc/>
    public async Task<Expense?> GetExpenseByIdAsync(Guid id, CancellationToken ct = default)
    {
        // جلب المصروف بالمعرف عبر معالج استمرارية الكيانات
        return await EntityPersistence.GetByIdAsync<Expense>(_context, id, ct);
    }

    /// <inheritdoc/>
    public async Task<Expense?> GetExpenseWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // جلب تفاصيل المصروف مع الفرع وتصنيف المصروف والخزينة المسحوب منها
        return await _context.Set<Expense>()
            .Include(e => e.Branch)
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Safe)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Expense>> GetAllExpensesAsync(CancellationToken ct = default)
    {
        // جلب كافة سندات المصروفات مع بيانات ملاحاتها كقراءة فقط
        return await _context.Set<Expense>()
            .AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Safe)
            .ToListAsync(ct);
    }

    /// <summary>
    /// بناء استعلام المصروفات المفلتر المشترك بين حساب الإجمالي والبحث المصفح.
    /// </summary>
    /// <param name="request">معايير فلترة المصروفات.</param>
    /// <returns>استعلام المصروفات القابل للتنفيذ.</returns>
    private IQueryable<Expense> BuildExpenses(ExpenseQuery request)
    {
        // استخراج قيم الفلترة
        var branchId = request.BranchId;
        var categoryId = request.CategoryId;
        var safeId = request.SafeId;
        var paymentMethod = request.PaymentMethod;
        var fromDate = request.FromDate;
        var toDate = request.ToDate;
        var searchTerm = request.SearchTerm;

        // بدء الاستعلام كقراءة فقط وتضمين الكيانات المرتبطة
        var query = _context.Set<Expense>()
            .AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Safe)
            .AsQueryable();

        // فلترة بالفرع
        if (branchId.HasValue)
            query = query.Where(e => e.BranchId == branchId.Value);

        // فلترة بالتصنيف
        if (categoryId.HasValue)
            query = query.Where(e => e.ExpenseCategoryId == categoryId.Value);

        // فلترة بالخزينة
        if (safeId.HasValue)
            query = query.Where(e => e.SafeId == safeId.Value);

        // فلترة بطريقة الدفع
        if (paymentMethod.HasValue)
            query = query.Where(e => e.PaymentMethod == paymentMethod.Value);

        // فلترة من تاريخ
        if (fromDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= fromDate.Value);

        // فلترة إلى تاريخ
        if (toDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= toDate.Value);

        // تطبيق البحث النصي في رقم المصروف والبيان واسم المستفيد
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(e => e.ExpenseNumber.Contains(term) || e.Description.Contains(term) || (e.BeneficiaryName != null && e.BeneficiaryName.Contains(term)));
        }

        // إرجاع الاستعلام المبني
        return query;
    }

    /// <inheritdoc/>
    public async Task<decimal> GetExpenseTotalAsync(ExpenseQuery request, CancellationToken ct = default)
    {
        // احتساب مجموع مبالغ المصروفات المطابقة لشروط الفلترة
        return await BuildExpenses(request).SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;
    }

    /// <inheritdoc/>
    public async Task<PageResult<Expense>> SearchExpensesAsync(ExpenseQuery request, CancellationToken ct = default)
    {
        // استخراج أرقام وأحجام الصفحات
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;

        // توحيد بارامترات التقسيم لصفحات
        Pagination.Normalize(ref pageNumber, ref pageSize);

        // بناء استعلام الفلترة للمصروفات
        var query = BuildExpenses(request);

        // حساب إجمالي عدد المصروفات المطابقة
        int totalCount = await query.CountAsync(ct);

        // تطبيق الترتيب التنازلي للتاريخ ثم وقت الإنشاء والمعرف وتخطي العناصر السابقة
        var items = await query.OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // إرجاع نتائج الصفحة
        return new PageResult<Expense>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<string> GenerateExpenseNumberAsync(CancellationToken ct = default)
    {
        // احتساب تاريخ بداية اليوم بتوقيت UTC
        var today = DateTime.UtcNow.Date;

        // إحصاء عدد سندات المصروفات التي أنشئت في نفس اليوم
        var count = await _context.Set<Expense>().CountAsync(e => e.CreatedAt >= today, ct);

        // توليد لاحقة عشوائية لمنع التضارب بين العمليات المتزامنة
        var randomSuffix = Random.Shared.Next(100, 999);

        // تشكيل رقم سند المصروف وفق النمط المعتمد
        return $"EXP-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}-{randomSuffix}";
    }

    /// <inheritdoc/>
    public async Task<bool> ExpenseNumberExistsAsync(string expenseNumber, Guid? excludeId = null, CancellationToken ct = default)
    {
        // التحقق من تكرار رقم سند المصروف مع استثناء المعرف الحالي إن وجد
        return await _context.Set<Expense>().AnyAsync(e => e.ExpenseNumber == expenseNumber && (!excludeId.HasValue || e.Id != excludeId.Value), ct);
    }

    /// <inheritdoc/>
    public async Task AddExpenseAsync(Expense expense, CancellationToken ct = default)
    {
        // إضافة سند المصروف إلى سياق البيانات
        await _context.Set<Expense>().AddAsync(expense, ct);
    }

    /// <inheritdoc/>
    public void UpdateExpense(Expense expense)
    {
        // تحديث بيانات سند المصروف عبر معالج استمرارية الكيانات
        EntityPersistence.Update(_context, expense);
    }

    /// <inheritdoc/>
    public void SoftDeleteExpense(Expense expense)
    {
        // الحذف المنطقي لسند المصروف بنقله للمحذوفات
        EntityPersistence.SoftDelete(_context, expense);
    }
}
