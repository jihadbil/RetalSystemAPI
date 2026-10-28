using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات فواتير المشتريات وإسقاطات القراءة السريعة والتحقق من التبعيات.
/// </summary>
public partial interface IPurchaseInvoiceRepository
{
    /// <summary>
    /// التحقق من استخدام أي من الباركودات المعطاة في بنود أو تفاصيل فواتير المشتريات السابقة.
    /// </summary>
    /// <param name="removedBarcodeIds">معرفات الباركودات المراد فحصها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد ارتباط، وإلا False.</returns>
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود مشتريات سابقة للمنتج المحدد.
    /// </summary>
    /// <param name="id">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجد ارتباط، وإلا False.</returns>
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// البحث في فواتير المشتريات وتصفحها بنظام الصفحات والفرز.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>صفحة نتائج فواتير المشتريات PageResult.</returns>
    Task<PageResult<PurchaseInvoice>> SearchAsync(PurchaseInvoiceQuery request, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود رقم فاتورة مشتريات مسجل مسبقاً للمستأجر الحالي.
    /// </summary>
    /// <param name="invoiceNumber">رقم الفاتورة المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الرقم مستخدماً، وإلا False.</returns>
    Task<bool> NumberExistsAsync(string invoiceNumber, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل فاتورة المشتريات وبنودها وتفاصيل استلامها كقراءة فقط عبر مواصفة PurchaseInvoiceWithDetailsSpec.
    /// </summary>
    /// <param name="id">معرف الفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات الفاتورة المفصلة أو null.</returns>
    Task<PurchaseInvoice?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل استلام النكهات المرتبطة ببنود قديمة لتحديثها أو حذفها ككيانات متتبعة.
    /// </summary>
    /// <param name="oldItemIds">معرفات بنود الفاتورة القديمة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة تفاصيل الاستلام المتتبعة.</returns>
    Task<IReadOnlyList<PurchaseInvoiceItemBreakdown>> GetBreakdownsByItemIdsForUpdateAsync(IEnumerable<Guid> oldItemIds, CancellationToken ct = default);

    /// <summary>
    /// جلب بنود الفاتورة القديمة ككيانات متتبعة للتعديل أو الحذف.
    /// </summary>
    /// <param name="oldItemIds">معرفات البنود.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة بنود الفاتورة المتتبعة.</returns>
    Task<IReadOnlyList<PurchaseInvoiceItem>> GetItemsByIdsForUpdateAsync(IEnumerable<Guid> oldItemIds, CancellationToken ct = default);

    /// <summary>
    /// استرجاع فاتورة المشتريات وبنودها ككيانات متتبعة للتعديل ضمن وحدة العمل.
    /// </summary>
    /// <param name="id">معرف الفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>الفاتورة متتبعة أو null.</returns>
    Task<PurchaseInvoice?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع نتيجة قراءة فاتورة المشتريات المسقطة مباشرة PurchaseInvoiceReadResult للأداء العالي وتوفير الذاكرة.
    /// </summary>
    /// <param name="id">معرف الفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كائن نتيجة القراءة أو null.</returns>
    Task<PurchaseInvoiceReadResult?> GetReadResultAsync(Guid id, CancellationToken ct = default);
}
