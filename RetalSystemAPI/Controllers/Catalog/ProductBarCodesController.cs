using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة الأكواد والباركودات الدولية والمحلية لمنتجات الكتالوج.
/// يوفر نقاط النهاية للاستعلام العام عن الباركودات، واسترجاع باركودات منتج معين، وإضافة وتعديل وحذف الباركودات.
/// </summary>
[Authorize]
[Route("api/catalog/products/{productId:guid}/barcodes")]
public class ProductBarCodesController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة باركودات المنتجات في النظام.
    /// </summary>
    private readonly IProductBarCodeService _productBarCodeService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم باركودات المنتجات مع حقن خدمة الباركودات.
    /// </summary>
    /// <param name="productBarCodeService">واجهة خدمة باركودات المنتجات.</param>
    public ProductBarCodesController(IProductBarCodeService productBarCodeService)
    {
        // إسناد خدمة الباركودات المحقونة إلى الحقل الخاص
        _productBarCodeService = productBarCodeService;
    }

    /// <summary>
    /// استرجاع جميع الأكواد والباركودات المعرفة بالنظام مع إمكانية البحث باسم المنتج أو قيمة الباركود.
    /// نقطة نهاية عامة على مستوى الكتالوج بالكامل.
    /// </summary>
    /// <param name="search">نص اختياري للبحث في قيم الباركودات أو أسماء المنتجات التابعة لها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بكل الباركودات المطابقة مغلفة في استجابة موحدة.</returns>
    [HttpGet("/api/catalog/barcodes")]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetAllBarCodes([FromQuery] string? search, CancellationToken ct)
    {
        // استدعاء خدمة الباركودات للبحث عن كافة الباركودات في الكتالوج
        var result = await _productBarCodeService.GetAllAsync(search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع جميع الأكواد والباركودات التابعة لمنتج محدد بالمعرف.
    /// </summary>
    /// <param name="productId">المعرف الفريد للمنتج المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالباركودات التابعة للمنتج المحدد.</returns>
    [HttpGet]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetByProduct([FromRoute] Guid productId, CancellationToken ct)
    {
        // استدعاء خدمة الباركودات لجلب باركودات المنتج المحدد
        var result = await _productBarCodeService.GetByProductAsync(productId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة باركود جديد لمنتج وربطه بوحدة قياس محددة والتأكد من عدم تكراره في النظام.
    /// </summary>
    /// <param name="productId">المعرف الفريد للمنتج المستهدف.</param>
    /// <param name="dto">بيانات الباركود الجديد تشمل القيمة والوحدة والتسمية.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الباركود المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> AddBarCode(
        [FromRoute] Guid productId,
        [FromBody] CreateProductBarCodeDto dto,
        CancellationToken ct)
    {
        // استدعاء خدمة الباركودات لإضافة الباركود والتحقق من فرادته على مستوى النظام
        var result = await _productBarCodeService.AddBarCodeAsync(productId, dto, ct);

        // التحقق مما إذا كانت عملية إضافة الباركود قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات الباركود ورسالة التأكيد
            return StatusCode(201, ApiResponse<ProductBarCodeResponseDto>.Ok(result.Data!, "تم إضافة الباركود بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات باركود خاص بمنتج (التسمية، قيمة الباركود، والوصف التوضيحي).
    /// </summary>
    /// <param name="barCodeId">المعرف الفريد لسجل الباركود المراد تعديله.</param>
    /// <param name="dto">البيانات الجديدة للباركود.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الباركود بعد التحديث أو كود الخطأ المناسب.</returns>
    [HttpPut("{barCodeId:guid}")]
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> UpdateBarCode(
        [FromRoute] Guid barCodeId,
        [FromBody] UpdateProductBarCodeDto dto,
        CancellationToken ct)
    {
        // استدعاء خدمة الباركودات لتحديث بيانات الباركود المحدد
        var result = await _productBarCodeService.UpdateBarCodeAsync(barCodeId, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف باركود خاص بمنتج من النظام.
    /// </summary>
    /// <param name="barCodeId">المعرف الفريد لسجل الباركود المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح عملية حذف الباركود.</returns>
    [HttpDelete("{barCodeId:guid}")]
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> RemoveBarCode([FromRoute] Guid barCodeId, CancellationToken ct)
    {
        // استدعاء خدمة الباركودات لتنفيذ حذف الباركود
        var result = await _productBarCodeService.RemoveBarCodeAsync(barCodeId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
