using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة وحدات المنتج ومعاملات التحويل والوحدة الافتراضية.
/// </summary>
[Authorize]
[Route("api/catalog/products/{productId:guid}/units")]
public class ProductUnitsController : BaseApiController
{
    private readonly IProductUnitService _productUnitService;

    public ProductUnitsController(IProductUnitService productUnitService)
    {
        _productUnitService = productUnitService;
    }

    /// <summary>
    /// الحصول على وحدات قياس منتج محدد.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByProduct([FromRoute] Guid productId, CancellationToken ct)
    {
        var result = await _productUnitService.GetByProductAsync(productId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// ربط وحدة قياس جديدة بالمنتج مع معامل التحويل والسعر.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddUnitToProduct(
        [FromRoute] Guid productId,
        [FromBody] CreateProductUnitDto dto,
        CancellationToken ct)
    {
        var result = await _productUnitService.AddUnitToProductAsync(productId, dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<ProductUnitResponseDto>.Ok(result.Data!, "تم إضافة وحدة القياس للمنتج بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// إزالة وحدة قياس من المنتج.
    /// </summary>
    [HttpDelete("{productUnitId:guid}")]
    public async Task<IActionResult> RemoveUnitFromProduct([FromRoute] Guid productUnitId, CancellationToken ct)
    {
        var result = await _productUnitService.RemoveUnitFromProductAsync(productUnitId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تعيين وحدة القياس المحددة كوحدة افتراضية للمنتج.
    /// </summary>
    [HttpPatch("{productUnitId:guid}/set-default")]
    public async Task<IActionResult> SetDefaultUnit([FromRoute] Guid productUnitId, CancellationToken ct)
    {
        var result = await _productUnitService.SetDefaultUnitAsync(productUnitId, ct);
        return ToActionResult(result);
    }
}
