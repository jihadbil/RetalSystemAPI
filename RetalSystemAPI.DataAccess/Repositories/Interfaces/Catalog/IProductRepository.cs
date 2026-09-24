using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات الأصناف والمنتجات (Products) مع إدارة وحداتها وباركوداتها وصورها كوحدة متكاملة.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    // ── إدارة وحدات المنتج (Product Units) ───────────────────────

    /// <summary>إضافة وحدة تعبئة وقياس جديدة للمنتج</summary>
    Task AddUnitAsync(ProductUnit unit, CancellationToken ct = default);

    /// <summary>إضافة مجموعة وحدات للمنتج</summary>
    Task AddUnitsAsync(IEnumerable<ProductUnit> units, CancellationToken ct = default);

    /// <summary>تحديث بيانات وحدة المنتج</summary>
    void UpdateUnit(ProductUnit unit);

    /// <summary>إزالة وحدة من السياق المحلي</summary>
    void RemoveUnit(ProductUnit unit);

    /// <summary>حذف ناعم لوحدة المنتج</summary>
    void SoftDeleteUnit(ProductUnit unit);

    /// <summary>حذف نهائي لوحدة المنتج</summary>
    void HardDeleteUnit(ProductUnit unit);

    /// <summary>جلب كافة وحدات منتج محدد</summary>
    Task<IReadOnlyList<ProductUnit>> GetUnitsAsync(Guid productId, CancellationToken ct = default);

    /// <summary>البحث عن وحدات منتجات بشرط منطقي</summary>
    Task<IReadOnlyList<ProductUnit>> FindUnitsAsync(Expression<Func<ProductUnit, bool>> predicate, CancellationToken ct = default);

    /// <summary>البحث عن وحدات منتجات بمواصفة (Specification)</summary>
    Task<IReadOnlyList<ProductUnit>> FindUnitsAsync(ISpecification<ProductUnit> spec, CancellationToken ct = default);

    /// <summary>البحث عن وحدات منتجات مع تتبع التغييرات</summary>
    Task<IReadOnlyList<ProductUnit>> FindUnitsTrackedAsync(Expression<Func<ProductUnit, bool>> predicate, CancellationToken ct = default);

    /// <summary>جلب كافة وحدات المنتجات في النظام مع التتبع</summary>
    Task<IReadOnlyList<ProductUnit>> GetAllUnitsTrackedAsync(CancellationToken ct = default);

    /// <summary>جلب وحدة منتج بواسطة معرفها</summary>
    Task<ProductUnit?> GetUnitByIdAsync(Guid unitId, CancellationToken ct = default);

    /// <summary>التحقق من وجود وحدة مطابقة لشرط</summary>
    Task<bool> UnitExistsAsync(Expression<Func<ProductUnit, bool>> predicate, CancellationToken ct = default);

    /// <summary>جلب أول وحدة مطابقة لشرط مع التتبع</summary>
    Task<ProductUnit?> FirstOrDefaultUnitTrackedAsync(Expression<Func<ProductUnit, bool>> predicate, CancellationToken ct = default);

    /// <summary>إسقاط وتحديد حقول من وحدات المنتجات (Projection)</summary>
    Task<IReadOnlyList<TResult>> SelectUnitsAsync<TResult>(Expression<Func<ProductUnit, TResult>> selector, CancellationToken ct = default);


    // ── إدارة باركودات ونكهات المنتج (Product BarCodes) ─────────

    /// <summary>إضافة باركود/نكهة جديدة للمنتج</summary>
    Task AddBarCodeAsync(ProductBarCode barCode, CancellationToken ct = default);

    /// <summary>إضافة مجموعة باركودات للمنتج</summary>
    Task AddBarCodesAsync(IEnumerable<ProductBarCode> barCodes, CancellationToken ct = default);

    /// <summary>تحديث بيانات باركود</summary>
    void UpdateBarCode(ProductBarCode barCode);

    /// <summary>إزالة باركود من السياق المحلي</summary>
    void RemoveBarCode(ProductBarCode barCode);

    /// <summary>حذف ناعم لباركود</summary>
    void SoftDeleteBarCode(ProductBarCode barCode);

    /// <summary>حذف نهائي لباركود</summary>
    void HardDeleteBarCode(ProductBarCode barCode);

    /// <summary>جلب كافة باركودات منتج محدد</summary>
    Task<IReadOnlyList<ProductBarCode>> GetBarCodesAsync(Guid productId, CancellationToken ct = default);

    /// <summary>البحث عن باركودات بشرط منطقي</summary>
    Task<IReadOnlyList<ProductBarCode>> FindBarCodesAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default);

    /// <summary>البحث عن باركودات بمواصفة (Specification)</summary>
    Task<IReadOnlyList<ProductBarCode>> FindBarCodesAsync(ISpecification<ProductBarCode> spec, CancellationToken ct = default);

    /// <summary>البحث عن باركودات مع تتبع التغييرات</summary>
    Task<IReadOnlyList<ProductBarCode>> FindBarCodesTrackedAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default);

    /// <summary>جلب كافة الباركودات في النظام مع التتبع</summary>
    Task<IReadOnlyList<ProductBarCode>> GetAllBarCodesTrackedAsync(CancellationToken ct = default);

    /// <summary>جلب باركود بواسطة معرفه</summary>
    Task<ProductBarCode?> GetBarCodeByIdAsync(Guid barCodeId, CancellationToken ct = default);

    /// <summary>جلب أول باركود مطابق لشرط</summary>
    Task<ProductBarCode?> FirstOrDefaultBarCodeAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default);

    /// <summary>التحقق من وجود باركود مطابق لشرط</summary>
    Task<bool> BarCodeExistsAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default);

    /// <summary>جلب أول باركود مطابق لشرط مع التتبع</summary>
    Task<ProductBarCode?> FirstOrDefaultBarCodeTrackedAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default);

    /// <summary>إسقاط وتحديد حقول من الباركودات (Projection)</summary>
    Task<IReadOnlyList<TResult>> SelectBarCodesAsync<TResult>(Expression<Func<ProductBarCode, TResult>> selector, CancellationToken ct = default);


    // ── إدارة صور المنتج (Product Images) ─────────────────────────

    /// <summary>إضافة صورة للمنتج</summary>
    Task AddImageAsync(ProductImage image, CancellationToken ct = default);

    /// <summary>إضافة مجموعة صور للمنتج</summary>
    Task AddImagesAsync(IEnumerable<ProductImage> images, CancellationToken ct = default);

    /// <summary>تحديث بيانات صورة المنتج</summary>
    void UpdateImage(ProductImage image);

    /// <summary>إزالة صورة من السياق المحلي</summary>
    void RemoveImage(ProductImage image);

    /// <summary>حذف ناعم لصورة المنتج</summary>
    void SoftDeleteImage(ProductImage image);

    /// <summary>حذف نهائي لصورة المنتج</summary>
    void HardDeleteImage(ProductImage image);

    /// <summary>جلب كافة صور منتج محدد</summary>
    Task<IReadOnlyList<ProductImage>> GetImagesAsync(Guid productId, CancellationToken ct = default);

    /// <summary>البحث عن صور بشرط منطقي</summary>
    Task<IReadOnlyList<ProductImage>> FindImagesAsync(Expression<Func<ProductImage, bool>> predicate, CancellationToken ct = default);

    /// <summary>البحث عن صور مع تتبع التغييرات</summary>
    Task<IReadOnlyList<ProductImage>> FindImagesTrackedAsync(Expression<Func<ProductImage, bool>> predicate, CancellationToken ct = default);

    /// <summary>جلب صورة منتج بواسطة معرفها</summary>
    Task<ProductImage?> GetImageByIdAsync(Guid imageId, CancellationToken ct = default);

    /// <summary>التحقق من وجود صور للمنتج</summary>
    Task<bool> ImageExistsAsync(Expression<Func<ProductImage, bool>> predicate, CancellationToken ct = default);

    /// <summary>حساب عدد صور المنتج</summary>
    Task<int> CountImagesAsync(Guid productId, CancellationToken ct = default);
}
