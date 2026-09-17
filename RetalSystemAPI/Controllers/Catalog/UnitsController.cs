using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Catalog.Interfaces;

namespace RetalSystemAPI.Controllers.Catalog;

/// <summary>
/// متحكم إدارة وحدات القياس العامة (Units of Measure) في النظام.
/// يوفر نقاط النهاية لتعريف الوحدات الأساسية والفرعية (مثل: حبة، كرتون، لتر، متر) وإدارتها في كتالوج النظام.
/// </summary>
[Authorize]
[Route("api/catalog/units")]
public class UnitsController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة وحدات القياس.
    /// </summary>
    private readonly IUnitService _unitService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم وحدات القياس مع حقن خدمة الوحدات.
    /// </summary>
    /// <param name="unitService">واجهة خدمة وحدات القياس.</param>
    public UnitsController(IUnitService unitService)
    {
        // إسناد خدمة وحدات القياس المحقونة إلى الحقل الخاص
        _unitService = unitService;
    }

    /// <summary>
    /// استرجاع قائمة بجميع وحدات القياس المعرفة في النظام.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بكل وحدات القياس مغلفة في استجابة الـ API الموحدة.</returns>
    [HttpGet]
    [HasPermission(Permissions.Units.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // استدعاء خدمة وحدات القياس لجلب القائمة الكاملة لكافة الوحدات
        var result = await _unitService.GetAllAsync(ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل وحدة قياس محددة استناداً إلى معرفها الفريد.
    /// </summary>
    /// <param name="id">المعرف الفريد لوحدة القياس المستهدفة.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات وحدة القياس أو كود 404 في حال عدم العثور عليها.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Units.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة وحدات القياس للبحث عن تفاصيل الوحدة بالمعرف المحدد
        var result = await _unitService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء وحدة قياس جديدة في النظام.
    /// </summary>
    /// <param name="dto">بيانات وحدة القياس الجديدة المطلوب إنشاؤها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات وحدة القياس المنشأة مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Units.Create)]
    public async Task<IActionResult> Create([FromBody] CreateUnitDto dto, CancellationToken ct)
    {
        // استدعاء خدمة وحدات القياس لإنشاء الوحدة والتحقق من عدم تكرار الاسم
        var result = await _unitService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء الوحدة قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مصحوباً ببيانات الوحدة ورسالة التأكيد
            return StatusCode(201, ApiResponse<UnitResponseDto>.Ok(result.Data!, "تم إنشاء وحدة القياس بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات وحدة قياس موجودة في النظام.
    /// </summary>
    /// <param name="id">المعرف الفريد لوحدة القياس المراد تعديلها.</param>
    /// <param name="dto">البيانات الجديدة لوحدة القياس.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات وحدة القياس بعد التحديث أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Units.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUnitDto dto, CancellationToken ct)
    {
        // استدعاء خدمة وحدات القياس لتحديث بيانات الوحدة المحددة بالمعرف
        var result = await _unitService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف وحدة قياس من النظام في حال عدم ارتباطها بمنتجات في الكتالوج.
    /// </summary>
    /// <param name="id">المعرف الفريد لوحدة القياس المراد حذفها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح الحذف أو تبين مانع الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Units.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة وحدات القياس لتنفيذ عملية الحذف وفحص قيود الارتباط بالمنتجات
        var result = await _unitService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
