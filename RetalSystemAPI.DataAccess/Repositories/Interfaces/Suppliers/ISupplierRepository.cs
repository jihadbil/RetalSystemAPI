using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات الموردين (Suppliers) وإدارة هواتفهم وحركات حساباتهم كوحدة متكاملة.
/// </summary>
public partial interface ISupplierRepository : IRepository<Supplier>
{
    // ── هواتف المورد (Supplier Phones) ──────────────────────────

    /// <summary>
    /// إضافة رقم هاتف جديد للمورد.
    /// </summary>
    /// <param name="phone">كيان رقم هاتف المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddPhoneAsync(SupplierPhone phone, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة أرقام هواتف للمورد دفعة واحدة.
    /// </summary>
    /// <param name="phones">قائمة هواتف المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddPhonesAsync(IEnumerable<SupplierPhone> phones, CancellationToken ct = default);

    /// <summary>
    /// تحديث رقم هاتف مورد في سياق قاعدة البيانات.
    /// </summary>
    /// <param name="phone">كيان الهاتف المحدث.</param>
    void UpdatePhone(SupplierPhone phone);

    /// <summary>
    /// إزالة رقم هاتف من السياق المحلي وقاعدة البيانات.
    /// </summary>
    /// <param name="phone">كيان الهاتف المراد حذفه.</param>
    void RemovePhone(SupplierPhone phone);

    /// <summary>
    /// تطبيق الحذف المنطقي على رقم هاتف المورد بتعيين IsDeleted = true.
    /// </summary>
    /// <param name="phone">كيان الهاتف المراد حذفه منطقياً.</param>
    void SoftDeletePhone(SupplierPhone phone);

    /// <summary>
    /// حذف نهائي وفيزيائي لرقم هاتف المورد من قاعدة البيانات.
    /// </summary>
    /// <param name="phone">كيان الهاتف المراد حذفه نهائياً.</param>
    void HardDeletePhone(SupplierPhone phone);

    /// <summary>
    /// جلب هاتف مورد بواسطة معرفه ككيان متتبع للتعديل.
    /// </summary>
    /// <param name="phoneId">المعرف الفريد لرقم الهاتف.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان الهاتف أو null.</returns>
    Task<SupplierPhone?> GetPhoneByIdAsync(Guid phoneId, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة أرقام هواتف مورد محدد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="supplierId">معرف المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أرقام هواتف المورد.</returns>
    Task<IReadOnlyList<SupplierPhone>> GetPhonesAsync(Guid supplierId, CancellationToken ct = default);

    // ── حركات كشف حساب المورد (Supplier Transactions) ───────────

    /// <summary>
    /// إضافة حركة مالية جديدة في كشف حساب المورد (فاتورة شراء، سند صرف، مرتجع، تسوية).
    /// </summary>
    /// <param name="transaction">كيان حركة حساب المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddTransactionAsync(SupplierTransaction transaction, CancellationToken ct = default);

    /// <summary>
    /// جلب حركات كشف حساب مورد محدد مرتبة تنازلياً حسب تاريخ الحركة كقراءة فقط.
    /// </summary>
    /// <param name="supplierId">معرف المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الحركات المالية للمورد.</returns>
    Task<IReadOnlyList<SupplierTransaction>> GetTransactionsAsync(Guid supplierId, CancellationToken ct = default);

    /// <summary>
    /// جلب حركات كشف حساب مورد خلال فترة زمنية محددة مع خيار فلترة الفرع مرتبة تصاعدياً للعرض الدفتري.
    /// </summary>
    /// <param name="supplierId">معرف المورد.</param>
    /// <param name="fromDate">تاريخ بداية الفترة (اختياري).</param>
    /// <param name="toDate">تاريخ نهاية الفترة (اختياري).</param>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري).</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة حركات كشف الحساب المتضمنة تفاصيل الفرع.</returns>
    Task<IReadOnlyList<SupplierTransaction>> GetTransactionsByDateRangeAsync(Guid supplierId, DateTime? fromDate, DateTime? toDate, Guid? branchId = null, CancellationToken ct = default);

    /// <summary>
    /// حساب رصيد المورد التراكمي في اللحظة السابقة لتاريخ محدد (الرصيد الافتتاحي لكشف الحساب للفترة).
    /// </summary>
    /// <param name="supplierId">معرف المورد.</param>
    /// <param name="fromDate">التاريخ المستهدف لبدء الفترة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>الرصيد السابق للتاريخ شاملاً الرصيد الافتتاحي وفارق الحركات السابقة.</returns>
    Task<decimal> GetBalanceBeforeDateAsync(Guid supplierId, DateTime fromDate, CancellationToken ct = default);
}
