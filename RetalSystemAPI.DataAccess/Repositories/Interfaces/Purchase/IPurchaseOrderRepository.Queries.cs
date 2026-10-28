using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات أوامر وطلبات الشراء والتحقق من التبعيات.
/// </summary>
public partial interface IPurchaseOrderRepository
{
    /// <summary>
    /// التحقق من استخدام أي من الباركودات المعطاة في بنود أوامر شراء سابقة.
    /// </summary>
    /// <param name="removedBarcodeIds">معرفات الباركودات المراد فحصها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد ارتباط، وإلا False.</returns>
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل أمر الشراء وبنوده والمورد والفرع والمستودع كقراءة فقط عبر مواصفة PurchaseOrderWithDetailsSpec.
    /// </summary>
    /// <param name="id">معرف أمر الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات أمر الشراء المفصلة أو null.</returns>
    Task<PurchaseOrder?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة أوامر الشراء المطابقة لمعايير وفلاتر الاستعلام كقراءة فقط.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أوامر الشراء.</returns>
    Task<IReadOnlyList<PurchaseOrder>> ListAsync(PurchaseOrderQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في أوامر الشراء وتصفحها بنظام الصفحات.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>صفحة نتائج أوامر الشراء PageResult.</returns>
    Task<PageResult<PurchaseOrder>> SearchAsync(PurchaseOrderQuery request, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود أمر شراء يحمل نفس الرقم للمستأجر الحالي.
    /// </summary>
    /// <param name="orderNumber">رقم أمر الشراء المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الرقم مستخدماً، وإلا False.</returns>
    Task<bool> NumberExistsAsync(string orderNumber, CancellationToken ct = default);

    /// <summary>
    /// استرجاع أمر الشراء مع بنوده ككيانات متتبعة للتعديل ضمن وحدة العمل.
    /// </summary>
    /// <param name="id">معرف أمر الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>أمر الشراء متتبعاً أو null.</returns>
    Task<PurchaseOrder?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
