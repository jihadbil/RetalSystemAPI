using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Controllers.Warehouses;

/// <summary>
/// متحكم إدارة المخازن المستودعية وصالات العرض وتوزيعها على الفروع.
/// يوفر نقاط النهاية للاستعلام والفلترة وإنشاء وتعديل وتجميد المخازن وحذفها وفق ضوابط خلو الرصيد.
/// </summary>
[Authorize]
[Route("api/warehouses")]
public class WarehousesController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة المستودعات وصالات العرض في النظام.
    /// </summary>
    private readonly IWarehouseService _warehouseService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم المستودعات مع حقن خدمة المستودعات.
    /// </summary>
    /// <param name="warehouseService">واجهة خدمة المستودعات.</param>
    public WarehousesController(IWarehouseService warehouseService)
    {
        // إسناد خدمة المستودعات المحقونة إلى الحقل الخاص
        _warehouseService = warehouseService;
    }

    /// <summary>
    /// استرجاع جميع المخازن وصالات العرض التابعة للمستأجر مع إمكانية التصفية بالفرع ونوع المستودع.
    /// </summary>
    /// <param name="branchId">معرف الفرع لتصفية المستودعات التابعة له (اختياري).</param>
    /// <param name="type">نوع المستودع (مخزن تخزين أو صالة عرض) للتصفية.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالمستودعات المطابقة لمعايير البحث مغلفة في استجابة موحدة.</returns>
    [HttpGet]
    [HasPermission(Permissions.Warehouses.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] WarehouseType? type = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة المستودعات لجلب كافة المستودعات والصالات المطابقة للتصفية
        var result = await _warehouseService.GetAllAsync(branchId, type, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية بالمخازن وصالات العرض مع دعم الترقيم والفلترة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="branchId">معرف الفرع للفلترة.</param>
    /// <param name="type">نوع المستودع للفلترة.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات ملخص المستودعات وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.Warehouses.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] WarehouseType? type = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة المستودعات لجلب صفحة المستودعات المحددة وفق شروط التصفية
        var result = await _warehouseService.GetPagedAsync(pageNumber, pageSize, branchId, type, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع البيانات التفصيلية لمخزن أو صالة عرض محددة بواسطة المعرف الفريد.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستودع المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستودع التفصيلية أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Warehouses.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة المستودعات للبحث عن المستودع بالمعرف المحدد
        var result = await _warehouseService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء مخزن تخزين أو صالة عرض جديدة وربطها بالفرع المعني.
    /// </summary>
    /// <param name="dto">بيانات المستودع الجديد المراد إنشاؤه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستودع المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Warehouses.Create)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المستودعات لإنشاء المستودع والتحقق من عدم تكرار الاسم في الفرع
        var result = await _warehouseService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء المستودع قد تكللت بالنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات المستودع ورسالة التأكيد
            return StatusCode(201, ApiResponse<WarehouseResponseDto>.Ok(result.Data!, "تم إنشاء المخزن/الصالة بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات مخزن أو صالة عرض قائمة في النظام.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستودع المراد تعديل بياناته.</param>
    /// <param name="dto">البيانات الجديدة للمستودع.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستودع بعد التحديث أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Warehouses.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateWarehouseDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المستودعات لتحديث بيانات المستودع المحدد
        var result = await _warehouseService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مخزن أو صالة عرض من النظام (يشترط خلوه التام من أي أرصدة مخزنية قائمة).
    /// </summary>
    /// <param name="id">المعرف الفريد للمستودع المطلوب حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح الحذف أو تبين المانع (وجود رصيد).</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Warehouses.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة المستودعات لتنفيذ عملية الحذف والتحقق من خلو الرصيد
        var result = await _warehouseService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة نشاط المستودع أو الصالة بين التفعيل والتعطيل (Toggle Active Status).
    /// </summary>
    /// <param name="id">المعرف الفريد للمستودع المراد تبديل حالته.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستودع بالوضعية المحدثة بعد تبديل حالته.</returns>
    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Warehouses.Edit)]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة المستودعات لتبديل حالة نشاط المستودع
        var result = await _warehouseService.ToggleActiveStatusAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
