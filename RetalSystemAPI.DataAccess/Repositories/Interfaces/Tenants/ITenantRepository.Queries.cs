using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات المستأجرين والتحقق من الأسماء والبحث بالقوائم.
/// </summary>
public partial interface ITenantRepository
{
    /// <summary>
    /// جلب قائمة مختصرة لجميع المستأجرين لاستخدامها في القوائم المنسدلة (Lookup List) كقراءة فقط.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة المستأجرين مرتبة بالمعرف.</returns>
    Task<IReadOnlyList<Tenant>> ListLookupAsync(CancellationToken ct = default);

    /// <summary>
    /// فحص وجود مستأجر مسجل بنفس الاسم في النظام.
    /// </summary>
    /// <param name="name">اسم المستأجر المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الاسم محجوزاً، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// فحص وجود مستأجر بنفس الاسم مع استثناء معرف مستأجر معين (عند التعديل).
    /// </summary>
    /// <param name="name">الاسم المحدث.</param>
    /// <param name="id">معرف المستأجر المستثنى.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن كان الاسم مكرراً لمستأجر آخر، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);

    /// <summary>
    /// البحث في المستأجرين وتصفحهم بنظام الصفحات والفرز حسب تاريخ الإنشاء.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتائج الصفحة للمستأجرين.</returns>
    Task<PageResult<Tenant>> SearchAsync(TenantQuery request, CancellationToken ct = default);
}
