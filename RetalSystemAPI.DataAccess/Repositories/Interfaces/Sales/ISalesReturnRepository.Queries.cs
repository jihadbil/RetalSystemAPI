using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات مرتجعات المبيعات والتحقق من التبعيات وسقوف الكميات المرتجعة.
/// </summary>
public partial interface ISalesReturnRepository
{
    /// <summary>
    /// التحقق من استخدام أي من الباركودات في بنود مرتجعات مبيعات سابقة.
    /// </summary>
    /// <param name="removedBarcodeIds">معرفات الباركودات المراد فحصها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد ارتباط، وإلا False.</returns>
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود مرتجعات مبيعات سابقة للمنتج المحدد.
    /// </summary>
    /// <param name="id">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجد ارتباط، وإلا False.</returns>
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود أي مرتجع مبيعات مرتبط بفاتورة مبيعات محددة.
    /// </summary>
    /// <param name="id">معرف الفاتورة الأصلية.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد مرتجع صادر للفاتورة، وإلا False.</returns>
    Task<bool> HasInvoiceReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل مرتجع المبيعات وبنوده كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="id">معرف المرتجع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات المرتجع أو null.</returns>
    Task<SalesReturn?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// البحث عن مرتجع مبيعات برقم الإشعار الفريد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="returnNumber">رقم إشعار المرتجع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات المرتجع أو null.</returns>
    Task<SalesReturn?> GetByNumberAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة مرتجعات المبيعات المطابقة للفلاتر المحددة.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة مرتجعات المبيعات.</returns>
    Task<IReadOnlyList<SalesReturn>> ListAsync(SalesReturnQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في مرتجعات المبيعات وتصفحها بنظام الصفحات.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>صفحة نتائج المرتجعات PageResult.</returns>
    Task<PageResult<SalesReturn>> SearchAsync(SalesReturnQuery request, CancellationToken ct = default);

    /// <summary>
    /// فحص تكرار رقم إشعار مرتجع المبيعات للمستأجر الحالي.
    /// </summary>
    /// <param name="returnNumber">رقم المرتجع المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الرقم مستخدماً، وإلا False.</returns>
    Task<bool> NumberExistsAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة البنود التي تم إرجاعها مسبقاً لفاتورة أصلية ومنتجات محددة لاحتساب سقف الإرجاع المتبقي.
    /// </summary>
    /// <param name="originalInvoiceId">معرف الفاتورة الأصلية.</param>
    /// <param name="productIds">معرفات المنتجات المراد فحص مرتجعاتها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بنود المرتجعات السابقة.</returns>
    Task<IReadOnlyList<SalesReturnItem>> GetReturnedItemsByInvoiceAndProductsAsync(Guid originalInvoiceId, IEnumerable<Guid> productIds, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة فواتير المرتجع الصادرة على فاتورة مبيعات أصلية كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="originalInvoiceId">معرف الفاتورة الأصلية.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة مرتجعات الفاتورة.</returns>
    Task<IReadOnlyList<SalesReturn>> GetByInvoiceAsync(Guid originalInvoiceId, CancellationToken ct = default);

    /// <summary>
    /// استرجاع مرتجع المبيعات وبنوده ككيانات متتبعة للتعديل.
    /// </summary>
    /// <param name="id">معرف المرتجع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>المرتجع متتبعاً أو null.</returns>
    Task<SalesReturn?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
