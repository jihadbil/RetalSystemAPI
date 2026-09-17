using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Sales.Interfaces;

namespace RetalSystemAPI.Controllers.Sales;

/// <summary>
/// متحكم إدارة مرتجعات المبيعات واسترجاع البضائع المباعة إلى رصيد المخزون ومعالجة أثرها المالي.
/// يوفر نقاط النهاية لإصدار فواتير المرتجع، الاستعلام عنها، والبحث برقم المرتجع.
/// </summary>
[Authorize]
[Route("api/sales-returns")]
public class SalesReturnsController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإصدار مرتجعات المبيعات.
    /// </summary>
    private readonly ISalesReturnService _salesReturnService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم مرتجعات المبيعات مع حقن خدمة المرتجعات.
    /// </summary>
    /// <param name="salesReturnService">واجهة خدمة مرتجعات المبيعات.</param>
    public SalesReturnsController(ISalesReturnService salesReturnService)
    {
        // إسناد خدمة مرتجعات المبيعات المحقونة إلى الحقل الخاص
        _salesReturnService = salesReturnService;
    }

    /// <summary>
    /// استرجاع جميع مرتجعات المبيعات مع إمكانية الفلترة بالفرع، المستودع، العميل، سبب الإرجاع، والفترة الزمنية.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة.</param>
    /// <param name="warehouseId">معرف المستودع للفلترة.</param>
    /// <param name="customerId">معرف العميل للفلترة.</param>
    /// <param name="reason">سبب المرتجع (تالف، خطأ في الصنف، رغبة العميل) للفلترة.</param>
    /// <param name="fromDate">تاريخ بداية الفترة للفلترة.</param>
    /// <param name="toDate">تاريخ نهاية الفترة للفلترة.</param>
    /// <param name="search">نص للبحث في رقم المرتجع أو الملاحظات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بمرتجعات المبيعات المطابقة للمعايير المحددة.</returns>
    [HttpGet]
    [HasPermission(Permissions.SalesReturns.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] SalesReturnReason? reason = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة مرتجعات المبيعات لجلب كافة المرتجعات وفق شروط الفلترة المحددة
        var result = await _salesReturnService.GetAllAsync(branchId, warehouseId, customerId, reason, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية لمرتجعات المبيعات مع دعم الفلترة والبحث والترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="branchId">معرف الفرع للفلترة.</param>
    /// <param name="warehouseId">معرف المستودع للفلترة.</param>
    /// <param name="customerId">معرف العميل للفلترة.</param>
    /// <param name="reason">سبب الإرجاع.</param>
    /// <param name="fromDate">من تاريخ.</param>
    /// <param name="toDate">إلى تاريخ.</param>
    /// <param name="search">نص البحث السريع.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات ملخص المرتجعات وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.SalesReturns.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] SalesReturnReason? reason = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة مرتجعات المبيعات لجلب صفحة المرتجعات المحددة
        var result = await _salesReturnService.GetPagedAsync(pageNumber, pageSize, branchId, warehouseId, customerId, reason, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع التفاصيل الكاملة لمرتجع مبيعات محدد بالمعرف متضمناً قائمة الأصناف المرتجعة.
    /// </summary>
    /// <param name="id">المعرف الفريد لمرتجع المبيعات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات مرتجع المبيعات التفصيلية أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.SalesReturns.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة مرتجعات المبيعات للبحث عن تفاصيل المرتجع بالمعرف المحدد
        var result = await _salesReturnService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن مرتجع مبيعات ومطابقته بواسطة رقم المرتجع المميز (ReturnNumber).
    /// </summary>
    /// <param name="returnNumber">رقم المرتجع النصي المراد البحث عنه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات مرتجع المبيعات المطابق أو خطأ 404.</returns>
    [HttpGet("number/{returnNumber}")]
    [HasPermission(Permissions.SalesReturns.View)]
    public async Task<IActionResult> GetByReturnNumber([FromRoute] string returnNumber, CancellationToken ct)
    {
        // استدعاء خدمة مرتجعات المبيعات للبحث عن المرتجع برقم المرتجع
        var result = await _salesReturnService.GetByReturnNumberAsync(returnNumber, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء وإصدار مرتجع مبيعات جديد وإعادة البضاعة المسترجعة إلى المخزون تلقائياً وتعديل رصيد العميل.
    /// </summary>
    /// <param name="dto">بيانات مرتجع المبيعات تشمل الفاتورة الأصلية وبنود الكميات المرتجعة والسبب.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المرتجع المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.SalesReturns.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSalesReturnDto dto, CancellationToken ct)
    {
        // استدعاء خدمة مرتجعات المبيعات لإنشاء المرتجع وإعادة الكميات للمخزون
        var result = await _salesReturnService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء المرتجع قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات المرتجع ورسالة التأكيد
            return StatusCode(201, ApiResponse<SalesReturnResponseDto>.Ok(result.Data!, "تم إنشاء مرتجع المبيعات وإعادة البضاعة للمخزون بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف أو أرشفة مرتجع مبيعات من النظام.
    /// </summary>
    /// <param name="id">المعرف الفريد لمرتجع المبيعات المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح عملية الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.SalesReturns.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة مرتجعات المبيعات لتنفيذ عملية حذف المرتجع
        var result = await _salesReturnService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
