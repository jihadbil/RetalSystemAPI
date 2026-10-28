using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات فواتير المبيعات والتحقق من التبعيات وتفرد أرقام الفواتير.
/// </summary>
public partial interface ISalesInvoiceRepository
{
    /// <summary>
    /// التحقق من استخدام أي من الباركودات المعطاة في بنود فواتير مبيعات سابقة لمنع حذفها.
    /// </summary>
    /// <param name="removedBarcodeIds">قائمة معرفات الباركودات المراد فحصها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجدت أي إشارة لباركود، وإلا False.</returns>
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);

    /// <summary>
    /// التحقق من استخدام المنتج المحدد في بنود فواتير مبيعات سابقة لمنع حذفه.
    /// </summary>
    /// <param name="id">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان للمنتج مبيعات سابقة، وإلا False.</returns>
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل فاتورة المبيعات الشاملة وبنودها كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="id">معرف الفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات الفاتورة المفصلة أو null.</returns>
    Task<SalesInvoice?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// البحث عن فاتورة مبيعات بالرقم الفريد للفاتورة كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="invoiceNumber">رقم الفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات الفاتورة أو null.</returns>
    Task<SalesInvoice?> GetByNumberAsync(string invoiceNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة فواتير المبيعات المطابقة للفلاتر والمعايير المحددة.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة فواتير المبيعات.</returns>
    Task<IReadOnlyList<SalesInvoice>> ListAsync(SalesInvoiceQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في فواتير المبيعات وتصفحها بنظام الصفحات.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>صفحة نتائج فواتير المبيعات PageResult.</returns>
    Task<PageResult<SalesInvoice>> SearchAsync(SalesInvoiceQuery request, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود رقم فاتورة مبيعات مسجل مسبقاً للمستأجر الحالي.
    /// </summary>
    /// <param name="invoiceNumber">رقم الفاتورة المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان رقم الفاتورة مستخدماً، وإلا False.</returns>
    Task<bool> NumberExistsAsync(string invoiceNumber, CancellationToken ct = default);

    /// <summary>
    /// استرجاع الفاتورة مع بنودها ككيانات متتبعة بالكامل في الذاكرة لتعديلها.
    /// </summary>
    /// <param name="id">معرف الفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>الفاتورة متتبعة مع بنودها أو null.</returns>
    Task<SalesInvoice?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
