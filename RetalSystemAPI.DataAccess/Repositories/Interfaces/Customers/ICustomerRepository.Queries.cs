using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات وتلخيصات أرصدة العملاء والزبائن.
/// </summary>
public partial interface ICustomerRepository
{
    /// <summary>
    /// فحص وجود إشارات تاريخية أو مالية للعميل تمنع حذفه.
    /// </summary>
    /// <param name="id">معرف العميل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان للعميل تاريخ مالي، وإلا False.</returns>
    Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل العميل وهوادفه التابعة كقراءة فقط دون تتبع عبر مواصفة CustomerWithDetailsSpec.
    /// </summary>
    /// <param name="id">معرف العميل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات العميل أو null.</returns>
    Task<Customer?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة العملاء المطابقة للشروط والفلاتر مع الهواتف دون تتبع.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة العملاء.</returns>
    Task<IReadOnlyList<Customer>> ListAsync(CustomerQuery request, CancellationToken ct = default);

    /// <summary>
    /// احتساب الملخص المالي الإجمالي لأرصدة العملاء (إجمالي الديون الدائنة والمدينة وعدد العملاء المدينين).
    /// </summary>
    /// <param name="request">شروط الفلترة المطبقة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>ملخص الأرصدة BalanceSummary.</returns>
    Task<BalanceSummary> GetBalanceSummaryAsync(CustomerQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في العملاء بنظام الصفحات مع فلاتر البحث والديون.
    /// </summary>
    /// <param name="request">معايير البحث والتصفح.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>صفحة نتائج العملاء PageResult.</returns>
    Task<PageResult<Customer>> SearchAsync(CustomerQuery request, CancellationToken ct = default);

    /// <summary>
    /// فحص وجود عميل مسجل بنفس الكود ضمن نطاق المستأجر الحالي.
    /// </summary>
    /// <param name="code">كود العميل المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الكود مستخدماً، وإلا False.</returns>
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// فحص وجود عميل بنفس الكود مع استثناء معرف العميل الحالي عند التعديل.
    /// </summary>
    /// <param name="code">الكود المحدث.</param>
    /// <param name="id">معرف العميل المستثنى.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد عميل آخر بنفس الكود، وإلا False.</returns>
    Task<bool> CodeExistsAsync(string code, Guid id, CancellationToken ct = default);

    /// <summary>
    /// فحص وجود تاريخ مالي مسجل للعميل (فواتير بيع، مرتجعات مبيعات، أو حركات كشف حساب) حتى المحذوفة منطقياً.
    /// </summary>
    /// <param name="id">معرف العميل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان هناك تاريخ مالي، وإلا False.</returns>
    Task<bool> HasFinancialHistoryAsync(Guid id, CancellationToken ct = default);
}
