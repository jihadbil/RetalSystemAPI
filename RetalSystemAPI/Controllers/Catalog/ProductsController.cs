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
/// متحكم إدارة المنتجات والبحث السريع عنها بالأكواد والأسماء والتصنيفات في كتالوج النظام،
/// بالإضافة إلى خدمات استيراد وتصدير الأصناف ومطابقتها عبر ملفات Excel المتقدمة.
/// </summary>
[Authorize]
[Route("api/catalog/products")]
public class ProductsController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة بيانات المنتجات والكتالوج.
    /// </summary>
    private readonly IProductService _productService;

    /// <summary>
    /// خدمة استيراد وتصدير وفحص ملفات Excel الخاصة بالمنتجات.
    /// </summary>
    private readonly IProductExcelService _productExcelService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم المنتجات وحقن الخدمات المطلوبة.
    /// </summary>
    /// <param name="productService">واجهة خدمة المنتجات.</param>
    /// <param name="productExcelService">واجهة خدمة معالجة ملفات Excel للمنتجات.</param>
    public ProductsController(IProductService productService, IProductExcelService productExcelService)
    {
        // إسناد خدمة المنتجات المحقونة إلى الحقل الخاص
        _productService = productService;
        // إسناد خدمة إكسل المنتجات المحقونة إلى الحقل الخاص
        _productExcelService = productExcelService;
    }

    /// <summary>
    /// استرجاع جميع المنتجات المتاحة (بدون ترقيم صفحي) لاستخدامها في شاشات الكاشير ونقاط البيع والقوائم المنسدلة.
    /// </summary>
    /// <param name="categoryId">معرف التصنيف لتصفية المنتجات التابعة له فقط (اختياري).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بكل المنتجات مغلفة في استجابة الـ API الموحدة.</returns>
    [HttpGet]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId = null, CancellationToken ct = default)
    {
        // استدعاء خدمة المنتجات لجلب كافة المنتجات مع تصفية التصنيف إن وجدت
        var result = await _productService.GetAllAsync(categoryId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية بالمنتجات مع إمكانية التصفية بحسب التصنيف.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="categoryId">معرف التصنيف لتصفية المنتجات (اختياري).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات المنتجات وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة المنتجات لجلب صفحة المنتجات المحددة
        var result = await _productService.GetPagedAsync(pageNumber, pageSize, categoryId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث السريع عن المنتجات باستخدام استعلام نصي يبحث في اسم المنتج أو كوده.
    /// </summary>
    /// <param name="q">النص المراد البحث عنه في قاعدة البيانات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالمنتجات المطابقة لنص البحث.</returns>
    [HttpGet("search")]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        // استدعاء خدمة المنتجات لتنفيذ البحث النصي بالاستعلام الممرر
        var result = await _productService.SearchAsync(q, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن منتج ومطابقته فورياً بواسطة الباركود الخاص به (ماسح الباركود).
    /// </summary>
    /// <param name="barCode">قيمة الباركود المطلوب قراءتها والبحث عنها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المنتج والوحدة المرتبطة بالباركود أو خطأ 404.</returns>
    [HttpGet("by-barcode/{barCode}")]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetByBarCode([FromRoute] string barCode, CancellationToken ct)
    {
        // استدعاء خدمة المنتجات لمطابقة الباركود وجلب المنتج التابع له
        var result = await _productService.GetByBarCodeAsync(barCode, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع التفاصيل الكاملة لمنتج محدد بالمعرف (يشمل الوحدات، الباركودات، الأسعار، والصور).
    /// </summary>
    /// <param name="id">المعرف الفريد للمنتج المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المنتج التفصيلية الشاملة أو كود 404.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة المنتجات للبحث عن تفاصيل المنتج بالمعرف المحدد
        var result = await _productService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء منتج جديد في الكتالوج مع تعريف وحداته وباركوداته الأولية.
    /// </summary>
    /// <param name="dto">بيانات المنتج الجديد المراد إنشاؤه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المنتج المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Products.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المنتجات لإنشاء المنتج والتحقق من صحة الكود والبيانات
        var result = await _productService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إضافة المنتج قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات المنتج ورسالة التأكيد
            return StatusCode(201, ApiResponse<ProductResponseDto>.Ok(result.Data!, "تم إنشاء المنتج بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات منتج قائم في النظام.
    /// </summary>
    /// <param name="id">المعرف الفريد للمنتج المراد تعديله.</param>
    /// <param name="dto">البيانات الجديدة للمنتج.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المنتج بعد التحديث أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المنتجات لتحديث بيانات المنتج المحدد
        var result = await _productService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف منتج من النظام (حذف منطقي) في حال عدم ارتباطه بحركات مخزنية أو فواتير.
    /// </summary>
    /// <param name="id">المعرف الفريد للمنتج المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح الحذف أو توضح مانع الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Products.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة المنتجات لتنفيذ حذف المنتج وفحص القيود
        var result = await _productService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تصدير كافة بيانات الأصناف والباركودات والتصنيفات والوحدات والمخزون في ملف مصنف Excel (.xlsx).
    /// </summary>
    /// <param name="singleSheet">تحديد ما إذا كان التصدير في ورقة عمل واحدة مدمجة أو أوراق عمل متعددة.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>ملف Excel يحتوي على كافة الأصناف بتنسيق احترافي وجاهز للتنزيل.</returns>
    [HttpGet("export-excel")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> ExportExcel([FromQuery] bool singleSheet = true, CancellationToken ct = default)
    {
        // استدعاء خدمة إكسل المنتجات لتوليد مصفوفة بايتات ملف الإكسل
        var fileBytes = await _productExcelService.ExportProductsToExcelAsync(singleSheet, ct);

        // إرجاع الملف للعميل للتنزيل المباشر مع تحديد نوع المحتوى واسم الملف الزمني
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Products_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// تنزيل قالب استيراد Excel فارغ ومصمم بالأعمدة المطلوبة ومزود ببيانات إرشادية وتوضيحية.
    /// </summary>
    /// <param name="singleSheet">تحديد نوع القالب (ورقة واحدة مدمجة أو أوراق عمل متعددة).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>ملف قالب الإكسل جاهز للتعبئة والتنزيل.</returns>
    [HttpGet("excel-template")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> DownloadTemplate([FromQuery] bool singleSheet = true, CancellationToken ct = default)
    {
        // استدعاء خدمة إكسل المنتجات لتوليد قالب الإكسل الإرشادي
        var fileBytes = await _productExcelService.DownloadTemplateAsync(singleSheet, ct);

        // تحديد اسم ملف القالب بناءً على النمط المختار
        string filename = singleSheet ? "Products_Import_Template_SingleSheet.xlsx" : "Products_Import_Template_MultiSheet.xlsx";

        // إرجاع ملف القالب للعميل للتنزيل المباشر
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    /// <summary>
    /// فحص ومعاينة ملف Excel قبل الاستيراد الفعلي (Dry-Run Preview) للتأكد من سلامة البيانات وعرض التنبيهات والأخطاء.
    /// </summary>
    /// <param name="file">ملف الإكسل المرفوع من المستخدم.</param>
    /// <param name="options">خيارات الاستيراد وقواعد المعالجة (التحديث، التجاوز، إلخ).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>تقرير مفصل بنتائج الفحص متضمناً عدد الأسطر السليمة والمرفوضة والتنبيهات.</returns>
    [HttpPost("validate-excel")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> ValidateExcel(Microsoft.AspNetCore.Http.IFormFile file, [FromQuery] ProductExcelImportOptionsDto? options, CancellationToken ct)
    {
        // التحقق من تزويد ملف غير فارغ في الطلب
        if (file == null || file.Length == 0)
        {
            // إرجاع كود 400 في حال عدم وجود ملف الإكسل
            return BadRequest(ApiResponse<object>.Fail("يرجى تزويد ملف Excel مناسب للفحص", RetalSystemAPI.Services.Common.Models.ErrorCodes.ValidationError));
        }

        // فتح دفق قراءة الملف المرفوع في الذاكرة
        using var stream = file.OpenReadStream();

        // استدعاء خدمة إكسل المنتجات لتنفيذ الفحص والمعاينة الأولية
        var result = await _productExcelService.ValidateExcelAsync(stream, options, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استيراد الأصناف والباركودات والتصنيفات والوحدات والمخزون من ملف Excel وحفظها في قاعدة البيانات.
    /// </summary>
    /// <param name="file">ملف الإكسل المرفوع المحتوي على بيانات الأصناف.</param>
    /// <param name="options">خيارات الاستيراد والتحديث.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>تقرير شامل بنتيجة الاستيراد النهائي والأسطر المنشأة والمحدثة.</returns>
    [HttpPost("import-excel")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> ImportExcel(Microsoft.AspNetCore.Http.IFormFile file, [FromQuery] ProductExcelImportOptionsDto? options, CancellationToken ct)
    {
        // التحقق من صلاحية الملف وتوفره في الطلب
        if (file == null || file.Length == 0)
        {
            // إرجاع كود 400 عند عدم وجود الملف
            return BadRequest(ApiResponse<object>.Fail("يرجى تزويد ملف Excel مناسب للاستيراد", RetalSystemAPI.Services.Common.Models.ErrorCodes.ValidationError));
        }

        // فتح دفق قراءة لملف الإكسل المرفوع
        using var stream = file.OpenReadStream();

        // استدعاء خدمة إكسل المنتجات لتنفيذ الاستيراد الفعلي وحفظ البيانات
        var result = await _productExcelService.ImportProductsFromExcelAsync(stream, options, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// توليد وتحميل ملف Excel مخصص يحتوي على الأسطر المرفوضة فقط مع إيضاح سبب الخطأ أمام كل صف للتصحيح.
    /// </summary>
    /// <param name="failedRows">قائمة الأسطر المرفوضة وتفاصيل أخطائها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>ملف Excel يحتوي على السجلات المتعثرة لتسهيل تعديلها وإعادة رفعها.</returns>
    [HttpPost("export-rejected-excel")]
    [HasPermission(Permissions.Products.ExportImport)]
    public async Task<IActionResult> ExportRejectedExcel([FromBody] List<FailedRowDetailsDto> failedRows, CancellationToken ct)
    {
        // التحقق من وجود أسطر مرفوضة في القائمة الممررة
        if (failedRows == null || failedRows.Count == 0)
        {
            // إرجاع كود 400 في حال كانت القائمة فارغة
            return BadRequest(ApiResponse<object>.Fail("لا توجد أسطر مرفوضة لتصديرها", RetalSystemAPI.Services.Common.Models.ErrorCodes.ValidationError));
        }

        // استدعاء خدمة إكسل المنتجات لتوليد ملف الإكسل الخاص بالأسطر المرفوضة
        var fileBytes = await _productExcelService.GenerateFailedRowsExcelAsync(failedRows, ct);

        // إرجاع الملف للعميل للتنزيل المباشر
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Rejected_Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}
