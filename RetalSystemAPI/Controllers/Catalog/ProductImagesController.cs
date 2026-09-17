using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.FileUpload.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة رفع ومعالجة صور المنتجات وربطها بالباركودات وتعيين الصورة الافتراضية للمنتج.
/// يوفر نقاط النهاية لاستعراض الصور، رفع صور متعددة، تعيين الصورة الأساسية، وحذف الصور.
/// </summary>
[Authorize]
[Route("api/catalog/products/{productId:guid}/images")]
public class ProductImagesController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة سجلات صور المنتجات في قاعدة البيانات.
    /// </summary>
    private readonly IProductImageService _productImageService;

    /// <summary>
    /// خدمة فحص وتخزين الملفات والصور على الخادم.
    /// </summary>
    private readonly IFileUploadService _fileUploadService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم صور المنتجات مع حقن الخدمات المطلوبة.
    /// </summary>
    /// <param name="productImageService">واجهة خدمة صور المنتجات.</param>
    /// <param name="fileUploadService">واجهة خدمة رفع الملفات.</param>
    public ProductImagesController(
        IProductImageService productImageService,
        IFileUploadService fileUploadService)
    {
        // إسناد خدمة صور المنتجات المحقونة
        _productImageService = productImageService;
        // إسناد خدمة رفع الملفات المحقونة
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// استرجاع جميع الصور المرفوعة والمرتبطة بمنتج محدد.
    /// </summary>
    /// <param name="productId">المعرف الفريد للمنتج المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بصور المنتج ومساراتها وحالتها (افتراضية أو عادية).</returns>
    [HttpGet]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetByProduct([FromRoute] Guid productId, CancellationToken ct)
    {
        // استدعاء خدمة صور المنتجات لجلب صور المنتج المحدد
        var result = await _productImageService.GetByProductAsync(productId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// رفع ملف صورة جديدة وتخزينها على الخادم ثم ربطها بالمنتج المحدد وتعيين خياراتها.
    /// </summary>
    /// <param name="productId">المعرف الفريد للمنتج المستهدف.</param>
    /// <param name="file">ملف الصورة المرفوع عبر نموذج الطلب المتعدد (multipart/form-data).</param>
    /// <param name="isDefault">تحديد ما إذا كانت هذه الصورة هي الصورة الافتراضية للمنتج.</param>
    /// <param name="barcodeId">معرف الباركود الخاص لربط الصورة بمتغير محدد (اختياري).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الصورة المنشأة مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Products.Edit)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(
        [FromRoute] Guid productId,
        IFormFile file,
        [FromForm] bool isDefault = false,
        [FromForm] Guid? barcodeId = null,
        CancellationToken ct = default)
    {
        // التحقق من تزويد ملف صورة صالح وألا يكون فارغاً
        if (file is null || file.Length == 0)
        {
            // إرجاع كود 400 لعدم توفير ملف الصورة
            return BadRequest(ApiResponse<ProductImageResponseDto>.Fail("ملف الصورة مطلوب", ErrorCodes.ValidationError));
        }

        // فحص امتداد ملف الصورة لضمان كونه مدعوماً ومسموحاً به (.jpg, .png, .webp)
        if (!_fileUploadService.IsValidImageExtension(file.FileName))
        {
            // إرجاع خطأ امتداد الملف غير المسموح
            return BadRequest(ApiResponse<ProductImageResponseDto>.Fail("نوع الملف غير مسموح به، يرجى رفع صورة صالحة (.jpg, .png, .webp)", ErrorCodes.InvalidFileType));
        }

        // فحص حجم الملف وألا يتجاوز الحد الأقصى المسموح به (5 ميجابايت)
        if (!_fileUploadService.IsWithinSizeLimit(file.Length))
        {
            // إرجاع خطأ تجاوز الحجم الأقصى
            return BadRequest(ApiResponse<ProductImageResponseDto>.Fail("حجم الصورة يتجاوز الحد المسموح به (5 ميجابايت)", ErrorCodes.FileTooLarge));
        }

        // 1. استدعاء خدمة رفع الملفات لحفظ الصورة في مجلد المنتجات
        var uploadResult = await _fileUploadService.UploadAsync(file, "products", ct);

        // التحقق من نجاح عملية التخزين على الخادم
        if (!uploadResult.IsSuccess)
        {
            // تحويل وإرجاع خطأ الرفع
            return ToActionResult(uploadResult);
        }

        // 2. إعداد كائن نقل البيانات لربط الصورة بالمنتج والباركود
        var dto = new CreateProductImageDto
        {
            ProductId = productId,
            ImageUrl = uploadResult.Data!,
            IsDefault = isDefault,
            BarcodeId = barcodeId
        };

        // استدعاء خدمة صور المنتجات لحفظ سجل الصورة وربطه بالمنتج في قاعدة البيانات
        var addResult = await _productImageService.AddImageAsync(productId, dto, ct);

        // التحقق من نجاح عملية الحفظ والربط
        if (addResult.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مصحوباً ببيانات الصورة ورسالة التأكيد
            return StatusCode(201, ApiResponse<ProductImageResponseDto>.Ok(addResult.Data!, "تم رفع ورابط الصورة بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(addResult);
    }

    /// <summary>
    /// حذف صورة منتج من قاعدة البيانات وإزالة ملفها الفعلي من الخادم.
    /// </summary>
    /// <param name="imageId">المعرف الفريد لسجل الصورة المراد حذفها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح حذف الصورة.</returns>
    [HttpDelete("{imageId:guid}")]
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> RemoveImage([FromRoute] Guid imageId, CancellationToken ct)
    {
        // استدعاء خدمة صور المنتجات لحذف سجل الصورة وملفها
        var result = await _productImageService.RemoveImageAsync(imageId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تعيين الصورة المحددة لتكون الصورة الافتراضية الرئيسية لعرض المنتج.
    /// </summary>
    /// <param name="imageId">المعرف الفريد للصورة المراد جعلها افتراضية.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح تعيين الصورة الافتراضية.</returns>
    [HttpPatch("{imageId:guid}/set-default")]
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> SetDefaultImage([FromRoute] Guid imageId, CancellationToken ct)
    {
        // استدعاء خدمة صور المنتجات لتعيين الصورة الافتراضية وإلغاء سابقتها
        var result = await _productImageService.SetDefaultImageAsync(imageId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
