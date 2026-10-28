using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات الأصناف والمنتجات (Products) مع إدارة وحداتها وباركوداتها وصورها كوحدة متكاملة.
/// </summary>
public partial interface IProductRepository : IRepository<Product>
{
    // ── إدارة وحدات المنتج (Product Units) ───────────────────────

    /// <summary>
    /// إضافة وحدة تعبئة وقياس جديدة للمنتج (كرتون، باكت، حبة).
    /// </summary>
    /// <param name="unit">كيان وحدة المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddUnitAsync(ProductUnit unit, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة وحدات قياس للمنتج دفعة واحدة.
    /// </summary>
    /// <param name="units">قائمة وحدات المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddUnitsAsync(IEnumerable<ProductUnit> units, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات وحدة المنتج وأسعارها ومعامل التحويل.
    /// </summary>
    /// <param name="unit">كيان وحدة المنتج المحدث.</param>
    void UpdateUnit(ProductUnit unit);

    /// <summary>
    /// إزالة وحدة المنتج من السياق المحلي وقاعدة البيانات.
    /// </summary>
    /// <param name="unit">كيان وحدة المنتج المراد حذفها.</param>
    void RemoveUnit(ProductUnit unit);

    /// <summary>
    /// تطبيق الحذف المنطقي على وحدة المنتج بتعيين IsDeleted = true.
    /// </summary>
    /// <param name="unit">كيان وحدة المنتج.</param>
    void SoftDeleteUnit(ProductUnit unit);

    /// <summary>
    /// الحذف النهائي والفيزيائي لوحدة المنتج من قاعدة البيانات.
    /// </summary>
    /// <param name="unit">كيان وحدة المنتج المراد حذفها نهائياً.</param>
    void HardDeleteUnit(ProductUnit unit);

    /// <summary>
    /// جلب كافة وحدات منتج محدد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="productId">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة وحدات المنتج.</returns>
    Task<IReadOnlyList<ProductUnit>> GetUnitsAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// جلب وحدة منتج بواسطة معرفها الفريد ككيان متتبع للتعديل.
    /// </summary>
    /// <param name="unitId">معرف وحدة المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان وحدة المنتج أو null.</returns>
    Task<ProductUnit?> GetUnitByIdAsync(Guid unitId, CancellationToken ct = default);

    // ── إدارة باركودات ونكهات المنتج (Product BarCodes) ─────────

    /// <summary>
    /// إضافة باركود أو نكهة جديدة للمنتج.
    /// </summary>
    /// <param name="barCode">كيان باركود المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddBarCodeAsync(ProductBarCode barCode, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة باركودات للمنتج دفعة واحدة.
    /// </summary>
    /// <param name="barCodes">قائمة باركودات المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddBarCodesAsync(IEnumerable<ProductBarCode> barCodes, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات باركود المنتج.
    /// </summary>
    /// <param name="barCode">كيان باركود المنتج المحدث.</param>
    void UpdateBarCode(ProductBarCode barCode);

    /// <summary>
    /// إزالة باركود من السياق وقاعدة البيانات.
    /// </summary>
    /// <param name="barCode">كيان الباركود المراد حذفه.</param>
    void RemoveBarCode(ProductBarCode barCode);

    /// <summary>
    /// تطبيق الحذف المنطقي على باركود المنتج.
    /// </summary>
    /// <param name="barCode">كيان الباركود.</param>
    void SoftDeleteBarCode(ProductBarCode barCode);

    /// <summary>
    /// الحذف النهائي والفيزيائي لباركود المنتج من قاعدة البيانات.
    /// </summary>
    /// <param name="barCode">كيان الباركود.</param>
    void HardDeleteBarCode(ProductBarCode barCode);

    /// <summary>
    /// جلب كافة باركودات منتج محدد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="productId">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة باركودات المنتج.</returns>
    Task<IReadOnlyList<ProductBarCode>> GetBarCodesAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// جلب باركود منتج بواسطة معرفه الفريد ككيان متتبع للتعديل.
    /// </summary>
    /// <param name="barCodeId">معرف الباركود.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان الباركود أو null.</returns>
    Task<ProductBarCode?> GetBarCodeByIdAsync(Guid barCodeId, CancellationToken ct = default);

    // ── إدارة صور المنتج (Product Images) ─────────────────────────

    /// <summary>
    /// إضافة صورة جديدة للمنتج أو لباركود محدد منه.
    /// </summary>
    /// <param name="image">كيان صورة المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddImageAsync(ProductImage image, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة صور للمنتج دفعة واحدة.
    /// </summary>
    /// <param name="images">قائمة صور المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddImagesAsync(IEnumerable<ProductImage> images, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات صورة المنتج (مثل جعلها الصورة الافتراضية).
    /// </summary>
    /// <param name="image">كيان الصورة المحدث.</param>
    void UpdateImage(ProductImage image);

    /// <summary>
    /// إزالة صورة من السياق المحلي وقاعدة البيانات.
    /// </summary>
    /// <param name="image">كيان الصورة المراد حذفه.</param>
    void RemoveImage(ProductImage image);

    /// <summary>
    /// تطبيق الحذف المنطقي على صورة المنتج.
    /// </summary>
    /// <param name="image">كيان الصورة.</param>
    void SoftDeleteImage(ProductImage image);

    /// <summary>
    /// الحذف النهائي والفيزيائي لصورة المنتج من قاعدة البيانات.
    /// </summary>
    /// <param name="image">كيان الصورة المراد حذفه نهائياً.</param>
    void HardDeleteImage(ProductImage image);

    /// <summary>
    /// جلب كافة صور منتج محدد كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="productId">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة صور المنتج.</returns>
    Task<IReadOnlyList<ProductImage>> GetImagesAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// جلب صورة منتج بواسطة معرفها الفريد ككيان متتبع للتعديل.
    /// </summary>
    /// <param name="imageId">معرف الصورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان صورة المنتج أو null.</returns>
    Task<ProductImage?> GetImageByIdAsync(Guid imageId, CancellationToken ct = default);

    /// <summary>
    /// جلب كيان صورة المنتج متتبعاً لتنفيذ عملية الحذف الفعلي.
    /// </summary>
    /// <param name="imageId">معرف الصورة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>كيان الصورة أو null.</returns>
    Task<ProductImage?> GetImageForDeletionAsync(Guid imageId, CancellationToken ct = default);

    /// <summary>
    /// حساب إجمالي عدد الصور المسجلة لمنتج محدد.
    /// </summary>
    /// <param name="productId">معرف المنتج.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>عدد الصور.</returns>
    Task<int> CountImagesAsync(Guid productId, CancellationToken ct = default);
}
