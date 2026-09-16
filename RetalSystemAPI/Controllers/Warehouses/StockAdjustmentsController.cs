using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Controllers.Warehouses;

/// <summary>
/// متحكم إدارة التسويات الجردية وضبط أرصدة المخزون الفعلية ومعالجة الفوارق.
/// </summary>
[Authorize]
[Route("api/stock-adjustments")]
public class StockAdjustmentsController : BaseApiController
{
    private readonly IStockAdjustmentService _stockAdjustmentService;

    public StockAdjustmentsController(IStockAdjustmentService stockAdjustmentService)
    {
        _stockAdjustmentService = stockAdjustmentService;
    }

    /// <summary>
    /// الحصول على جميع التسويات الجردية مع إمكانية الفلترة بالمستودع أو سبب التسوية والتواريخ.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.StockAdjustments.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] StockAdjustmentReason? reason = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _stockAdjustmentService.GetAllAsync(warehouseId, reason, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية للتسويات الجردية.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.StockAdjustments.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] StockAdjustmentReason? reason = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _stockAdjustmentService.GetPagedAsync(pageNumber, pageSize, warehouseId, reason, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل تسوية جردية محددة بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.StockAdjustments.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _stockAdjustmentService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن تسوية جردية برقم التسوية.
    /// </summary>
    [HttpGet("number/{adjustmentNumber}")]
    [HasPermission(Permissions.StockAdjustments.View)]
    public async Task<IActionResult> GetByAdjustmentNumber([FromRoute] string adjustmentNumber, CancellationToken ct)
    {
        var result = await _stockAdjustmentService.GetByAdjustmentNumberAsync(adjustmentNumber, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء تسوية جردية جديدة وتعديل الأرصدة الفعلية في المخزون فورياً.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.StockAdjustments.Create)]
    public async Task<IActionResult> Create([FromBody] CreateStockAdjustmentDto dto, CancellationToken ct)
    {
        var result = await _stockAdjustmentService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<StockAdjustmentResponseDto>.Ok(result.Data!, "تم إنشاء التسوية الجردية وضبط المخزون بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// حذف/أرشفة تسوية جردية.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.StockAdjustments.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _stockAdjustmentService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
