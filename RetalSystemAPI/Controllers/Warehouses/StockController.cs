using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Controllers.Warehouses;

/// <summary>
/// متحكم إدارة رصيد المخزون (مخازن التخزين وصالات العرض وتنبيهات الحد الأدنى).
/// </summary>
[Authorize]
[Route("api/stock")]
public class StockController : BaseApiController
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    // ── Storge Stock Endpoints (مخزون التخزين بالباركود) ──────

    /// <summary>
    /// الحصول على جميع الكميات المخزنية لمخزن تخزين محدد بالمعرف.
    /// </summary>
    [HttpGet("storge/{warehouseId:guid}")]
    public async Task<IActionResult> GetStorgeStocksByWarehouse([FromRoute] Guid warehouseId, CancellationToken ct)
    {
        var result = await _stockService.GetStorgeStocksByWarehouseAsync(warehouseId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على رصيد باركود محدد في مخزن تخزين معين.
    /// </summary>
    [HttpGet("storge/{warehouseId:guid}/barcode/{productBarcodeId:guid}")]
    public async Task<IActionResult> GetStorgeStock([FromRoute] Guid warehouseId, [FromRoute] Guid productBarcodeId, CancellationToken ct)
    {
        var result = await _stockService.GetStorgeStockAsync(warehouseId, productBarcodeId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تعيين أو تحديث رصيد التخزين والحد الأدنى للمخزون لباركود معين.
    /// </summary>
    [HttpPost("storge")]
    public async Task<IActionResult> SetStorgeStock([FromBody] SetStorgeStockDto dto, CancellationToken ct)
    {
        var result = await _stockService.SetStorgeStockAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تنبيهات وأرصدة المخزون المنخفضة (أقل من الحد الأدنى) لمخازن التخزين.
    /// </summary>
    [HttpGet("storge/low-stock")]
    public async Task<IActionResult> GetLowStorgeStockAlerts([FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
    {
        var result = await _stockService.GetLowStorgeStockAlertsAsync(warehouseId, ct);
        return ToActionResult(result);
    }

    // ── Showroom Stock Endpoints (مخزون صالات العرض بالمنتج) ──

    /// <summary>
    /// الحصول على جميع الكميات المخزنية لصالة عرض محددة بالمعرف.
    /// </summary>
    [HttpGet("showroom/{warehouseId:guid}")]
    public async Task<IActionResult> GetShowroomStocksByWarehouse([FromRoute] Guid warehouseId, CancellationToken ct)
    {
        var result = await _stockService.GetShowroomStocksByWarehouseAsync(warehouseId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على رصيد منتج محدد في صالة عرض معينة.
    /// </summary>
    [HttpGet("showroom/{warehouseId:guid}/product/{productId:guid}")]
    public async Task<IActionResult> GetShowroomStock([FromRoute] Guid warehouseId, [FromRoute] Guid productId, CancellationToken ct)
    {
        var result = await _stockService.GetShowroomStockAsync(warehouseId, productId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تعيين أو تحديث رصيد صالة العرض والحد الأدنى لمنتج معين.
    /// </summary>
    [HttpPost("showroom")]
    public async Task<IActionResult> SetShowroomStock([FromBody] SetShowroomStockDto dto, CancellationToken ct)
    {
        var result = await _stockService.SetShowroomStockAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تنبيهات وأرصدة المخزون المنخفضة (أقل من الحد الأدنى) لصالات العرض.
    /// </summary>
    [HttpGet("showroom/low-stock")]
    public async Task<IActionResult> GetLowShowroomStockAlerts([FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
    {
        var result = await _stockService.GetLowShowroomStockAlertsAsync(warehouseId, ct);
        return ToActionResult(result);
    }
}
