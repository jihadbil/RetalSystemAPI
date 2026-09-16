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
/// متحكم إدارة مرتجعات المشتريات وإرجاع البضائع إلى الموردين وخصمها من المخازن.
/// </summary>
[Authorize]
[Route("api/purchase-returns")]
public class PurchaseReturnsController : BaseApiController
{
    private readonly IPurchaseReturnService _purchaseReturnService;

    public PurchaseReturnsController(IPurchaseReturnService purchaseReturnService)
    {
        _purchaseReturnService = purchaseReturnService;
    }

    /// <summary>
    /// جلب جميع مرتجعات المشتريات مع دعم الفلترة بالفرع، المستودع، المورد، طريقة الدفع، والتواريخ.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.PurchaseReturns.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] PurchaseReturnReason? reason = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _purchaseReturnService.GetAllAsync(branchId, warehouseId, supplierId, reason, paymentMethod, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// جلب قائمة صفحية لمرتجعات المشتريات مع دعم الفلترة والبحث.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.PurchaseReturns.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] PurchaseReturnReason? reason = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _purchaseReturnService.GetPagedAsync(pageNumber, pageSize, branchId, warehouseId, supplierId, reason, paymentMethod, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// جلب تفاصيل مرتجع مشتريات محدد بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.PurchaseReturns.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _purchaseReturnService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن مرتجع مشتريات بواسطة رقم المرتجع.
    /// </summary>
    [HttpGet("number/{returnNumber}")]
    [HasPermission(Permissions.PurchaseReturns.View)]
    public async Task<IActionResult> GetByReturnNumber([FromRoute] string returnNumber, CancellationToken ct = default)
    {
        var result = await _purchaseReturnService.GetByReturnNumberAsync(returnNumber, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء وإصدار مرتجع مشتريات جديد وخصم البضاعة من المخزن تلقائياً.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.PurchaseReturns.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseReturnDto dto, CancellationToken ct = default)
    {
        var result = await _purchaseReturnService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<PurchaseReturnResponseDto>.Ok(result.Data!, "تم إنشاء مرتجع المشتريات وخصم البضاعة من المخزن بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// حذف أو أرشفة مرتجع مشتريات وإعادة البضاعة إلى رصيد المخزن.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.PurchaseReturns.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _purchaseReturnService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
