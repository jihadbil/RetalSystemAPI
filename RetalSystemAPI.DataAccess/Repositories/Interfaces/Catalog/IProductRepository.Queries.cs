using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات المنتجات والباركودات والوحدات والصور، وعمليات التصدير والاستيراد الضخمة.
/// </summary>
public partial interface IProductRepository
{
    /// <summary>البحث في الباركودات بالمعايير والصفحات كقراءة فقط دون تتبع</summary>
    Task<IReadOnlyList<ProductBarCode>> SearchBarcodesAsync(BarcodeQuery request, CancellationToken ct = default);

    /// <summary>جلب باركودات المنتج مع تفاصيل الصنف وصوره ووحداته</summary>
    Task<IReadOnlyList<ProductBarCode>> GetBarcodesWithDetailsAsync(Guid productId, CancellationToken ct = default);

    /// <summary>فحص وجود باركود مطابق في قاعدة البيانات للمستأجر الحالي</summary>
    Task<bool> BarCodeExistsAsync(string barCode, CancellationToken ct = default);

    /// <summary>فحص وجود باركود مطابق مع استثناء سجل باركود محدد</summary>
    Task<bool> BarCodeExistsAsync(string barCode, Guid barCodeId, CancellationToken ct = default);

    /// <summary>جلب بيانات الأصناف بصيغة التصدير لملفات الإكسل كقراءة خفيفة</summary>
    Task<IReadOnlyList<ProductExportRow>> GetExportRowsAsync(CancellationToken ct = default);

    /// <summary>جلب باركودات الأصناف بصيغة التصدير لملفات الإكسل متعددة الشيتات</summary>
    Task<IReadOnlyList<ProductBarcodeExportRow>> GetBarcodeExportRowsAsync(CancellationToken ct = default);

    /// <summary>جلب وحدات الأصناف بصيغة التصدير لملفات الإكسل متعددة الشيتات</summary>
    Task<IReadOnlyList<ProductUnitExportRow>> GetUnitExportRowsAsync(CancellationToken ct = default);

    /// <summary>استرجاع قائمة مختصرة للمنتجات للقوائم المنسدلة (Lookup)</summary>
    Task<IReadOnlyList<ProductLookupRow>> GetLookupAsync(CancellationToken ct = default);

    /// <summary>استرجاع قائمة مختصرة لباركودات المنتجات للقوائم المنسدلة</summary>
    Task<IReadOnlyList<ProductBarcodeLookupRow>> GetBarcodeLookupAsync(CancellationToken ct = default);

    /// <summary>جلب الأصناف المرشحة للتحديث أثناء استيراد ملفات الإكسل بدفعة واحدة ككيانات متتبعة</summary>
    Task<IReadOnlyList<Product>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default);

    /// <summary>جلب كافة صور المنتج كقراءة فقط دون تتبع</summary>
    Task<IReadOnlyList<ProductImage>> GetImagesByProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>التحقق من أن معرف الباركود ينتمي بالفعل للمنتج المحدد</summary>
    Task<bool> BarcodeBelongsToProductAsync(Guid barcodeId, Guid productId, CancellationToken ct = default);

    /// <summary>فحص ما إذا كان للمنتج أي صور مسجلة</summary>
    Task<bool> HasImagesAsync(Guid productId, CancellationToken ct = default);

    /// <summary>جلب الصور الافتراضية للمنتج ككيانات متتبعة للتعديل (لإلغاء وضع الافتراضي عند تعيين صورة جديدة)</summary>
    Task<IReadOnlyList<ProductImage>> GetDefaultImagesForUpdateAsync(Guid productId, CancellationToken ct = default);

    /// <summary>جلب كافة صور المنتج ككيانات متتبعة للتعديل</summary>
    Task<IReadOnlyList<ProductImage>> GetImagesForUpdateAsync(Guid productId, CancellationToken ct = default);

    /// <summary>جلب قائمة الأصناف المطابقة للشروط والفلاتر مع كافة تفاصيلها</summary>
    Task<IReadOnlyList<Product>> ListAsync(ProductQuery request, CancellationToken ct = default);

    /// <summary>البحث السريع في الأصناف عبر نص البحث للقوائم المنسدلة وشاشات البيع</summary>
    Task<IReadOnlyList<Product>> SearchLookupAsync(string search, CancellationToken ct = default);

    /// <summary>استرجاع تفاصيل المنتج الكاملة (التصنيف، الوحدات، الباركودات، الصور) كقراءة فقط</summary>
    Task<Product?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>البحث في الأصناف بنظام الصفحات والفلاتر المتقدمة</summary>
    Task<PageResult<Product>> SearchAsync(ProductQuery request, CancellationToken ct = default);

    /// <summary>البحث عن باركود بالقيمة النصية المطابقة له كقراءة فقط</summary>
    Task<ProductBarCode?> GetBarcodeByValueAsync(string barCode, CancellationToken ct = default);

    /// <summary>استرجاع أول باركود مطابق لأي من القيم النصية المعطاة</summary>
    Task<ProductBarCode?> GetFirstMatchingBarcodeAsync(IEnumerable<string> barCodeStrings, CancellationToken ct = default);

    /// <summary>تحميل المنتج مع وحداته وباركوداته وصوره ككيانات متتبعة بالكامل للتعديل الذري</summary>
    Task<Product?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>فحص ما إذا كان أي من الباركودات المعطاة مستخدماً لدى صنف آخر غير الصنف الحالي</summary>
    Task<bool> BarcodesExistOutsideProductAsync(IEnumerable<string> barCodeStrings, Guid id, CancellationToken ct = default);

    /// <summary>جلب الصور المرتبطة بباركودات محددة ككيانات متتبعة لحذفها عند حذف الباركود</summary>
    Task<IReadOnlyList<ProductImage>> GetImagesByBarcodesForUpdateAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default);

    /// <summary>جلب باركودات المنتج كقراءة فقط بدون تتبع</summary>
    Task<IReadOnlyList<ProductBarCode>> GetBarcodesByProductAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب باركودات المنتج ككيانات متتبعة للتعديل</summary>
    Task<IReadOnlyList<ProductBarCode>> GetBarcodesForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب وحدات المنتج ككيانات متتبعة للتعديل</summary>
    Task<IReadOnlyList<ProductUnit>> GetUnitsForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب وحدات المنتج مع تفاصيل اسم الوحدة الأساسية كقراءة فقط</summary>
    Task<IReadOnlyList<ProductUnit>> GetUnitsWithDetailsAsync(Guid productId, CancellationToken ct = default);

    /// <summary>التحقق من ربط وحدة قياس معينة بالمنتج المحدد مسبقاً</summary>
    Task<bool> ProductUnitExistsAsync(Guid productId, Guid unitId, CancellationToken ct = default);

    /// <summary>فحص ما إذا كان للمنتج أي وحدات قياس مسجلة</summary>
    Task<bool> HasUnitsAsync(Guid productId, CancellationToken ct = default);

    /// <summary>جلب الوحدات الافتراضية للمنتج ككيانات متتبعة لتعديلها</summary>
    Task<IReadOnlyList<ProductUnit>> GetDefaultUnitsForUpdateAsync(Guid productId, CancellationToken ct = default);

    /// <summary>التحقق من أن وحدة القياس مستخدمة بالفعل لدى أي صنف في النظام</summary>
    Task<bool> IsUnitInUseAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب باركودات مجموعة منتجات محددة</summary>
    Task<IReadOnlyList<ProductBarCode>> GetBarcodesByProductIdsAsync(IEnumerable<Guid> fallbackProductIds, CancellationToken ct = default);

    /// <summary>جلب مجموعة أصناف بالمعرفات ككيانات متتبعة للتعديل</summary>
    Task<IReadOnlyList<Product>> GetByIdsForUpdateAsync(IEnumerable<Guid> productIds, CancellationToken ct = default);

    /// <summary>جلب مجموعة باركودات بالمعرفات كقراءة فقط دون تتبع</summary>
    Task<IReadOnlyList<ProductBarCode>> GetBarcodesByIdsAsync(IEnumerable<Guid> barcodeIds, CancellationToken ct = default);

    /// <summary>جلب مجموعة منتجات بالمعرفات كقراءة فقط دون تتبع</summary>
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> itemProductIds, CancellationToken ct = default);

    /// <summary>استرجاع ملكية الباركودات (المعرف، الباركود، ومعرف الصنف) كقراءة خفيفة</summary>
    Task<IReadOnlyList<ProductBarcodeOwnershipRow>> GetBarcodeOwnershipAsync(CancellationToken ct = default);

    /// <summary>التحقق السريع من وجود معرف باركود في النظام</summary>
    Task<bool> BarcodeIdExistsAsync(Guid productBarcodeId, CancellationToken ct = default);

    /// <summary>استرجاع ملكية الأصناف (المعرف، الاسم، والتصنيف) كقراءة خفيفة</summary>
    Task<IReadOnlyList<ProductOwnershipRow>> GetOwnershipAsync(CancellationToken ct = default);

    /// <summary>جلب كافة معرفات الباركودات المسجلة للمستأجر</summary>
    Task<IReadOnlyList<Guid>> GetBarcodeIdsAsync(CancellationToken ct = default);

    /// <summary>جلب كافة معرفات الأصناف المسجلة للمستأجر</summary>
    Task<IReadOnlyList<Guid>> GetIdsAsync(CancellationToken ct = default);

    /// <summary>جلب باركودات الأصناف المرشحة للتحديث أثناء استيراد الإكسل ككيانات متتبعة</summary>
    Task<IReadOnlyList<ProductBarCode>> GetImportBarcodesForUpdateAsync(ImportKeys keys, IEnumerable<Guid> productIds, CancellationToken ct = default);

    /// <summary>جلب وحدات الأصناف المرشحة للتحديث أثناء استيراد الإكسل ككيانات متتبعة</summary>
    Task<IReadOnlyList<ProductUnit>> GetImportUnitsForUpdateAsync(IEnumerable<Guid> productIds, CancellationToken ct = default);

    /// <summary>جلب كافة القيم النصية للباركودات المسجلة للمستأجر للتحقق السريع من التكرار في الذاكرة</summary>
    Task<IReadOnlyList<string>> GetBarcodeValuesAsync(CancellationToken ct = default);
}
