using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة الأكواد والباركوادت الخاصة بالمنتجات.
/// </summary>
public interface IProductBarCodeService
{
    Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ServiceResult<ProductBarCodeResponseDto>> AddBarCodeAsync(Guid productId, CreateProductBarCodeDto dto, CancellationToken ct = default);
    Task<ServiceResult<ProductBarCodeResponseDto>> UpdateBarCodeAsync(Guid barCodeId, UpdateProductBarCodeDto dto, CancellationToken ct = default);
    Task<ServiceResult> RemoveBarCodeAsync(Guid barCodeId, CancellationToken ct = default);
}
