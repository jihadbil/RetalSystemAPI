using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Sales.Interfaces;

namespace RetalSystemAPI.Controllers.Sales;

/// <summary>
/// متحكم إدارة مرتجعات المبيعات واسترجاع البضائع إلى المخزون.
/// </summary>
[Authorize]
[Route("api/sales-returns")]
public class SalesReturnsController : BaseApiController
{
    private readonly ISalesReturnService _salesReturnService;

    public SalesReturnsController(ISalesReturnService salesReturnService)
    {
        _salesReturnService = salesReturnService;
    }

    /// <summary>
    /// الحصول على جميع مرتجعات المبيعات مع إمكانية الفلترة بالفرع أو المستودع أو العميل أو سبب الإرجاع والتواريخ.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] SalesReturnReason? reason = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _salesReturnService.GetAllAsync(branchId, warehouseId, customerId, reason, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية لمرتجعات المبيعات مع دعم الفلترة والبحث.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] SalesReturnReason? reason = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _salesReturnService.GetPagedAsync(pageNumber, pageSize, branchId, warehouseId, customerId, reason, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل مرتجع مبيعات محدد بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _salesReturnService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن مرتجع مبيعات برقم المرتجع.
    /// </summary>
    [HttpGet("number/{returnNumber}")]
    public async Task<IActionResult> GetByReturnNumber([FromRoute] string returnNumber, CancellationToken ct)
    {
        var result = await _salesReturnService.GetByReturnNumberAsync(returnNumber, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء وإصدار مرتجع مبيعات جديد وإعادة البضاعة إلى المخزون تلقائياً.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesReturnDto dto, CancellationToken ct)
    {
        var result = await _salesReturnService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<SalesReturnResponseDto>.Ok(result.Data!, "تم إنشاء مرتجع المبيعات وإعادة البضاعة للمخزون بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// حذف/أرشفة مرتجع مبيعات.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _salesReturnService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
