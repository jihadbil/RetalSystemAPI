using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة إدارة صور المنتجات.
/// </summary>
public interface IProductImageService
{
    Task<ServiceResult<IReadOnlyList<ProductImageResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ServiceResult<ProductImageResponseDto>> AddImageAsync(Guid productId, CreateProductImageDto dto, CancellationToken ct = default);
    Task<ServiceResult> RemoveImageAsync(Guid imageId, CancellationToken ct = default);
    Task<ServiceResult> SetDefaultImageAsync(Guid imageId, CancellationToken ct = default);
}
