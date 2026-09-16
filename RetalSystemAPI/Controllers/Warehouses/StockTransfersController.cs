using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Controllers.Warehouses;

/// <summary>
/// متحكم إدارة أوامر التحويل المخزني ونقل البضائع بين المستودعات والصالات.
/// </summary>
[Authorize]
[Route("api/stock-transfers")]
public class StockTransfersController : BaseApiController
{
    private readonly IStockTransferService _stockTransferService;

    public StockTransfersController(IStockTransferService stockTransferService)
    {
        _stockTransferService = stockTransferService;
    }

    /// <summary>
    /// الحصول على جميع أوامر التحويل المخزني مع إمكانية الفلترة بالمستودعات والحالة والتواريخ.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.StockTransfers.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? fromWarehouseId = null,
        [FromQuery] Guid? toWarehouseId = null,
        [FromQuery] StockTransferStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _stockTransferService.GetAllAsync(fromWarehouseId, toWarehouseId, status, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية لأوامر التحويل المخزني.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.StockTransfers.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? fromWarehouseId = null,
        [FromQuery] Guid? toWarehouseId = null,
        [FromQuery] StockTransferStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _stockTransferService.GetPagedAsync(pageNumber, pageSize, fromWarehouseId, toWarehouseId, status, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل أمر تحويل مخزني بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.StockTransfers.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _stockTransferService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن أمر تحويل مخزني برقم التحويل.
    /// </summary>
    [HttpGet("number/{transferNumber}")]
    [HasPermission(Permissions.StockTransfers.View)]
    public async Task<IActionResult> GetByTransferNumber([FromRoute] string transferNumber, CancellationToken ct)
    {
        var result = await _stockTransferService.GetByTransferNumberAsync(transferNumber, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء أمر تحويل مخزني جديد كمسودة (Draft).
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.StockTransfers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateStockTransferDto dto, CancellationToken ct)
    {
        var result = await _stockTransferService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<StockTransferResponseDto>.Ok(result.Data!, "تم إنشاء أمر التحويل المخزني بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات وملاحظات أمر التحويل المخزني.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.StockTransfers.Create)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateStockTransferDto dto, CancellationToken ct)
    {
        var result = await _stockTransferService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث حالة أمر التحويل (عند التحديث إلى Completed يتم ترحيل ونقل المخزون فعلياً بين المستودعين).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.StockTransfers.Approve)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] StockTransferStatus status, CancellationToken ct)
    {
        var result = await _stockTransferService.UpdateStatusAsync(id, status, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف/أرشفة أمر تحويل مخزني (يشترط ألا يكون مرحلاً ومكتملاً).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.StockTransfers.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _stockTransferService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
