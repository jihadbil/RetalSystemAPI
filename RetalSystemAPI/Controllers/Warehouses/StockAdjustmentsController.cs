using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Controllers.Warehouses;

/// <summary>
/// متحكم إدارة التسويات الجردية وضبط أرصدة المخزون الفعلية ومعالجة الفوارق الناتجة عن الجرد الدوري، التلف، أو العجز والزيادة.
/// يوفر نقاط النهاية لإنشاء التسويات وتعديل المخزون فورياً، والبحث برقم التسوية واستعراض السجلات.
/// </summary>
[Authorize]
[Route("api/stock-adjustments")]
public class StockAdjustmentsController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة التسويات الجردية وضبط المخزون.
    /// </summary>
    private readonly IStockAdjustmentService _stockAdjustmentService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم التسويات الجردية مع حقن خدمة التسويات.
    /// </summary>
    /// <param name="stockAdjustmentService">واجهة خدمة التسويات الجردية.</param>
    public StockAdjustmentsController(IStockAdjustmentService stockAdjustmentService)
    {
        // إسناد خدمة التسويات الجردية المحقونة إلى الحقل الخاص
        _stockAdjustmentService = stockAdjustmentService;
    }

    /// <summary>
    /// استرجاع جميع التسويات الجردية مع إمكانية الفلترة بالمستودع، سبب التسوية، والفترة الزمنية.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع للفلترة.</param>
    /// <param name="reason">سبب التسوية (عجز، تلف، زيادة، جرد دوري) للفلترة.</param>
    /// <param name="fromDate">تاريخ بداية الفترة للفلترة.</param>
    /// <param name="toDate">تاريخ نهاية الفترة للفلترة.</param>
    /// <param name="search">نص للبحث في رقم التسوية أو الملاحظات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالتسويات الجردية المطابقة للشروط المحددة.</returns>
    [HttpGet]
    [HasPermission(Permissions.StockAdjustments.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] StockAdjustmentReason? reason = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة التسويات الجردية لجلب كافة التسويات وفق شروط التصفية
        var result = await _stockAdjustmentService.GetAllAsync(warehouseId, reason, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية للتسويات الجردية مع دعم الفلترة والبحث والترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="warehouseId">معرف المستودع للفلترة.</param>
    /// <param name="reason">سبب التسوية الجردية.</param>
    /// <param name="fromDate">من تاريخ.</param>
    /// <param name="toDate">إلى تاريخ.</param>
    /// <param name="search">نص البحث السريع.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات ملخص التسويات الجردية وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.StockAdjustments.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] StockAdjustmentReason? reason = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة التسويات الجردية لجلب صفحة التسويات المحددة
        var result = await _stockAdjustmentService.GetPagedAsync(pageNumber, pageSize, warehouseId, reason, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل تسوية جردية محددة بالمعرف متضمنة بنود الفروقات والكميات الدفترية والفعلية.
    /// </summary>
    /// <param name="id">المعرف الفريد لسجل التسوية الجردية المستهدفة.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات التسوية الجردية التفصيلية أو كود 404 في حال عدم العثور عليها.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.StockAdjustments.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة التسويات الجردية للبحث عن تفاصيل التسوية بالمعرف المحدد
        var result = await _stockAdjustmentService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن تسوية جردية ومطابقتها بواسطة رقم التسوية المميز (AdjustmentNumber).
    /// </summary>
    /// <param name="adjustmentNumber">رقم التسوية النصي المراد البحث عنه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات التسوية الجردية المطابقة أو خطأ 404.</returns>
    [HttpGet("number/{adjustmentNumber}")]
    [HasPermission(Permissions.StockAdjustments.View)]
    public async Task<IActionResult> GetByAdjustmentNumber([FromRoute] string adjustmentNumber, CancellationToken ct)
    {
        // استدعاء خدمة التسويات الجردية للبحث عن التسوية برقمها
        var result = await _stockAdjustmentService.GetByAdjustmentNumberAsync(adjustmentNumber, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء تسوية جردية جديدة وتعديل الأرصدة الفعلية في المخزون فورياً بناءً على فوارق الجرد.
    /// </summary>
    /// <param name="dto">بيانات التسوية الجردية الجديدة تشمل المستودع، السبب، وبنود الكميات الفعلية والفرق.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات التسوية المنشأة مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.StockAdjustments.Create)]
    public async Task<IActionResult> Create([FromBody] CreateStockAdjustmentDto dto, CancellationToken ct)
    {
        // استدعاء خدمة التسويات الجردية لإنشاء التسوية وتطبيق ضبط المخزون الفوري
        var result = await _stockAdjustmentService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء التسوية قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات التسوية ورسالة التأكيد
            return StatusCode(201, ApiResponse<StockAdjustmentResponseDto>.Ok(result.Data!, "تم إنشاء التسوية الجردية وضبط المخزون بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف أو أرشفة تسوية جردية من النظام.
    /// </summary>
    /// <param name="id">المعرف الفريد للتسوية الجردية المراد حذفها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح عملية الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.StockAdjustments.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة التسويات الجردية لتنفيذ حذف سجل التسوية
        var result = await _stockAdjustmentService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
