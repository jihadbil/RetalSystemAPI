using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات مرتجعات المشتريات والتحقق من التبعيات واحتساب الكميات المرتجعة.
/// </summary>
public partial interface IPurchaseReturnRepository
{
    /// <summary>
    /// التحقق من استخدام أي من الباركودات المعطاة في بنود مرتجعات مشتريات سابقة.
    /// </summary>
    /// <param name="removedBarcodeIds">معرفات الباركودات المراد فحصها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد ارتباط، وإلا False.</returns>
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود مرتجعات مشتريات سابقة للمنتج المحدد.
    /// </summary>
    /// <param name="id">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجد ارتباط، وإلا False.</returns>
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود مرتجع مشتريات صادر لفاتورة مشتريات محددة.
    /// </summary>
    /// <param name="id">معرف فاتورة الشراء الأصلية.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد ارتباط، وإلا False.</returns>
    Task<bool> HasInvoiceReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل مرتجع المشتريات وبنوده كقراءة فقط عبر مواصفة PurchaseReturnWithDetailsSpec.
    /// </summary>
    /// <param name="id">معرف المرتجع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات المرتجع المفصلة أو null.</returns>
    Task<PurchaseReturn?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// البحث عن مرتجع مشتريات برقم الإشعار الفريد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="returnNumber">رقم المرتجع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات المرتجع أو null.</returns>
    Task<PurchaseReturn?> GetByNumberAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة مرتجعات المشتريات المطابقة للفلاتر المحددة.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة مرتجعات المشتريات.</returns>
    Task<IReadOnlyList<PurchaseReturn>> ListAsync(PurchaseReturnQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في مرتجعات المشتريات وتصفحها بنظام الصفحات.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>صفحة نتائج المرتجعات PageResult.</returns>
    Task<PageResult<PurchaseReturn>> SearchAsync(PurchaseReturnQuery request, CancellationToken ct = default);

    /// <summary>
    /// فحص تكرار رقم إشعار مرتجع المشتريات للمستأجر الحالي.
    /// </summary>
    /// <param name="returnNumber">رقم المرتجع المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الرقم مستخدماً، وإلا False.</returns>
    Task<bool> NumberExistsAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب البنود المرتجعة سابقاً لفاتورة شراء وباركود معين لاحتساب الكمية المتبقية القابلة للإرجاع.
    /// </summary>
    /// <param name="originalInvoiceId">معرف فاتورة الشراء الأصلية.</param>
    /// <param name="barcodeId">معرف الباركود المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بنود المرتجعات السابقة.</returns>
    Task<IReadOnlyList<PurchaseReturnItem>> GetReturnedItemsByInvoiceAndBarcodeAsync(Guid originalInvoiceId, Guid barcodeId, CancellationToken ct = default);

    /// <summary>
    /// جلب البنود المرتجعة سابقاً لفاتورة شراء ومجموعة باركودات محددة.
    /// </summary>
    /// <param name="originalInvoiceId">معرف فاتورة الشراء الأصلية.</param>
    /// <param name="returnBarcodeIds">معرفات الباركودات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بنود المرتجعات السابقة.</returns>
    Task<IReadOnlyList<PurchaseReturnItem>> GetReturnedItemsByInvoiceAndBarcodesAsync(Guid originalInvoiceId, IEnumerable<Guid> returnBarcodeIds, CancellationToken ct = default);

    /// <summary>
    /// استرجاع مرتجع المشتريات وبنوده ككيانات متتبعة للتعديل ضمن وحدة العمل.
    /// </summary>
    /// <param name="id">معرف المرتجع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>المرتجع متتبعاً أو null.</returns>
    Task<PurchaseReturn?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
