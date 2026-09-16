using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة وحدات القياس الخاصة بالمنتجات.
/// </summary>
[Authorize]
[Route("api/catalog/units")]
public class UnitsController : BaseApiController
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    /// <summary>
    /// الحصول على جميع وحدات القياس.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Units.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _unitService.GetAllAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل وحدة قياس محددة بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Units.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _unitService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء وحدة قياس جديدة.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Units.Create)]
    public async Task<IActionResult> Create([FromBody] CreateUnitDto dto, CancellationToken ct)
    {
        var result = await _unitService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<UnitResponseDto>.Ok(result.Data!, "تم إنشاء وحدة القياس بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات وحدة قياس موجودة.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Units.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUnitDto dto, CancellationToken ct)
    {
        var result = await _unitService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف وحدة قياس.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Units.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _unitService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
