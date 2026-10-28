using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات واستكشافات الفروع المتقدمة (Branch Repository Query Operations).
/// </summary>
public partial interface IBranchRepository
{
    /// <summary>
    /// فحص ما إذا كان الفرع مرتبطاً بمستندات أو حركات تاريخية تمنع حذفه.
    /// </summary>
    /// <param name="id">معرف الفرع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجدت إشارات تاريخية، وإلا False.</returns>
    Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// التحقق من أن الفرع نشط ومتاح للعمل وينتمي للمستأجر المحدد.
    /// </summary>
    /// <param name="branchId">معرف الفرع.</param>
    /// <param name="currentTenantId">معرف المستأجر الحالي.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الفرع نشطاً وينتمي للمستأجر، وإلا False.</returns>
    Task<bool> IsActiveForTenantAsync(Guid branchId, Guid currentTenantId, CancellationToken ct = default);

    /// <summary>
    /// استرجاع بيانات الفرع المفصلة شاملة أرقام الهواتف التابعة له كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="id">معرف الفرع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات الفرع مع هواتفه أو null.</returns>
    Task<Branch?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة الفروع النشطة مطابقة للشروط المحددة مع هواتفها.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الفروع المسترجعة.</returns>
    Task<IReadOnlyList<Branch>> ListAsync(BranchQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في الفروع بنظام الصفحات (Pagination) والترتيب.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة متضمنة إجمالي السجلات والبيانات.</returns>
    Task<PageResult<Branch>> SearchAsync(BranchQuery request, CancellationToken ct = default);

    /// <summary>
    /// فحص تكرار اسم الفرع للمستأجر الحالي قبل الإنشاء.
    /// </summary>
    /// <param name="name">اسم الفرع المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الاسم مستخدماً، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// فحص تكرار اسم الفرع مع استثناء معرف فرع محدد (عند التحديث).
    /// </summary>
    /// <param name="name">اسم الفرع المحدث.</param>
    /// <param name="id">معرف الفرع المستثنى من الفحص.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الاسم محجوزاً لفرع آخر، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل الفرع مع هواتفه ومستخدميه لفحص شروط الحذف.
    /// </summary>
    /// <param name="id">معرف الفرع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات الفرع مع الارتباطات أو null.</returns>
    Task<Branch?> GetDetailsForDeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// فحص ما إذا كان للفرع سجلات تشغيلية (مستودعات، خزائن، أوامر، فواتير، جلسات كاشير).
    /// </summary>
    /// <param name="id">معرف الفرع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان هناك تاريخ تشغيلي، وإلا False.</returns>
    Task<bool> HasOperationalHistoryAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تحميل الفرع مع هواتفه ككيان متتبع بالكامل لتعديل بياناته وهواتفه ذرية واحدة.
    /// </summary>
    /// <param name="id">معرف الفرع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>الفرع متتبعاً مع هواتفه أو null.</returns>
    Task<Branch?> GetWithPhonesForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// فحص وجود مستخدمين مرتبطين بهذا الفرع في نظام الهوية.
    /// </summary>
    /// <param name="id">معرف الفرع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد مستخدمون مرتبطون، وإلا False.</returns>
    Task<bool> HasUsersAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// فحص شامل لتبعيات الفرع التشغيلية والمستندية حتى المحذوفة منطقياً لمنع كسر التكامل المرجعي.
    /// </summary>
    /// <param name="id">معرف الفرع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجدت أي تبعيات، وإلا False.</returns>
    Task<bool> HasDependenciesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// فحص وجود أي من أرقام الهواتف المعطاة مسجلة مسبقاً لدى فروع أخرى للمستأجر.
    /// </summary>
    /// <param name="numbers">قائمة أرقام الهواتف المراد فحصها.</param>
    /// <param name="excludedBranchId">معرف الفرع المستثنى من الفحص (اختياري).</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وُجد أي رقم هاتف مسجل مسبقاً، وإلا False.</returns>
    Task<bool> PhonesExistAsync(IReadOnlyCollection<string> numbers, Guid? excludedBranchId = null, CancellationToken ct = default);
}
