using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Customers.Interfaces;

namespace RetalSystemAPI.Controllers.Customers;

/// <summary>
/// متحكم إدارة العملاء وحساباتهم وأرقام هواتفهم في النظام.
/// يوفر نقاط النهاية للاستعلام والفلترة، وإنشاء وتعديل وتجميد العملاء، وإدارة أرقام هواتفهم المتعددة.
/// </summary>
[Authorize]
[Route("api/customers")]
public class CustomersController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة العملاء والعمليات المالية والبيانات المرتبطة بهم.
    /// </summary>
    private readonly ICustomerService _customerService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم العملاء مع حقن خدمة العملاء.
    /// </summary>
    /// <param name="customerService">واجهة خدمة العملاء.</param>
    public CustomersController(ICustomerService customerService)
    {
        // إسناد خدمة العملاء المحقونة إلى الحقل الخاص
        _customerService = customerService;
    }

    /// <summary>
    /// استرجاع جميع العملاء مع إمكانية التصفية بنوع العميل، حالة النشاط، والبحث النصي.
    /// </summary>
    /// <param name="type">نوع العميل (فردي، شركة، جملة، قطاعي) للتصفية.</param>
    /// <param name="isActive">حالة نشاط العميل للتصفية (نشط / غير نشط).</param>
    /// <param name="search">نص اختياري للبحث في الاسم أو الكود أو الهاتف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالعملاء المطابقين للمعايير المحددة.</returns>
    [HttpGet]
    [HasPermission(Permissions.Customers.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] CustomerType? type = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة العملاء لجلب قائمة العملاء وفقاً لشروط التصفية والبحث المحددة
        var result = await _customerService.GetAllAsync(type, isActive, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية بالعملاء مع دعم الفلترة والبحث والترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="type">نوع العميل للتصفية.</param>
    /// <param name="isActive">حالة النشاط للتصفية.</param>
    /// <param name="search">نص البحث السريع في بيانات العميل.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات ملخص العملاء وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.Customers.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] CustomerType? type = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة العملاء لجلب صفحة العملاء المحددة مع تطبيق معايير التصفية
        var result = await _customerService.GetPagedAsync(pageNumber, pageSize, type, isActive, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل عميل محدد بالمعرف شاملاً بيانات الحساب والحد الائتماني وقائمة الهواتف.
    /// </summary>
    /// <param name="id">المعرف الفريد للعميل المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات العميل التفصيلية أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Customers.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة العملاء للبحث عن تفاصيل العميل بالمعرف المحدد
        var result = await _customerService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة عميل جديد إلى النظام مع إمكانية إرفاق أرقام هواتفه المبدئية والحد الائتماني.
    /// </summary>
    /// <param name="dto">بيانات العميل الجديد المراد إنشاؤه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات العميل المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Customers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto, CancellationToken ct)
    {
        // استدعاء خدمة العملاء لإنشاء سجل العميل والتحقق من كود العميل والبيانات
        var result = await _customerService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إضافة العميل قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة 201 Created مصحوباً ببيانات العميل المنشأ ورسالة التأكيد
            return StatusCode(201, ApiResponse<CustomerResponseDto>.Ok(result.Data!, "تم إضافة العميل بنجاح"));
        }

        // تحويل وإرجاع كود الخطأ في حال فشل العملية
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات عميل قائم في النظام.
    /// </summary>
    /// <param name="id">المعرف الفريد للعميل المراد تعديله.</param>
    /// <param name="dto">البيانات الجديدة للعميل.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات العميل بعد التعديل أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Customers.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
    {
        // استدعاء خدمة العملاء لتحديث بيانات العميل المحدد
        var result = await _customerService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف أو أرشفة عميل (حذف منطقي) في حال عدم ارتباطه بفواتير مبيعات سابقة.
    /// </summary>
    /// <param name="id">المعرف الفريد للعميل المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح الحذف أو تبين المانع.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Customers.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة العملاء لتنفيذ حذف العميل وفحص قيود الفواتير المرتبطة
        var result = await _customerService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة نشاط العميل بين التفعيل والتعطيل (Toggle Active Status).
    /// </summary>
    /// <param name="id">المعرف الفريد للعميل المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات العميل بالوضعية المحدثة بعد تبديل حالته.</returns>
    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Customers.Edit)]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة العملاء لتبديل حالة نشاط العميل
        var result = await _customerService.ToggleActiveStatusAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة رقم هاتف جديد إلى قائمة هواتف العميل.
    /// </summary>
    /// <param name="customerId">المعرف الفريد للعميل المستهدف.</param>
    /// <param name="dto">بيانات رقم الهاتف ونوعه وما إذا كان أساسياً.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات العميل المحدثة بعد إضافة رقم الهاتف الجديد.</returns>
    [HttpPost("{customerId:guid}/phones")]
    [HasPermission(Permissions.Customers.Edit)]
    public async Task<IActionResult> AddPhone([FromRoute] Guid customerId, [FromBody] CustomerPhoneDto dto, CancellationToken ct)
    {
        // استدعاء خدمة العملاء لإضافة رقم الهاتف لسجل العميل المحدد
        var result = await _customerService.AddPhoneAsync(customerId, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف رقم هاتف محدد من قائمة أرقام هواتف العميل.
    /// </summary>
    /// <param name="customerId">المعرف الفريد للعميل المستهدف.</param>
    /// <param name="phoneId">المعرف الفريد لرقم الهاتف المطلوب حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح حذف رقم الهاتف.</returns>
    [HttpDelete("{customerId:guid}/phones/{phoneId:guid}")]
    [HasPermission(Permissions.Customers.Edit)]
    public async Task<IActionResult> DeletePhone([FromRoute] Guid customerId, [FromRoute] Guid phoneId, CancellationToken ct)
    {
        // استدعاء خدمة العملاء لحذف رقم الهاتف بالمعرفات المحددة
        var result = await _customerService.DeletePhoneAsync(customerId, phoneId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
