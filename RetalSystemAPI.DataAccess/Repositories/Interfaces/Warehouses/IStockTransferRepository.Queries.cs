using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات أوامر التحويل المخزني وفحص مراجع الأصناف والباركودات.
/// </summary>
public partial interface IStockTransferRepository
{
    /// <summary>
    /// التحقق من استخدام أي من الباركودات المعطاة في بنود أوامر تحويل سابقة.
    /// </summary>
    /// <param name="barcodeIds">قائمة معرفات الباركودات المراد فحصها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجد ارتباط ببنود تحويل، وإلا False.</returns>
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> barcodeIds, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود أوامر تحويل مخزني سابقة للمنتج المحدد.
    /// </summary>
    /// <param name="id">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجد تحويل للمنتج، وإلا False.</returns>
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل أمر التحويل وبنوده مع بيانات المستودعين كقراءة فقط عبر مواصفة StockTransferWithDetailsSpec.
    /// </summary>
    /// <param name="id">معرف أمر التحويل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان أمر التحويل المفصل أو null.</returns>
    Task<StockTransfer?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// البحث عن أمر تحويل برقم السند الفريد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="transferNumber">رقم التحويل المخزني.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان أمر التحويل أو null.</returns>
    Task<StockTransfer?> GetByNumberAsync(string transferNumber, CancellationToken ct = default);

    /// <summary>
    /// استرجاع قائمة أوامر التحويل المخزني وفق معايير التصفية المحددة.
    /// </summary>
    /// <param name="request">معايير فلترة التحويلات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أوامر التحويل المطابقة.</returns>
    Task<IReadOnlyList<StockTransfer>> ListAsync(StockTransferQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في أوامر التحويل المخزني مع تقسيم الصفحات والفرز.
    /// </summary>
    /// <param name="request">معايير البحث والاستعلام والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة المحددة من أوامر التحويل.</returns>
    Task<PageResult<StockTransfer>> SearchAsync(StockTransferQuery request, CancellationToken ct = default);

    /// <summary>
    /// التحقق من تكرار رقم أمر التحويل المخزني للمستأجر.
    /// </summary>
    /// <param name="transferNumber">رقم التحويل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الرقم مستخدماً مسبقاً، وإلا False.</returns>
    Task<bool> NumberExistsAsync(string transferNumber, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل أمر التحويل وبنوده مع تتبع الكيانات لإجراء التعديل أو الترحيل.
    /// </summary>
    /// <param name="id">معرف أمر التحويل.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان أمر التحويل المتتبع أو null.</returns>
    Task<StockTransfer?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
