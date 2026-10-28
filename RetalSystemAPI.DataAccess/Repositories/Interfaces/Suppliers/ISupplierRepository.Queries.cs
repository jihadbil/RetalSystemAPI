using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات وتلخيصات أرصدة الموردين وحركاتهم.
/// </summary>
public partial interface ISupplierRepository
{
    /// <summary>
    /// فحص وجود إشارات تاريخية أو مستندية للمورد تمنع حذفه.
    /// </summary>
    /// <param name="id">معرف المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان للمورد تاريخ مالي أو تشغيلي، وإلا False.</returns>
    Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل المورد وهوادفه التابعة كقراءة فقط دون تتبع عبر مواصفة SupplierWithDetailsSpec.
    /// </summary>
    /// <param name="id">معرف المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات المورد أو null.</returns>
    Task<Supplier?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة الموردين المطابقة للشروط والفلاتر مع الهواتف دون تتبع.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الموردين.</returns>
    Task<IReadOnlyList<Supplier>> ListAsync(SupplierQuery request, CancellationToken ct = default);

    /// <summary>
    /// احتساب الملخص المالي الإجمالي لأرصدة الموردين (إجمالي الديون الدائنة والمدينة وعدد الموردين المستحق لهم مبالغ).
    /// </summary>
    /// <param name="request">شروط الفلترة المطبقة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>ملخص الأرصدة BalanceSummary.</returns>
    Task<BalanceSummary> GetBalanceSummaryAsync(SupplierQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في الموردين بنظام الصفحات مع فلاتر البحث والديون.
    /// </summary>
    /// <param name="request">معايير البحث والتصفح.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>صفحة نتائج الموردين PageResult.</returns>
    Task<PageResult<Supplier>> SearchAsync(SupplierQuery request, CancellationToken ct = default);

    /// <summary>
    /// فحص وجود مورد مسجل بنفس الاسم للمستأجر الحالي.
    /// </summary>
    /// <param name="name">اسم المورد المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الاسم مستخدماً، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// فحص وجود مورد بنفس الاسم مع استثناء سجل المورد الحالي عند التعديل.
    /// </summary>
    /// <param name="name">الاسم المحدث.</param>
    /// <param name="id">معرف المورد المستثنى.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد مورد آخر بنفس الاسم، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);

    /// <summary>
    /// فحص وجود تاريخ مالي مسجل للمورد (فواتير شراء، أوامر، مرتجعات، أو حركات كشف حساب) حتى المحذوفة منطقياً.
    /// </summary>
    /// <param name="id">معرف المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان هناك تاريخ مالي، وإلا False.</returns>
    Task<bool> HasFinancialHistoryAsync(Guid id, CancellationToken ct = default);
}
