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
/// متحكم إدارة فواتير المشتريات المباشرة.
/// </summary>
[Authorize]
[Route("api/purchase-invoices")]
public class PurchaseInvoicesController : BaseApiController
{
    private readonly IPurchaseInvoiceService _purchaseInvoiceService;

    public PurchaseInvoicesController(IPurchaseInvoiceService purchaseInvoiceService)
    {
        _purchaseInvoiceService = purchaseInvoiceService;
    }

    /// <summary>
    /// جلب قائمة صفحية بفواتير المشتريات مع إمكانية التصفية بالمورد، الفرع، المستودع، والحالة.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.PurchaseInvoices.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _purchaseInvoiceService.GetPagedAsync(
            pageNumber, pageSize, supplierId, branchId, warehouseId, status, paymentMethod, fromDate, toDate, search, ct);

        return ToActionResult(result);
    }

    /// <summary>
    /// جلب تفاصيل فاتورة مشتريات محددة بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.PurchaseInvoices.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _purchaseInvoiceService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء فاتورة مشتريات جديدة مع زيادة المخزون تلقائياً في المستودع المختار.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.PurchaseInvoices.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        var result = await _purchaseInvoiceService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<PurchaseInvoiceResponseDto>.Ok(result.Data!, "تم إنشاء فاتورة المشتريات وزيادة المخزون بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات فاتورة مشتريات.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.PurchaseInvoices.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        var result = await _purchaseInvoiceService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إلغاء فاتورة مشتريات.
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    [HasPermission(Permissions.PurchaseInvoices.Edit)]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _purchaseInvoiceService.CancelAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف فاتورة مشتريات (حذف منطقي).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.PurchaseInvoices.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _purchaseInvoiceService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
