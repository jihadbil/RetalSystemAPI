using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات تسويات الجرد المخزني والتحقق من ارتباط الأصناف والباركودات.
/// </summary>
public partial interface IStockAdjustmentRepository
{
    /// <summary>
    /// التحقق من استخدام أي من الباركودات المعطاة في بنود تسويات جرد سابقة.
    /// </summary>
    /// <param name="barcodeIds">قائمة معرفات الباركودات المراد فحصها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجد ارتباط، وإلا False.</returns>
    Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> barcodeIds, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود تسويات جرد سابقة للمنتج المحدد.
    /// </summary>
    /// <param name="id">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا وجدت تسوية جرد للمنتج، وإلا False.</returns>
    Task<bool> HasProductReferencesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل تسوية الجرد وبنودها مع بيانات المستودع كقراءة فقط عبر مواصفة StockAdjustmentWithDetailsSpec.
    /// </summary>
    /// <param name="id">معرف تسوية الجرد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان تسوية الجرد المفصل أو null.</returns>
    Task<StockAdjustment?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// البحث عن تسوية جرد برقم السند الفريد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="adjustmentNumber">رقم التسوية المخزنية.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان تسوية الجرد أو null.</returns>
    Task<StockAdjustment?> GetByNumberAsync(string adjustmentNumber, CancellationToken ct = default);

    /// <summary>
    /// استرجاع قائمة تسويات الجرد المخزني وفق معايير التصفية المحددة.
    /// </summary>
    /// <param name="request">معايير فلترة التسويات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة تسويات الجرد المطابقة.</returns>
    Task<IReadOnlyList<StockAdjustment>> ListAsync(StockAdjustmentQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في تسويات الجرد المخزني مع تقسيم الصفحات والفرز.
    /// </summary>
    /// <param name="request">معايير البحث والاستعلام والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة المحددة من تسويات الجرد.</returns>
    Task<PageResult<StockAdjustment>> SearchAsync(StockAdjustmentQuery request, CancellationToken ct = default);

    /// <summary>
    /// التحقق من تكرار رقم تسوية الجرد المخزني للمستأجر.
    /// </summary>
    /// <param name="adjustmentNumber">رقم التسوية.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الرقم مستخدماً مسبقاً، وإلا False.</returns>
    Task<bool> NumberExistsAsync(string adjustmentNumber, CancellationToken ct = default);

    /// <summary>
    /// استرجاع تفاصيل تسوية الجرد وبنودها مع تتبع الكيانات لإجراء التعديل أو الترحيل.
    /// </summary>
    /// <param name="id">معرف تسوية الجرد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان تسوية الجرد المتتبع أو null.</returns>
    Task<StockAdjustment?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);
}
