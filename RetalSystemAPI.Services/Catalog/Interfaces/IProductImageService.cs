using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة إدارة صور المنتجات وتحديد الصور الافتراضية وربط الصور بالباركودات/النكهات.
/// </summary>
public interface IProductImageService
{
    /// <summary>جلب كافة صور المنتج</summary>
    Task<ServiceResult<IReadOnlyList<ProductImageResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>إضافة صورة جديدة للمنتج أو لباركود محدد</summary>
    Task<ServiceResult<ProductImageResponseDto>> AddImageAsync(Guid productId, CreateProductImageDto dto, CancellationToken ct = default);

    /// <summary>حذف صورة منتج نهائياً</summary>
    Task<ServiceResult> RemoveImageAsync(Guid imageId, CancellationToken ct = default);

    /// <summary>تعيين صورة كالصورة الرئيسية الافتراضية للمنتج</summary>
    Task<ServiceResult> SetDefaultImageAsync(Guid imageId, CancellationToken ct = default);
}
