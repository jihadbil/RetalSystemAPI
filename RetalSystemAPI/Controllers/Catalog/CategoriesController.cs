using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة تصنيفات المنتجات والهيكل الهرمي للتصنيفات.
/// </summary>
[Authorize]
[Route("api/catalog/categories")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// الحصول على جميع التصنيفات.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _categoryService.GetAllAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على التصنيفات الجذرية (بدون تصنيف أب).
    /// </summary>
    [HttpGet("roots")]
    public async Task<IActionResult> GetRoots(CancellationToken ct)
    {
        var result = await _categoryService.GetRootCategoriesAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على التصنيفات الفرعية لتصنيف محدد.
    /// </summary>
    [HttpGet("{parentId:guid}/children")]
    public async Task<IActionResult> GetChildren([FromRoute] Guid parentId, CancellationToken ct)
    {
        var result = await _categoryService.GetSubCategoriesAsync(parentId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية بالتصنيفات.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _categoryService.GetPagedAsync(pageNumber, pageSize, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل تصنيف محدد بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _categoryService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء تصنيف جديد.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var result = await _categoryService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<CategoryResponseDto>.Ok(result.Data!, "تم إنشاء التصنيف بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل تصنيف موجود.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        var result = await _categoryService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف تصنيف.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _categoryService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة النشاط للتصنيف (تفعيل / تعطيل).
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _categoryService.ToggleActiveStatusAsync(id, ct);
        return ToActionResult(result);
    }
}
