using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.FileUpload.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة رفع وصور المنتجات وتعيين الصورة الافتراضية.
/// </summary>
[Authorize]
[Route("api/catalog/products/{productId:guid}/images")]
public class ProductImagesController : BaseApiController
{
    private readonly IProductImageService _productImageService;
    private readonly IFileUploadService _fileUploadService;

    public ProductImagesController(
        IProductImageService productImageService,
        IFileUploadService fileUploadService)
    {
        _productImageService = productImageService;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// الحصول على جميع صور المنتج المحددة.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByProduct([FromRoute] Guid productId, CancellationToken ct)
    {
        var result = await _productImageService.GetByProductAsync(productId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// رفع صورة جديدة وإسنادها للمنتج.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(
        [FromRoute] Guid productId,
        IFormFile file,
        [FromForm] bool isDefault = false,
        [FromForm] Guid? barcodeId = null,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<ProductImageResponseDto>.Fail("ملف الصورة مطلوب", ErrorCodes.ValidationError));
        }

        if (!_fileUploadService.IsValidImageExtension(file.FileName))
        {
            return BadRequest(ApiResponse<ProductImageResponseDto>.Fail("نوع الملف غير مسموح به، يرجى رفع صورة صالحة (.jpg, .png, .webp)", ErrorCodes.InvalidFileType));
        }

        if (!_fileUploadService.IsWithinSizeLimit(file.Length))
        {
            return BadRequest(ApiResponse<ProductImageResponseDto>.Fail("حجم الصورة يتجاوز الحد المسموح به (5 ميجابايت)", ErrorCodes.FileTooLarge));
        }

        // 1. رفع الصورة إلى المجلد المحلي
        var uploadResult = await _fileUploadService.UploadAsync(file, "products", ct);
        if (!uploadResult.IsSuccess)
        {
            return ToActionResult(uploadResult);
        }

        // 2. ربط الصورة بالمنتج
        var dto = new CreateProductImageDto
        {
            ProductId = productId,
            ImageUrl = uploadResult.Data!,
            IsDefault = isDefault,
            BarcodeId = barcodeId
        };

        var addResult = await _productImageService.AddImageAsync(productId, dto, ct);
        if (addResult.IsSuccess)
        {
            return StatusCode(201, ApiResponse<ProductImageResponseDto>.Ok(addResult.Data!, "تم رفع ورابط الصورة بنجاح"));
        }

        return ToActionResult(addResult);
    }

    /// <summary>
    /// حذف صورة منتج.
    /// </summary>
    [HttpDelete("{imageId:guid}")]
    public async Task<IActionResult> RemoveImage([FromRoute] Guid imageId, CancellationToken ct)
    {
        var result = await _productImageService.RemoveImageAsync(imageId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تعيين الصورة كمظهر افتراضي للمنتج.
    /// </summary>
    [HttpPatch("{imageId:guid}/set-default")]
    public async Task<IActionResult> SetDefaultImage([FromRoute] Guid imageId, CancellationToken ct)
    {
        var result = await _productImageService.SetDefaultImageAsync(imageId, ct);
        return ToActionResult(result);
    }
}
