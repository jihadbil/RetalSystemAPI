using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Accounting;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
/// <summary>
/// واجهة مستودع بيانات الحسابات المالية وإدارة الخزائن والمصروفات وحركاتها.
/// </summary>
public interface IAccountingRepository
{
    // ── الخزائن والبنوك (Safes) ─────────────────────────────────
    /// <summary>جلب خزينة بواسطة معرفها الفريد</summary>
    Task<Safe?> GetSafeByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>جلب خزينة مع تفاصيل الفرع والحركات</summary>
    Task<Safe?> GetSafeWithDetailsAsync(Guid id, CancellationToken ct = default);
    /// <summary>جلب كافة الخزائن</summary>
    Task<IReadOnlyList<Safe>> GetAllSafesAsync(CancellationToken ct = default);
    /// <summary>جلب صفحة خزائن مجزأة مع الفلترة</summary>
    Task<PageResult<Safe>> SearchSafesAsync(SafeQuery request, CancellationToken ct = default);
    /// <summary>التحقق من وجود خزينة بنفس الاسم في الفرع</summary>
    Task<bool> SafeNameExistsAsync(string name, Guid branchId, Guid? excludeId = null, CancellationToken ct = default);
    /// <summary>التحقق مما إذا كانت الخزينة تحتوي على أي حركات مالية</summary>
    Task<bool> SafeHasTransactionsAsync(Guid safeId, CancellationToken ct = default);
    /// <summary>إضافة خزينة جديدة</summary>
    Task AddSafeAsync(Safe safe, CancellationToken ct = default);
    /// <summary>تحديث بيانات خزينة</summary>
    void UpdateSafe(Safe safe);
    /// <summary>حذف ناعم لخزينة</summary>
    void SoftDeleteSafe(Safe safe);
    // ── حركات الخزائن (Safe Transactions) ───────────────────────
    /// <summary>إضافة حركة مالية في الخزينة</summary>
    Task AddSafeTransactionAsync(SafeTransaction transaction, CancellationToken ct = default);
    /// <summary>جلب حركات خزينة محددة</summary>
    Task<IReadOnlyList<SafeTransaction>> GetSafeTransactionsAsync(Guid safeId, CancellationToken ct = default);
    /// <summary>جلب صفحة حركات الخزائن مجزأة مع الفلترة والترتيب التنازلي</summary>
    Task<PageResult<SafeTransaction>> SearchSafeTransactionsAsync(SafeTransactionQuery request, CancellationToken ct = default);
    /// <summary>توليد رقم حركة مالي فريد</summary>
    Task<string> GenerateSafeTransactionNumberAsync(CancellationToken ct = default);
    // ── تصنيفات المصروفات (Expense Categories) ───────────────────
    /// <summary>جلب تصنيف مصروفات بواسطة معرفه</summary>
    Task<ExpenseCategory?> GetExpenseCategoryByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>جلب تصنيف مصروفات مع المصروفات المرتبطة</summary>
    Task<ExpenseCategory?> GetExpenseCategoryWithDetailsAsync(Guid id, CancellationToken ct = default);
    /// <summary>جلب كافة تصنيفات المصروفات</summary>
    Task<IReadOnlyList<ExpenseCategory>> GetAllExpenseCategoriesAsync(CancellationToken ct = default);
    /// <summary>جلب صفحة تصنيفات المصروفات مجزأة مع الفلترة</summary>
    Task<PageResult<ExpenseCategory>> SearchExpenseCategoriesAsync(ExpenseCategoryQuery request, CancellationToken ct = default);
    /// <summary>التحقق من وجود تصنيف مصروفات بنفس الاسم</summary>
    Task<bool> ExpenseCategoryNameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    /// <summary>التحقق مما إذا كان التصنيف مرتبط بمصروفات مسجلة</summary>
    Task<bool> ExpenseCategoryHasExpensesAsync(Guid categoryId, CancellationToken ct = default);
    /// <summary>إضافة تصنيف مصروفات جديد</summary>
    Task AddExpenseCategoryAsync(ExpenseCategory category, CancellationToken ct = default);
    /// <summary>تحديث تصنيف مصروفات</summary>
    void UpdateExpenseCategory(ExpenseCategory category);
    /// <summary>حذف ناعم لتصنيف مصروفات</summary>
    void SoftDeleteExpenseCategory(ExpenseCategory category);
    // ── المصروفات (Expenses) ────────────────────────────────────
    /// <summary>جلب مصروف بواسطة معرفه</summary>
    Task<Expense?> GetExpenseByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>جلب مصروف مع التفاصيل (الفرع، التصنيف، الخزينة)</summary>
    Task<Expense?> GetExpenseWithDetailsAsync(Guid id, CancellationToken ct = default);
    /// <summary>جلب كافة المصروفات</summary>
    Task<IReadOnlyList<Expense>> GetAllExpensesAsync(CancellationToken ct = default);
    /// <summary>جلب صفحة مصروفات مجزأة مع الفلترة المتقدمة</summary>
    Task<PageResult<Expense>> SearchExpensesAsync(ExpenseQuery request, CancellationToken ct = default);
    /// <summary>توليد رقم سند مصروف فريد</summary>
    Task<string> GenerateExpenseNumberAsync(CancellationToken ct = default);
    /// <summary>التحقق من وجود سند مصروف بنفس الرقم</summary>
    Task<bool> ExpenseNumberExistsAsync(string expenseNumber, Guid? excludeId = null, CancellationToken ct = default);
    /// <summary>إضافة مصروف جديد</summary>
    Task AddExpenseAsync(Expense expense, CancellationToken ct = default);
    /// <summary>تحديث بيانات مصروف</summary>
    void UpdateExpense(Expense expense);
    /// <summary>حذف ناعم لمصروف</summary>
    void SoftDeleteExpense(Expense expense);
}
