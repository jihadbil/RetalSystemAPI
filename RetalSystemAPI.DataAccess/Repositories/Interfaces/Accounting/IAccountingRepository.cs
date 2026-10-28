using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات الحسابات المالية وإدارة الخزائن والمصروفات وحركاتها المالية.
/// </summary>
public interface IAccountingRepository
{
    // ── الخزائن والبنوك (Safes) ─────────────────────────────────

    /// <summary>
    /// جلب بيانات خزينة أو حساب بنكي بواسطة معرفها الفريد.
    /// </summary>
    /// <param name="id">معرف الخزينة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان الخزينة أو null في حال عدم العثور عليها.</returns>
    Task<Safe?> GetSafeByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب بيانات خزينة مفصلة مع الفرع التابعة له.
    /// </summary>
    /// <param name="id">معرف الخزينة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان الخزينة متضمناً الفرع أو null.</returns>
    Task<Safe?> GetSafeWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة الخزائن والحسابات البنكية للمستأجر دون فلترة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بكافة الخزائن.</returns>
    Task<IReadOnlyList<Safe>> GetAllSafesAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة الخزائن والحسابات البنكية وفق معايير الفرع والنوع وحالة التنشيط.
    /// </summary>
    /// <param name="branchId">معرف الفرع الاختياري للتصفية.</param>
    /// <param name="type">نوع الخزينة (نقدية، بنكية، إلخ).</param>
    /// <param name="isActive">حالة تنشيط الخزينة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الخزائن المطابقة.</returns>
    Task<IReadOnlyList<Safe>> GetAllSafesAsync(Guid? branchId, SafeType? type, bool? isActive, CancellationToken ct = default);

    /// <summary>
    /// البحث في الخزائن والحسابات البنكية مع تقسيم الصفحات والفرز.
    /// </summary>
    /// <param name="request">معايير البحث والاستعلام والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة المحددة من الخزائن.</returns>
    Task<PageResult<Safe>> SearchSafesAsync(SafeQuery request, CancellationToken ct = default);

    /// <summary>
    /// التحقق من تكرار اسم الخزينة داخل نفس الفرع.
    /// </summary>
    /// <param name="name">اسم الخزينة.</param>
    /// <param name="branchId">معرف الفرع.</param>
    /// <param name="excludeId">معرف الخزينة المستثناة عند التعديل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الاسم مستخدماً مسبقاً في الفرع، وإلا False.</returns>
    Task<bool> SafeNameExistsAsync(string name, Guid branchId, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// التحقق مما إذا كانت الخزينة تحتوي على أي حركات مالية مسجلة تمنع حذفها.
    /// </summary>
    /// <param name="safeId">معرف الخزينة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجدت حركات مالية، وإلا False.</returns>
    Task<bool> SafeHasTransactionsAsync(Guid safeId, CancellationToken ct = default);

    /// <summary>
    /// إضافة خزينة أو حساب بنكي جديد إلى قاعدة البيانات.
    /// </summary>
    /// <param name="safe">كيان الخزينة المراد إضافته.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddSafeAsync(Safe safe, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات الخزينة أو الحساب البنكي.
    /// </summary>
    /// <param name="safe">كيان الخزينة المحدث.</param>
    void UpdateSafe(Safe safe);

    /// <summary>
    /// الحذف المنطقي للخزينة بنقلها إلى سلة المحذوفات.
    /// </summary>
    /// <param name="safe">كيان الخزينة المراد حذفها منطقياً.</param>
    void SoftDeleteSafe(Safe safe);

    // ── حركات الخزائن (Safe Transactions) ───────────────────────

    /// <summary>
    /// تسجيل حركة إيداع أو سحب أو تحويل مالي في الخزينة.
    /// </summary>
    /// <param name="transaction">كيان الحركة المالية.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddSafeTransactionAsync(SafeTransaction transaction, CancellationToken ct = default);

    /// <summary>
    /// جلب سجل الحركات المالية لخزينة محددة مرتبة تنازلياً بحسب تاريخ الحركة.
    /// </summary>
    /// <param name="safeId">معرف الخزينة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الحركات المالية في الخزينة.</returns>
    Task<IReadOnlyList<SafeTransaction>> GetSafeTransactionsAsync(Guid safeId, CancellationToken ct = default);

    /// <summary>
    /// البحث في حركات الخزائن المالية مع تطبيق الفلاتر والتقسيم لصفحات.
    /// </summary>
    /// <param name="request">معايير فلترة حركات الخزائن والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة المحددة من الحركات المالية.</returns>
    Task<PageResult<SafeTransaction>> SearchSafeTransactionsAsync(SafeTransactionQuery request, CancellationToken ct = default);

    /// <summary>
    /// توليد رقم حركة مالي تسلسلي فريد لسجلات الخزينة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>رقم الحركة الفريد بصيغة نصية.</returns>
    Task<string> GenerateSafeTransactionNumberAsync(CancellationToken ct = default);

    // ── تصنيفات المصروفات (Expense Categories) ───────────────────

    /// <summary>
    /// جلب تصنيف مصروفات بواسطة معرفه الفريد.
    /// </summary>
    /// <param name="id">معرف تصنيف المصروفات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان التصنيف أو null.</returns>
    Task<ExpenseCategory?> GetExpenseCategoryByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب تصنيف مصروفات متضمناً قائمة المصروفات التابعة له.
    /// </summary>
    /// <param name="id">معرف التصنيف.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان التصنيف بالمصروفات أو null.</returns>
    Task<ExpenseCategory?> GetExpenseCategoryWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة تصنيفات المصروفات للمستأجر مع بنود المصروفات كقراءة فقط.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة تصنيفات المصروفات المتاحة.</returns>
    Task<IReadOnlyList<ExpenseCategory>> GetAllExpenseCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// البحث في تصنيفات المصروفات مع تقسيم الصفحات والفرز.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة المحددة من تصنيفات المصروفات.</returns>
    Task<PageResult<ExpenseCategory>> SearchExpenseCategoriesAsync(ExpenseCategoryQuery request, CancellationToken ct = default);

    /// <summary>
    /// التحقق من تكرار اسم تصنيف المصروفات للمستأجر.
    /// </summary>
    /// <param name="name">اسم تصنيف المصروفات.</param>
    /// <param name="excludeId">معرف التصنيف المستثنى عند التعديل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الاسم مستخدماً مسبقاً، وإلا False.</returns>
    Task<bool> ExpenseCategoryNameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// التحقق مما إذا كان تصنيف المصروفات مرتبطاً بسجلات مصروفات فعلية تمنع حذفه.
    /// </summary>
    /// <param name="categoryId">معرف تصنيف المصروفات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا ارتبطت به مصروفات، وإلا False.</returns>
    Task<bool> ExpenseCategoryHasExpensesAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>
    /// إضافة تصنيف مصروفات جديد.
    /// </summary>
    /// <param name="category">كيان التصنيف المراد حفظه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddExpenseCategoryAsync(ExpenseCategory category, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات تصنيف مصروفات مسجل.
    /// </summary>
    /// <param name="category">كيان التصنيف المحدث.</param>
    void UpdateExpenseCategory(ExpenseCategory category);

    /// <summary>
    /// الحذف المنطقي لتصنيف المصروفات بنقله للمحذوفات.
    /// </summary>
    /// <param name="category">كيان التصنيف المراد حذفه.</param>
    void SoftDeleteExpenseCategory(ExpenseCategory category);

    // ── المصروفات (Expenses) ────────────────────────────────────

    /// <summary>
    /// جلب سند مصروف بواسطة معرفه الفريد.
    /// </summary>
    /// <param name="id">معرف المصروف.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان سند المصروف أو null.</returns>
    Task<Expense?> GetExpenseByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب سند المصروف مع بيانات الفرع وتصنيف المصروف والخزينة المصروف منها.
    /// </summary>
    /// <param name="id">معرف المصروف.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان المصروف المفصل أو null.</returns>
    Task<Expense?> GetExpenseWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة سندات المصروفات للمستأجر مع تفاصيلها.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بكافة سندات المصروفات.</returns>
    Task<IReadOnlyList<Expense>> GetAllExpensesAsync(CancellationToken ct = default);

    /// <summary>
    /// احتساب إجمالي مبالغ المصروفات المطابقة لمعايير الفلترة المحددة.
    /// </summary>
    /// <param name="request">معايير تصفية المصروفات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>إجمالي المبالغ المصروفة كقيمة عشرية.</returns>
    Task<decimal> GetExpenseTotalAsync(ExpenseQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في سندات المصروفات مع تقسيم الصفحات والفرز التنازلي بحسب التاريخ.
    /// </summary>
    /// <param name="request">معايير البحث والاستعلام والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة المحددة من سندات المصروفات.</returns>
    Task<PageResult<Expense>> SearchExpensesAsync(ExpenseQuery request, CancellationToken ct = default);

    /// <summary>
    /// توليد رقم سند مصروف تسلسلي فريد لليوم الحالي.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>رقم سند المصروف المتسلسل الفريد.</returns>
    Task<string> GenerateExpenseNumberAsync(CancellationToken ct = default);

    /// <summary>
    /// التحقق من تكرار رقم سند المصروف في قاعدة البيانات.
    /// </summary>
    /// <param name="expenseNumber">رقم سند المصروف.</param>
    /// <param name="excludeId">معرف المصروف المستثنى عند التعديل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الرقم مسجلاً مسبقاً، وإلا False.</returns>
    Task<bool> ExpenseNumberExistsAsync(string expenseNumber, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// إضافة سند مصروف جديد إلى قاعدة البيانات.
    /// </summary>
    /// <param name="expense">كيان سند المصروف المراد إضافته.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddExpenseAsync(Expense expense, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات سند مصروف مسجل.
    /// </summary>
    /// <param name="expense">كيان سند المصروف المحدث.</param>
    void UpdateExpense(Expense expense);

    /// <summary>
    /// الحذف المنطقي لسند المصروف بنقله للمحذوفات.
    /// </summary>
    /// <param name="expense">كيان سند المصروف المراد حذفه منطقياً.</param>
    void SoftDeleteExpense(Expense expense);
}
