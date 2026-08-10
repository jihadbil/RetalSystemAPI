using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة المنتجات الأساسية.
/// </summary>
public interface IProductService
{
    Task<ServiceResult<ProductResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<ProductSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? categoryId = null, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> SearchAsync(string query, CancellationToken ct = default);
    Task<ServiceResult<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default);
    Task<ServiceResult<ProductResponseDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<ServiceResult<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
