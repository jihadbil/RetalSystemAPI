using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Catalog.Specifications;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة التصنيفات والهيكل الشجري الهرمي للأصناف والمنتجات.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة التصنيفات مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(new CategoryWithDetailsSpec(id), ct);
        if (category is null)
        {
            return ServiceResult<CategoryResponseDto>.Failure("التصنيف غير موجود", ErrorCodes.CategoryNotFound);
        }

        var result = _mapper.Map<CategoryResponseDto>(category);
        return ServiceResult<CategoryResponseDto>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await _unitOfWork.Categories.FindAsync(new CategoryWithDetailsSpec(), ct);
        var result = _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories);
        return ServiceResult<IReadOnlyList<CategoryResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetRootCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await _unitOfWork.Categories.FindAsync(new RootCategoriesSpec(), ct);
        var result = _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories);
        return ServiceResult<IReadOnlyList<CategoryResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default)
    {
        var categories = await _unitOfWork.Categories.FindAsync(new SubCategoriesSpec(parentId), ct);
        var result = _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories);
        return ServiceResult<IReadOnlyList<CategoryResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<CategoryResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var spec = new CategoryWithDetailsSpec();
        var (items, totalCount) = await _unitOfWork.Categories.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<CategoryResponseDto>>(items);
        var pagedResult = PagedResult<CategoryResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<CategoryResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        bool nameExists = await _unitOfWork.Categories.ExistsAsync(c => c.Name == dto.Name, ct);
        if (nameExists)
        {
            return ServiceResult<CategoryResponseDto>.Failure("اسم التصنيف موجود بالفعل", ErrorCodes.CategoryNameExists);
        }

        if (dto.ParentCategoryId.HasValue)
        {
            bool parentExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == dto.ParentCategoryId.Value, ct);
            if (!parentExists)
            {
                return ServiceResult<CategoryResponseDto>.Failure("التصنيف الأب المحدد غير موجود", ErrorCodes.CategoryNotFound);
            }
        }

        var category = _mapper.Map<Category>(dto);
        await _unitOfWork.Categories.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var createdCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(new CategoryWithDetailsSpec(category.Id), ct) ?? category;
        var responseDto = _mapper.Map<CategoryResponseDto>(createdCategory);

        return ServiceResult<CategoryResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);
        if (category is null)
        {
            return ServiceResult<CategoryResponseDto>.Failure("التصنيف غير موجود", ErrorCodes.CategoryNotFound);
        }

        bool nameExists = await _unitOfWork.Categories.ExistsAsync(c => c.Name == dto.Name && c.Id != id, ct);
        if (nameExists)
        {
            return ServiceResult<CategoryResponseDto>.Failure("اسم التصنيف موجود بالفعل لدى تصنيف آخر", ErrorCodes.CategoryNameExists);
        }

        if (dto.ParentCategoryId.HasValue)
        {
            if (dto.ParentCategoryId.Value == id)
            {
                return ServiceResult<CategoryResponseDto>.Failure("لا يمكن اختيار التصنيف نفسه كتصنيف أب", ErrorCodes.CategoryCircularRef);
            }

            bool isCircular = await IsCircularReferenceAsync(id, dto.ParentCategoryId.Value, ct);
            if (isCircular)
            {
                return ServiceResult<CategoryResponseDto>.Failure("لا يمكن الربط كتصنيف أب لأنه سيتسبب بحلقة دائرية", ErrorCodes.CategoryCircularRef);
            }
        }

        category.Name = dto.Name;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.IsActive = dto.IsActive;
        category.SortOrder = dto.SortOrder;

        await _unitOfWork.SaveChangesAsync(ct);

        var updatedCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(new CategoryWithDetailsSpec(id), ct) ?? category;
        var responseDto = _mapper.Map<CategoryResponseDto>(updatedCategory);

        return ServiceResult<CategoryResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(new CategoryWithDetailsSpec(id), ct);
        if (category is null)
        {
            return ServiceResult.Failure("التصنيف غير موجود", ErrorCodes.CategoryNotFound);
        }

        if (category.Products.Any() || category.SubCategories.Any())
        {
            return ServiceResult.Failure("لا يمكن حذف التصنيف لاحتوائه على منتجات أو تصنيفات فرعية مرتبطة", ErrorCodes.CategoryHasProducts);
        }

        _unitOfWork.Categories.SoftDelete(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);
        if (category is null)
        {
            return ServiceResult.Failure("التصنيف غير موجود", ErrorCodes.CategoryNotFound);
        }

        category.IsActive = !category.IsActive;
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    /// <summary>
    /// فحص ومنع تشكيل مراجع دائرية في الشجرة الهرمية للتصنيفات.
    /// </summary>
    private async Task<bool> IsCircularReferenceAsync(Guid categoryId, Guid targetParentId, CancellationToken ct)
    {
        Guid? currentParentId = targetParentId;
        while (currentParentId.HasValue)
        {
            if (currentParentId.Value == categoryId)
            {
                return true;
            }

            var parent = await _unitOfWork.Categories.GetByIdAsync(currentParentId.Value, ct);
            if (parent is null)
            {
                break;
            }

            currentParentId = parent.ParentCategoryId;
        }

        return false;
    }
}
