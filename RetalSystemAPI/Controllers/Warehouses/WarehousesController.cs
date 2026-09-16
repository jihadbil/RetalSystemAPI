using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Controllers.Warehouses;

/// <summary>
/// متحكم إدارة المخازن وصالات العرض.
/// </summary>
[Authorize]
[Route("api/warehouses")]
public class WarehousesController : BaseApiController
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    /// <summary>
    /// الحصول على جميع المخازن وصالات العرض (مع إمكانية الفلترة بالفرع أو نوع المخزن).
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Warehouses.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] WarehouseType? type = null,
        CancellationToken ct = default)
    {
        var result = await _warehouseService.GetAllAsync(branchId, type, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية للمخازن وصالات العرض.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.Warehouses.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] WarehouseType? type = null,
        CancellationToken ct = default)
    {
        var result = await _warehouseService.GetPagedAsync(pageNumber, pageSize, branchId, type, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل مخزن أو صالة عرض بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Warehouses.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _warehouseService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء مخزن أو صالة عرض جديدة.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Warehouses.Create)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto, CancellationToken ct)
    {
        var result = await _warehouseService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<WarehouseResponseDto>.Ok(result.Data!, "تم إنشاء المخزن/الصالة بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات مخزن أو صالة عرض.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Warehouses.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateWarehouseDto dto, CancellationToken ct)
    {
        var result = await _warehouseService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مخزن أو صالة عرض (يشترط عدم وجود رصيد مخزوني به).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Warehouses.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _warehouseService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة النشاط للمخزن أو صالة العرض (تفعيل / تعطيل).
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Warehouses.Edit)]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _warehouseService.ToggleActiveStatusAsync(id, ct);
        return ToActionResult(result);
    }
}
