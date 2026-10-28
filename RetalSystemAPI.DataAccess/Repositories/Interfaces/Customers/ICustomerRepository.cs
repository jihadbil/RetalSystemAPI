using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات العملاء والزبائن (Customers) وإدارة هواتفهم وحركات حساباتهم كوحدة متكاملة.
/// </summary>
public partial interface ICustomerRepository : IRepository<Customer>
{
    // ── هواتف العميل (Customer Phones) ──────────────────────────

    /// <summary>
    /// إضافة رقم هاتف جديد للعميل.
    /// </summary>
    /// <param name="phone">كيان هاتف العميل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddPhoneAsync(CustomerPhone phone, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة أرقام هواتف للعميل دفعة واحدة.
    /// </summary>
    /// <param name="phones">قائمة هواتف العميل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddPhonesAsync(IEnumerable<CustomerPhone> phones, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات رقم هاتف عميل في سياق قاعدة البيانات.
    /// </summary>
    /// <param name="phone">كيان الهاتف المحدث.</param>
    void UpdatePhone(CustomerPhone phone);

    /// <summary>
    /// إزالة رقم هاتف من السياق المحلي وقاعدة البيانات.
    /// </summary>
    /// <param name="phone">كيان الهاتف المراد حذفه.</param>
    void RemovePhone(CustomerPhone phone);

    /// <summary>
    /// تطبيق الحذف المنطقي على رقم هاتف العميل بتعيين IsDeleted = true.
    /// </summary>
    /// <param name="phone">كيان الهاتف المراد حذفه منطقياً.</param>
    void SoftDeletePhone(CustomerPhone phone);

    /// <summary>
    /// الحذف النهائي والفيزيائي لرقم هاتف العميل من قاعدة البيانات.
    /// </summary>
    /// <param name="phone">كيان الهاتف المراد حذفه نهائياً.</param>
    void HardDeletePhone(CustomerPhone phone);

    /// <summary>
    /// جلب هاتف عميل بواسطة معرفه الفريد ككيان متتبع للتعديل.
    /// </summary>
    /// <param name="phoneId">المعرف الفريد لرقم الهاتف.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان الهاتف أو null.</returns>
    Task<CustomerPhone?> GetPhoneByIdAsync(Guid phoneId, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة هواتف عميل محدد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="customerId">معرف العميل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أرقام هواتف العميل.</returns>
    Task<IReadOnlyList<CustomerPhone>> GetPhonesAsync(Guid customerId, CancellationToken ct = default);

    // ── حركات كشف حساب العميل (Customer Transactions) ───────────

    /// <summary>
    /// إضافة حركة مالية جديدة في كشف حساب العميل (فاتورة، سند قبض، مرتجع، إلخ).
    /// </summary>
    /// <param name="transaction">كيان حركة حساب العميل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddTransactionAsync(CustomerTransaction transaction, CancellationToken ct = default);

    /// <summary>
    /// جلب حركات كشف حساب عميل محدد مرتبة تنازلياً حسب تاريخ الحركة كقراءة فقط.
    /// </summary>
    /// <param name="customerId">معرف العميل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الحركات المالية للعميل.</returns>
    Task<IReadOnlyList<CustomerTransaction>> GetTransactionsAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// جلب حركات كشف حساب عميل خلال فترة زمنية محددة مع خيار فلترة الفرع مرتبة تصاعدياً للعرض الدفتري.
    /// </summary>
    /// <param name="customerId">معرف العميل.</param>
    /// <param name="fromDate">تاريخ بداية الفترة (اختياري).</param>
    /// <param name="toDate">تاريخ نهاية الفترة (اختياري).</param>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري).</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة حركات كشف الحساب المتضمنة تفاصيل الفرع.</returns>
    Task<IReadOnlyList<CustomerTransaction>> GetTransactionsByDateRangeAsync(Guid customerId, DateTime? fromDate, DateTime? toDate, Guid? branchId = null, CancellationToken ct = default);

    /// <summary>
    /// حساب رصيد العميل التراكمي في اللحظة السابقة لتاريخ محدد (الرصيد الافتتاحي لكشف الحساب للفترة).
    /// </summary>
    /// <param name="customerId">معرف العميل.</param>
    /// <param name="fromDate">التاريخ المستهدف لبدء الفترة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>الرصيد السابق للتاريخ شاملاً الرصيد الافتتاحي وفارق الحركات السابقة.</returns>
    Task<decimal> GetBalanceBeforeDateAsync(Guid customerId, DateTime fromDate, CancellationToken ct = default);
}
