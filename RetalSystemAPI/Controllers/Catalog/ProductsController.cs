using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة المنتجات والبحث عنها بالأكواد والاسم والتصنيف.
/// </summary>
[Authorize]
[Route("api/catalog/products")]
public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;
    private readonly IProductExcelService _productExcelService;

    public ProductsController(IProductService productService, IProductExcelService productExcelService)
    {
        _productService = productService;
        _productExcelService = productExcelService;
    }

    /// <summary>
    /// الحصول على قائمة صفحية بالمنتجات مع تصفية اختارية حسب التصنيف.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        CancellationToken ct = default)
    {
        var result = await _productService.GetPagedAsync(pageNumber, pageSize, categoryId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن منتجات باستخدام استعلام نصي (الاسم/الكود).
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        var result = await _productService.SearchAsync(q, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن منتج بواسطة الباركود الخاص به.
    /// </summary>
    [HttpGet("by-barcode/{barCode}")]
    public async Task<IActionResult> GetByBarCode([FromRoute] string barCode, CancellationToken ct)
    {
        var result = await _productService.GetByBarCodeAsync(barCode, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل المنتج كاملة بالمعرف (تتضمن الوحدات والباركوادت والصور).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _productService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء منتج جديد.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var result = await _productService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<ProductResponseDto>.Ok(result.Data!, "تم إنشاء المنتج بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات منتج موجود.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var result = await _productService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف منتج (حذف منطقي).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _productService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تصدير كافة بيانات الأصناف والباركودات والتصنيفات في ملف Excel يحوي 3 اوراق عمل.
    /// </summary>
    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportExcel(CancellationToken ct)
    {
        var fileBytes = await _productExcelService.ExportProductsToExcelAsync(ct);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Products_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// تنزيل قالب Excel فارغ ومصمم بالأعمدة المطلوبة ومزود ببيانات توضيحية.
    /// </summary>
    [HttpGet("excel-template")]
    public async Task<IActionResult> DownloadTemplate(CancellationToken ct)
    {
        var fileBytes = await _productExcelService.DownloadTemplateAsync(ct);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products_Import_Template.xlsx");
    }

    /// <summary>
    /// استيراد الأصناف والباركودات والتصنيفات من ملف Excel بحسب هيكلية 3 اوراق عمل.
    /// </summary>
    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportExcel(Microsoft.AspNetCore.Http.IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("يرجى تزويد ملف Excel مناسب للاستيراد", RetalSystemAPI.Services.Common.Models.ErrorCodes.ValidationError));
        }

        using var stream = file.OpenReadStream();
        var result = await _productExcelService.ImportProductsFromExcelAsync(stream, ct);
        return ToActionResult(result);
    }
}
