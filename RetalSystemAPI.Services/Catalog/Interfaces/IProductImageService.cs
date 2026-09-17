using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة إدارة صور المنتجات وتحديد الصور الافتراضية وربط الصور بالباركودات/النكهات وحذفها من القرص.
/// </summary>
public interface IProductImageService
{
    /// <summary>
    /// جلب كافة صور المنتج مع بيانات الباركود المرتبط إن وجد.
    /// </summary>
    /// <param name="productId">معرف المنتج</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>قائمة بصور المنتج</returns>
    Task<ServiceResult<IReadOnlyList<ProductImageResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// إضافة صورة جديدة للمنتج أو لباركود محدد مع إمكانية تعيينها كصورة افتراضية.
    /// </summary>
    /// <param name="productId">معرف المنتج</param>
    /// <param name="dto">بيانات الصورة المراد إضافتها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الصورة المضافة</returns>
    Task<ServiceResult<ProductImageResponseDto>> AddImageAsync(Guid productId, CreateProductImageDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف صورة منتج نهائياً وحذف الملف المقترن بها من الخادم.
    /// </summary>
    /// <param name="imageId">معرف الصورة المراد حذفها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل العملية</returns>
    Task<ServiceResult> RemoveImageAsync(Guid imageId, CancellationToken ct = default);

    /// <summary>
    /// تعيين صورة كالصورة الرئيسية الافتراضية للمنتج وإلغاء الافتراضية عن بقية الصور.
    /// </summary>
    /// <param name="imageId">معرف الصورة المستهدفة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل العملية</returns>
    Task<ServiceResult> SetDefaultImageAsync(Guid imageId, CancellationToken ct = default);
}
