using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة وحدات المنتج ومعاملات التحويل والأسعار الخاصة بكل وحدة وتعيين الوحدة الافتراضية.
/// يوفر نقاط النهاية لربط وحدات القياس المتعددة بالمنتج الواحد (مثل: حبة، باكت، كرتون).
/// </summary>
[Authorize]
[Route("api/catalog/products/{productId:guid}/units")]
public class ProductUnitsController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة وحدات المنتجات ومعاملات التحويل.
    /// </summary>
    private readonly IProductUnitService _productUnitService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم وحدات المنتج مع حقن خدمة وحدات المنتجات.
    /// </summary>
    /// <param name="productUnitService">واجهة خدمة وحدات المنتج.</param>
    public ProductUnitsController(IProductUnitService productUnitService)
    {
        // إسناد خدمة وحدات المنتج المحقونة إلى الحقل الخاص
        _productUnitService = productUnitService;
    }

    /// <summary>
    /// استرجاع قائمة بجميع وحدات القياس المرتبطة بمنتج محدد شاملاً معاملات التحويل وأسعار البيع.
    /// </summary>
    /// <param name="productId">المعرف الفريد للمنتج المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بوحدات المنتج ومعاملات تحويلها وأسعارها.</returns>
    [HttpGet]
    [HasPermission(Permissions.Products.View)]
    public async Task<IActionResult> GetByProduct([FromRoute] Guid productId, CancellationToken ct)
    {
        // استدعاء خدمة وحدات المنتج لجلب الوحدات المرتبطة بالمنتج المحدد
        var result = await _productUnitService.GetByProductAsync(productId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// ربط وحدة قياس جديدة بالمنتج مع تحديد معامل التحويل بالنسبة للوحدة الأساسية وأسعار البيع.
    /// </summary>
    /// <param name="productId">المعرف الفريد للمنتج المستهدف.</param>
    /// <param name="dto">بيانات وحدة المنتج الجديدة تشمل معرف الوحدة، معامل التحويل، وسعر البيع.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات وحدة المنتج المنشأة مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> AddUnitToProduct(
        [FromRoute] Guid productId,
        [FromBody] CreateProductUnitDto dto,
        CancellationToken ct)
    {
        // استدعاء خدمة وحدات المنتج لإضافة الوحدة والتحقق من عدم تكرارها للمنتج
        var result = await _productUnitService.AddUnitToProductAsync(productId, dto, ct);

        // التحقق مما إذا كانت عملية إضافة الوحدة قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات الوحدة ورسالة التأكيد
            return StatusCode(201, ApiResponse<ProductUnitResponseDto>.Ok(result.Data!, "تم إضافة وحدة القياس للمنتج بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// إزالة وفك ربط وحدة قياس من منتج في حال عدم وجود حركات مخزنية أو فواتير مسجلة بها.
    /// </summary>
    /// <param name="productUnitId">المعرف الفريد لسجل ربط وحدة المنتج المطلوب حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح إزالة الوحدة أو توضح المانع.</returns>
    [HttpDelete("{productUnitId:guid}")]
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> RemoveUnitFromProduct([FromRoute] Guid productUnitId, CancellationToken ct)
    {
        // استدعاء خدمة وحدات المنتج لتنفيذ حذف ربط الوحدة والتحقق من القيود
        var result = await _productUnitService.RemoveUnitFromProductAsync(productUnitId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تعيين وحدة القياس المحددة لتكون الوحدة الافتراضية للبيع والعرض للمنتج.
    /// </summary>
    /// <param name="productUnitId">المعرف الفريد لوحدة المنتج المراد جعلها افتراضية.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح تعيين الوحدة الافتراضية.</returns>
    [HttpPatch("{productUnitId:guid}/set-default")]
    [HasPermission(Permissions.Products.Edit)]
    public async Task<IActionResult> SetDefaultUnit([FromRoute] Guid productUnitId, CancellationToken ct)
    {
        // استدعاء خدمة وحدات المنتج لتعيين الوحدة كافتراضية وإلغاء الافتراضية عن الوحدات الأخرى
        var result = await _productUnitService.SetDefaultUnitAsync(productUnitId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
