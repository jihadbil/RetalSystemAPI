using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Accounting.ExpenseCategories;
using RetalSystemAPI.Services.Accounting.Interfaces;

namespace RetalSystemAPI.Controllers.Accounting;

/// <summary>
/// متحكم إدارة تصنيفات المصروفات التشغيلية في النظام.
/// </summary>
[Authorize]
[Route("api/expense-categories")]
public class ExpenseCategoriesController : BaseApiController
{
    private readonly IExpenseCategoryService _categoryService;

    public ExpenseCategoriesController(IExpenseCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// استرجاع كافة تصنيفات المصروفات المسجلة.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.ExpenseCategories.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _categoryService.GetAllAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع صفحة بيانات مجزأة لتصنيفات المصروفات مع الفلترة والبحث.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.ExpenseCategories.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _categoryService.GetPagedAsync(isActive, search, pageNumber, pageSize, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل تصنيف مصروفات محدد بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.ExpenseCategories.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _categoryService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة تصنيف مصروفات جديد.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.ExpenseCategories.Create)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseCategoryDto dto, CancellationToken ct = default)
    {
        var result = await _categoryService.CreateAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات تصنيف مصروفات موجود.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.ExpenseCategories.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateExpenseCategoryDto dto, CancellationToken ct = default)
    {
        var result = await _categoryService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.ExpenseCategories.Edit)]
    public async Task<IActionResult> ToggleActive([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _categoryService.ToggleActiveAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف ناعم لتصنيف مصروفات بشرط عدم احتوائه على سندات مصروفات مسجلة.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.ExpenseCategories.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _categoryService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
