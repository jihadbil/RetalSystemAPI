using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة تصنيفات المنتجات وتفرعات الهيكل الشجري للتصنيفات في كتالوج المنتجات.
/// يوفر نقاط النهاية لجلب التصنيفات الشاملة، التصنيفات الجذرية، والتصنيفات الفرعية، وإنشاء وتعديل وحذف التصنيفات.
/// </summary>
[Authorize]
[Route("api/catalog/categories")]
public class CategoriesController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة تصنيفات المنتجات في النظام.
    /// </summary>
    private readonly ICategoryService _categoryService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم التصنيفات مع حقن خدمة التصنيفات.
    /// </summary>
    /// <param name="categoryService">واجهة خدمة التصنيفات.</param>
    public CategoriesController(ICategoryService categoryService)
    {
        // إسناد خدمة التصنيفات المحقونة إلى الحقل الخاص
        _categoryService = categoryService;
    }

    /// <summary>
    /// استرجاع قائمة كاملة بجميع التصنيفات المسجلة في النظام.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بكل التصنيفات مغلفة في استجابة الـ API الموحدة.</returns>
    [HttpGet]
    [HasPermission(Permissions.Categories.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // استدعاء خدمة التصنيفات لجلب قائمة كافة التصنيفات
        var result = await _categoryService.GetAllAsync(ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع التصنيفات الجذرية الرئيسية فقط (التي ليس لها تصنيف أب / المستوى الأول).
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالتصنيفات الجذرية الرئيسية.</returns>
    [HttpGet("roots")]
    [HasPermission(Permissions.Categories.View)]
    public async Task<IActionResult> GetRoots(CancellationToken ct)
    {
        // استدعاء خدمة التصنيفات لجلب التصنيفات الرئيسية التي لا ترتبط بأب
        var result = await _categoryService.GetRootCategoriesAsync(ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة التصنيفات الفرعية المباشرة التابعة لتصنيف أب محدد.
    /// </summary>
    /// <param name="parentId">المعرف الفريد للتصنيف الأب المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالتصنيفات التابعة للتصنيف الأب المحدد.</returns>
    [HttpGet("{parentId:guid}/children")]
    [HasPermission(Permissions.Categories.View)]
    public async Task<IActionResult> GetChildren([FromRoute] Guid parentId, CancellationToken ct)
    {
        // استدعاء خدمة التصنيفات لجلب الفروع التابعة للتصنيف الأب
        var result = await _categoryService.GetSubCategoriesAsync(parentId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية بالتصنيفات مع دعم الترقيم وحجم الصفحة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على ملخصات التصنيفات وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.Categories.View)]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        // استدعاء خدمة التصنيفات لجلب صفحة التصنيفات المحددة
        var result = await _categoryService.GetPagedAsync(pageNumber, pageSize, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل تصنيف محدد بالمعرف شاملاً بيانات الأب والأبناء والمنتجات المرتبطة.
    /// </summary>
    /// <param name="id">المعرف الفريد للتصنيف المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات التصنيف التفصيلية أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Categories.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة التصنيفات للبحث عن تفاصيل التصنيف بالمعرف المحدد
        var result = await _categoryService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء تصنيف منتجات جديد في النظام مع إمكانية ربطه بتصنيف أب.
    /// </summary>
    /// <param name="dto">بيانات التصنيف الجديد المطلوب إنشاؤه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات التصنيف المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Categories.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        // استدعاء خدمة التصنيفات لإنشاء التصنيف والتحقق من عدم تكرار الاسم أو المرجع الدائري
        var result = await _categoryService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء التصنيف قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات التصنيف ورسالة التأكيد
            return StatusCode(201, ApiResponse<CategoryResponseDto>.Ok(result.Data!, "تم إنشاء التصنيف بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات تصنيف موجود في النظام ومنع التداخل الدائري للأبناء.
    /// </summary>
    /// <param name="id">المعرف الفريد للتصنيف المراد تعديل بياناته.</param>
    /// <param name="dto">البيانات الجديدة للتصنيف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات التصنيف بعد التحديث أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Categories.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        // استدعاء خدمة التصنيفات لتحديث بيانات التصنيف المحدد وفحص صحة شجرة التبعية
        var result = await _categoryService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف تصنيف من النظام في حال عدم احتوائه على تصنيفات فرعية أو منتجات مرتبطة.
    /// </summary>
    /// <param name="id">المعرف الفريد للتصنيف المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح الحذف أو تبين مانع الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Categories.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة التصنيفات لتنفيذ عملية الحذف وفحص قيود الارتباط
        var result = await _categoryService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة نشاط التصنيف بين التفعيل والتعطيل (Toggle Active Status).
    /// </summary>
    /// <param name="id">المعرف الفريد للتصنيف المراد تبديل حالته.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات التصنيف بالوضعية المحدثة بعد تبديل حالته.</returns>
    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Categories.Edit)]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة التصنيفات لتبديل حالة نشاط التصنيف
        var result = await _categoryService.ToggleActiveStatusAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
