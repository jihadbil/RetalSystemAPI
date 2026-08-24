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
/// متحكم إدارة فواتير المبيعات وحركات البيع وخصم المخزون.
/// </summary>
[Authorize]
[Route("api/sales-invoices")]
public class SalesInvoicesController : BaseApiController
{
    private readonly ISalesInvoiceService _salesInvoiceService;

    public SalesInvoicesController(ISalesInvoiceService salesInvoiceService)
    {
        _salesInvoiceService = salesInvoiceService;
    }

    /// <summary>
    /// الحصول على جميع فواتير المبيعات مع دعم الفلترة متعددة المعايير وتصفية التواريخ.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _salesInvoiceService.GetAllAsync(branchId, warehouseId, customerId, status, paymentMethod, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية لفواتير المبيعات مع دعم الفلترة والبحث.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _salesInvoiceService.GetPagedAsync(pageNumber, pageSize, branchId, warehouseId, customerId, status, paymentMethod, fromDate, toDate, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل فاتورة مبيعات بالمعرف متضمنة البنود والأصناف.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _salesInvoiceService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن فاتورة مبيعات برقم الفاتورة.
    /// </summary>
    [HttpGet("number/{invoiceNumber}")]
    public async Task<IActionResult> GetByInvoiceNumber([FromRoute] string invoiceNumber, CancellationToken ct)
    {
        var result = await _salesInvoiceService.GetByInvoiceNumberAsync(invoiceNumber, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء وإصدار فاتورة مبيعات جديدة وخصم الكميات من المخزون تلقائياً.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceDto dto, CancellationToken ct)
    {
        var result = await _salesInvoiceService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<SalesInvoiceResponseDto>.Ok(result.Data!, "تم إصدار فاتورة المبيعات بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات فاتورة مبيعات (طريقة الدفع، المبلغ المدفوع، الملاحظات).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSalesInvoiceDto dto, CancellationToken ct)
    {
        var result = await _salesInvoiceService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث حالة الفاتورة (وفي حال الإلغاء أو الإبطال يتم استعادة المخزون تلقائياً).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] InvoiceStatus status, CancellationToken ct)
    {
        var result = await _salesInvoiceService.UpdateStatusAsync(id, status, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف/أرشفة فاتورة مبيعات.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _salesInvoiceService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
