using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة إسناد وحدات القياس للمنتجات ومعامل التحويل.
/// </summary>
public interface IProductUnitService
{
    Task<ServiceResult<IReadOnlyList<ProductUnitResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ServiceResult<ProductUnitResponseDto>> AddUnitToProductAsync(Guid productId, CreateProductUnitDto dto, CancellationToken ct = default);
    Task<ServiceResult> RemoveUnitFromProductAsync(Guid productUnitId, CancellationToken ct = default);
    Task<ServiceResult> SetDefaultUnitAsync(Guid productUnitId, CancellationToken ct = default);
}
