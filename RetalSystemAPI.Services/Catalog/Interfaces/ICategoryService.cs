using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة التصنيفات لإدارة الهيكل الهرمي للتصنيفات.
/// </summary>
public interface ICategoryService
{
    Task<ServiceResult<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetRootCategoriesAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<CategoryResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<ServiceResult<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<ServiceResult<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
