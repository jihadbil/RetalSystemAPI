using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
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
    /// الحصول على جميع المنتجات (بدون ترقيم صفحي) لاستخدامها في نقاط البيع والقوائم.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId = null, CancellationToken ct = default)
    {
        var result = await _productService.GetAllAsync(categoryId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية بالمنتجات مع تصفية اختارية حسب التصنيف.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.Products.View)]
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
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        var result = await _productService.SearchAsync(q, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن منتج بواسطة الباركود الخاص به.
    /// </summary>
    [HttpGet("by-barcode/{barCode}")]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetByBarCode([FromRoute] string barCode, CancellationToken ct)
    {
        var result = await _productService.GetByBarCodeAsync(barCode, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل المنتج كاملة بالمعرف (تتضمن الوحدات والباركوادت والصور).
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _productService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء منتج جديد.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Products.Create)]
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
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var result = await _productService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف منتج (حذف منطقي).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Products.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _productService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تصدير كافة بيانات الأصناف والباركودات والتصنيفات والوحدات والمخزون في ملف Excel.
    /// </summary>
    [HttpGet("export-excel")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> ExportExcel([FromQuery] bool singleSheet = true, CancellationToken ct = default)
    {
        var fileBytes = await _productExcelService.ExportProductsToExcelAsync(singleSheet, ct);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Products_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// تنزيل قالب Excel فارغ ومصمم بالأعمدة المطلوبة ومزود ببيانات توضيحية.
    /// </summary>
    [HttpGet("excel-template")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> DownloadTemplate([FromQuery] bool singleSheet = true, CancellationToken ct = default)
    {
        var fileBytes = await _productExcelService.DownloadTemplateAsync(singleSheet, ct);
        string filename = singleSheet ? "Products_Import_Template_SingleSheet.xlsx" : "Products_Import_Template_MultiSheet.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    /// <summary>
    /// فحص ومعاينة ملف Excel قبل الاستيراد الفعلي (Dry-Run Preview) للتأكد من سلامة البيانات وعرض الأخطاء والتنبيهات.
    /// </summary>
    [HttpPost("validate-excel")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> ValidateExcel(Microsoft.AspNetCore.Http.IFormFile file, [FromQuery] ProductExcelImportOptionsDto? options, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("يرجى تزويد ملف Excel مناسب للفحص", RetalSystemAPI.Services.Common.Models.ErrorCodes.ValidationError));
        }

        using var stream = file.OpenReadStream();
        var result = await _productExcelService.ValidateExcelAsync(stream, options, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// استيراد الأصناف والباركودات والتصنيفات والوحدات والمخزون من ملف Excel.
    /// </summary>
    [HttpPost("import-excel")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> ImportExcel(Microsoft.AspNetCore.Http.IFormFile file, [FromQuery] ProductExcelImportOptionsDto? options, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("يرجى تزويد ملف Excel مناسب للاستيراد", RetalSystemAPI.Services.Common.Models.ErrorCodes.ValidationError));
        }

        using var stream = file.OpenReadStream();
        var result = await _productExcelService.ImportProductsFromExcelAsync(stream, options, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// توليد وتحميل ملف Excel يحتوي على الأسطر المرفوضة فقط مع أسباب الخطأ.
    /// </summary>
    [HttpPost("export-rejected-excel")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> ExportRejectedExcel([FromBody] List<FailedRowDetailsDto> failedRows, CancellationToken ct)
    {
        if (failedRows == null || failedRows.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("لا توجد أسطر مرفوضة لتصديرها", RetalSystemAPI.Services.Common.Models.ErrorCodes.ValidationError));
        }

        var fileBytes = await _productExcelService.GenerateFailedRowsExcelAsync(failedRows, ct);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Rejected_Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}
