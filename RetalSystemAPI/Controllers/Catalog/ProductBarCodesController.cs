using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة الأكواد والباركودات للمنتجات.
/// </summary>
[Authorize]
[Route("api/catalog/products/{productId:guid}/barcodes")]
public class ProductBarCodesController : BaseApiController
{
    private readonly IProductBarCodeService _productBarCodeService;

    public ProductBarCodesController(IProductBarCodeService productBarCodeService)
    {
        _productBarCodeService = productBarCodeService;
    }

    /// <summary>
    /// الحصول على جميع الأكواد/الباركودات الشاملة بالنظام مع إمكانية البحث باسم المنتج أو الباركود.
    /// </summary>
    [HttpGet("/api/catalog/barcodes")]
    public async Task<IActionResult> GetAllBarCodes([FromQuery] string? search, CancellationToken ct)
    {
        var result = await _productBarCodeService.GetAllAsync(search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على جميع الأكواد/الباركودات التابعة لمنتج معين.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByProduct([FromRoute] Guid productId, CancellationToken ct)
    {
        var result = await _productBarCodeService.GetByProductAsync(productId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة باركود جديد لمنتج.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddBarCode(
        [FromRoute] Guid productId,
        [FromBody] CreateProductBarCodeDto dto,
        CancellationToken ct)
    {
        var result = await _productBarCodeService.AddBarCodeAsync(productId, dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<ProductBarCodeResponseDto>.Ok(result.Data!, "تم إضافة الباركود بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات باركود خاص بمنتج (الاسم/التسمية وقيمة الباركود والوصف).
    /// </summary>
    [HttpPut("{barCodeId:guid}")]
    public async Task<IActionResult> UpdateBarCode(
        [FromRoute] Guid barCodeId,
        [FromBody] UpdateProductBarCodeDto dto,
        CancellationToken ct)
    {
        var result = await _productBarCodeService.UpdateBarCodeAsync(barCodeId, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف باركود خاص بمنتج.
    /// </summary>
    [HttpDelete("{barCodeId:guid}")]
    public async Task<IActionResult> RemoveBarCode([FromRoute] Guid barCodeId, CancellationToken ct)
    {
        var result = await _productBarCodeService.RemoveBarCodeAsync(barCodeId, ct);
        return ToActionResult(result);
    }
}
