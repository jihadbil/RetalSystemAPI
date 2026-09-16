using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Purchase.Interfaces;

namespace RetalSystemAPI.Controllers.Purchase;

/// <summary>
/// متحكم إدارة الطلبيات وأوامر الشراء وتقاريرها.
/// </summary>
[Authorize]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : BaseApiController
{
    private readonly IPurchaseOrderService _purchaseOrderService;

    public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
    {
        _purchaseOrderService = purchaseOrderService;
    }

    /// <summary>
    /// الحصول على جميع الطلبيات وأوامر الشراء (مع إمكانية الفلترة بالفرع أو المخزن أو الحالة).
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.PurchaseOrders.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] PurchaseOrderStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await _purchaseOrderService.GetAllAsync(branchId, warehouseId, status, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية بالطلبيات والمشتريات.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.PurchaseOrders.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] PurchaseOrderStatus? status = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _purchaseOrderService.GetPagedAsync(pageNumber, pageSize, branchId, warehouseId, status, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل طلبية معينة بالمعرف بما في ذلك أصناف الشراء والتكلفة الإجمالية.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.PurchaseOrders.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _purchaseOrderService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء طلبية/أمر شراء جديد.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.PurchaseOrders.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto, CancellationToken ct)
    {
        var result = await _purchaseOrderService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<PurchaseOrderResponseDto>.Ok(result.Data!, "تم إنشاء أمر الشراء بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل أمر شراء قائم وأصنافه.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.PurchaseOrders.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePurchaseOrderDto dto, CancellationToken ct)
    {
        var result = await _purchaseOrderService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث حالة أمر الشراء (مثل: مسودة Draft، اعتماد Submitted، تم الاستلام Received، ملغية Canceled).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.PurchaseOrders.Approve)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] PurchaseOrderStatus status, CancellationToken ct)
    {
        var result = await _purchaseOrderService.UpdateStatusAsync(id, status, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف/أرشفة أمر شراء.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.PurchaseOrders.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _purchaseOrderService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
