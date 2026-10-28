using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وبحوثات أرصدة المستودعات الرئيسية (Storage) وصالات العرض (Showroom) وتصدير وتتبع المخزون.
/// </summary>
public partial interface IStockRepository
{
    /// <summary>
    /// احتساب إجمالي الكميات المتوفرة لمجموعة من المنتجات عبر كافة الصالات والمستودعات.
    /// </summary>
    /// <param name="productIds">معرفات المنتجات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قاموس يربط معرف المنتج بإجمالي كميته المتاحة في النظام.</returns>
    Task<IReadOnlyDictionary<Guid, long>> GetQuantitiesByProductAsync(IEnumerable<Guid> productIds, CancellationToken ct = default);

    /// <summary>
    /// استرجاع سجلات أرصدة صالات العرض لغرض التصدير.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أسطر تصدير أرصدة الصالات.</returns>
    Task<IReadOnlyList<StockShowroomExportRow>> GetShowroomExportRowsAsync(CancellationToken ct = default);

    /// <summary>
    /// استرجاع سجلات أرصدة المستودعات الرئيسية لغرض التصدير.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أسطر تصدير أرصدة المستودعات.</returns>
    Task<IReadOnlyList<StockStorageExportRow>> GetStorageExportRowsAsync(CancellationToken ct = default);

    /// <summary>
    /// فحص وجود كميات غير صفرية في المستودعات الرئيسية لمجموعة باركودات محددة لمنع حذفها.
    /// </summary>
    /// <param name="removedBarcodeIds">معرفات الباركودات المراد فحصها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان هناك أي رصيد غير صفري، وإلا False.</returns>
    Task<bool> HasNonzeroStorageByBarcodesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);

    /// <summary>
    /// استرجاع سجلات أرصدة المستودعات الرئيسية مع التتبع لباركودات محددة لتعديلها أو تصفيرها.
    /// </summary>
    /// <param name="removedBarcodeIds">معرفات الباركودات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة كيانات الأرصدة المتتبعة.</returns>
    Task<IReadOnlyList<StorgeStock>> GetStorageByBarcodesForUpdateAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود رصيد غير صفري في صالات العرض للمنتج المحدد لمنع حذفه.
    /// </summary>
    /// <param name="id">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان رصيد الصالة غير صفري، وإلا False.</returns>
    Task<bool> HasNonzeroShowroomByProductAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب أرصدة صالة العرض لمنتج محدد مع التتبع لإجراء التعديلات المخزنية.
    /// </summary>
    /// <param name="id">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أرصدة صالات العرض المتتبعة للمنتج.</returns>
    Task<IReadOnlyList<ShowroomStock>> GetShowroomByProductForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب أرصدة المستودع لمجموعة باركودات مع التتبع لتعديل الكميات بالدفعات.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع.</param>
    /// <param name="barcodeIdList">قائمة معرفات الباركودات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أرصدة المستودع المتتبعة.</returns>
    Task<IReadOnlyList<StorgeStock>> GetStorageForUpdateAsync(Guid warehouseId, IEnumerable<Guid> barcodeIdList, CancellationToken ct = default);

    /// <summary>
    /// جلب أرصدة الصالة لمجموعة منتجات مع التتبع لتعديل الكميات بالدفعات.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع (الصالة).</param>
    /// <param name="productIds">قائمة معرفات المنتجات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أرصدة الصالة المتتبعة.</returns>
    Task<IReadOnlyList<ShowroomStock>> GetShowroomForUpdateAsync(Guid warehouseId, IEnumerable<Guid> productIds, CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل رصيد باركود في مستودع مع بيانات المنتج والمستودع كقراءة فقط.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع.</param>
    /// <param name="productBarcodeId">معرف باركود المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان رصيد المستودع المفصل أو null.</returns>
    Task<StorgeStock?> GetStorageDetailsAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);

    /// <summary>
    /// جلب مفاتيح باركودات الأرصدة المخزنة في مستودع معين للمقارنة السريعة.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة مفاتيح المستودع والباركود.</returns>
    Task<IReadOnlyList<StockStorageKeysRow>> GetStorageKeysAsync(Guid warehouseId, CancellationToken ct = default);

    /// <summary>
    /// استرجاع قائمة أرصدة المستودع الرئيسي وفق استعلام التصفية.
    /// </summary>
    /// <param name="request">معايير فلترة أرصدة المستودع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أرصدة المستودع المطابقة مع التفاصيل.</returns>
    Task<IReadOnlyList<StorgeStock>> ListStorageAsync(StorageStockQuery request, CancellationToken ct = default);

    /// <summary>
    /// جلب رصيد باركود محدد في مستودع دون تتبع.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع.</param>
    /// <param name="productBarcodeId">معرف الباركود.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان رصيد المستودع أو null.</returns>
    Task<StorgeStock?> GetStorageAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);

    /// <summary>
    /// استرجاع الأصناف التي وصلت إلى حد الطلب أو أقل في المستودعات الرئيسية لتنبيه إعادة الشراء.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع الاختياري للفلترة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الأرصدة المنخفضة في المستودعات.</returns>
    Task<IReadOnlyList<StorgeStock>> GetLowStorageAsync(Guid? warehouseId, CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل رصيد صنف في صالة عرض مع بيانات المنتج والفرع كقراءة فقط.
    /// </summary>
    /// <param name="warehouseId">معرف الصالة/المستودع.</param>
    /// <param name="productId">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان رصيد الصالة المفصل أو null.</returns>
    Task<ShowroomStock?> GetShowroomDetailsAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);

    /// <summary>
    /// استرجاع مفاتيح المنتجات المتوفرة في صالة عرض معينة للتحقق السريع.
    /// </summary>
    /// <param name="warehouseId">معرف الصالة/المستودع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة مفاتيح الصالة والمنتج.</returns>
    Task<IReadOnlyList<StockShowroomKeysRow>> GetShowroomKeysAsync(Guid warehouseId, CancellationToken ct = default);

    /// <summary>
    /// استرجاع قائمة أرصدة صالات العرض وفق استعلام التصفية.
    /// </summary>
    /// <param name="request">معايير فلترة أرصدة الصالات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أرصدة الصالات المطابقة.</returns>
    Task<IReadOnlyList<ShowroomStock>> ListShowroomAsync(ShowroomStockQuery request, CancellationToken ct = default);

    /// <summary>
    /// جلب رصيد منتج في صالة عرض محددة دون تتبع.
    /// </summary>
    /// <param name="warehouseId">معرف الصالة.</param>
    /// <param name="productId">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان رصيد الصالة أو null.</returns>
    Task<ShowroomStock?> GetShowroomAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);

    /// <summary>
    /// استرجاع الأصناف التي وصلت إلى حد الطلب أو أقل في صالات العرض للتغذية من المستودع.
    /// </summary>
    /// <param name="warehouseId">معرف الصالة الاختياري للفلترة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الأرصدة المنخفضة في صالات العرض.</returns>
    Task<IReadOnlyList<ShowroomStock>> GetLowShowroomAsync(Guid? warehouseId, CancellationToken ct = default);

    /// <summary>
    /// التحقق مما إذا كان المستودع الرئيسي يحتوي على أي أرصدة موجبة تمنع حذفه.
    /// </summary>
    /// <param name="id">معرف المستودع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان هناك بضاعة متوفرة بالمستودع، وإلا False.</returns>
    Task<bool> HasPositiveStorageByWarehouseAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// التحقق مما إذا كانت صالة العرض تحتوي على أي أرصدة موجبة تمنع حذفها.
    /// </summary>
    /// <param name="id">معرف الصالة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان هناك بضاعة متوفرة بالصالة، وإلا False.</returns>
    Task<bool> HasPositiveShowroomByWarehouseAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// البحث في أرصدة المستودعات الرئيسية مع تقسيم الصفحات والفرز والبحث النصي.
    /// </summary>
    /// <param name="request">معايير البحث والاستعلام المتقدم.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة المحددة من أرصدة المستودع.</returns>
    Task<PageResult<StorgeStock>> SearchStorageAsync(StorageStockQuery request, CancellationToken ct = default);

    /// <summary>
    /// البحث في أرصدة صالات العرض مع تقسيم الصفحات والفرز والبحث النصي.
    /// </summary>
    /// <param name="request">معايير البحث والاستعلام المتقدم.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>نتيجة الصفحة المحددة من أرصدة الصالة.</returns>
    Task<PageResult<ShowroomStock>> SearchShowroomAsync(ShowroomStockQuery request, CancellationToken ct = default);
}
