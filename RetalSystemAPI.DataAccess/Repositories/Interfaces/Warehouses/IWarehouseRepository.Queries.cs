using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات المستودعات والتحقق من التبعيات والمستندات المرتبطة.
/// </summary>
public partial interface IWarehouseRepository
{
    /// <summary>
    /// استرجاع قائمة المستودعات بحسب نوعها (مستودع رئيسي، معرض/صالة، مخزن فرعي).
    /// </summary>
    /// <param name="type">نوع المستودع المطلوب تصفيته.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة المستودعات المطابقة للنوع.</returns>
    Task<IReadOnlyList<Warehouse>> GetByTypeAsync(WarehouseType type, CancellationToken ct = default);

    /// <summary>
    /// استرجاع قائمة مبسطة بالمستودعات لاستخدامها في القوائم المنسدلة للواجهة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة المستودعات المتاحة للمستأجر.</returns>
    Task<IReadOnlyList<Warehouse>> ListLookupAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل المستودع والفرع المرتبط به كقراءة فقط عبر مواصفة WarehouseWithDetailsSpec.
    /// </summary>
    /// <param name="id">معرف المستودع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات المستودع المفصلة أو null في حال عدم العثور عليه.</returns>
    Task<Warehouse?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع قائمة المستودعات وفق معايير الفلترة المحددة.
    /// </summary>
    /// <param name="request">معايير البحث والاستعلام.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة المستودعات المطابقة.</returns>
    Task<IReadOnlyList<Warehouse>> ListAsync(WarehouseQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في المستودعات مع دعم تقسيم الصفحات والفرز.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة المحددة من بيانات المستودعات.</returns>
    Task<PageResult<Warehouse>> SearchAsync(WarehouseQuery request, CancellationToken ct = default);

    /// <summary>
    /// فحص ما إذا كان المستودع مرتبطاً بأي مستندات أو حركات تاريخية تمنع حذفه (فواتير، تسويات، تحويلات).
    /// </summary>
    /// <param name="id">معرف المستودع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجد أي مستند تاريخي مرتبط بالمستودع، وإلا False.</returns>
    Task<bool> HasDocumentReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// التحقق من تكرار اسم المستودع داخل نفس الفرع.
    /// </summary>
    /// <param name="name">اسم المستودع.</param>
    /// <param name="branchId">معرف الفرع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الاسم مستخدماً مسبقاً في الفرع، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, Guid branchId, CancellationToken ct = default);

    /// <summary>
    /// التحقق من تكرار اسم المستودع داخل نفس الفرع باستثناء المستودع الحالي قيد التعديل.
    /// </summary>
    /// <param name="name">اسم المستودع.</param>
    /// <param name="branchId">معرف الفرع.</param>
    /// <param name="id">معرف المستودع الحالي المستثنى من الفحص.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجد مستودع آخر يحمل نفس الاسم في الفرع، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, Guid branchId, Guid id, CancellationToken ct = default);
}
