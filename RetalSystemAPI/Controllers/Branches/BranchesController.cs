using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Branch;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Branch.Interfaces;

namespace RetalSystemAPI.Controllers.Branches;

/// <summary>
/// متحكم إدارة الفروع وأرقام هواتفها التابعة للمستأجر الحالي في النظام.
/// يوفر نقاط النهاية لإنشاء وتعديل واسترجاع وحذف الفروع وإدارة أرقام هواتف الاتصال الملحقة بها.
/// </summary>
[Authorize]
[Route("api/branches")]
public class BranchesController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة الفروع في طبقة الخدمات.
    /// </summary>
    private readonly IBranchService _branchService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم الفروع مع حقن خدمة الفروع.
    /// </summary>
    /// <param name="branchService">واجهة خدمة الفروع.</param>
    public BranchesController(IBranchService branchService)
    {
        // إسناد خدمة الفروع المحقونة إلى الحقل الخاص
        _branchService = branchService;
    }

    /// <summary>
    /// استرجاع قائمة بجميع الفروع التابعة للمستأجر الحالي.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بكل الفروع مغلفة في استجابة الـ API الموحدة.</returns>
    [HttpGet]
    [HasPermission(Permissions.Branches.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // استدعاء خدمة الفروع لجلب كافة الفروع المسجلة للمستأجر الحالي
        var result = await _branchService.GetAllAsync(ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية (Paged List) بالفروع مع دعم الترقيم وحجم الصفحة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على ملخصات الفروع وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.Branches.View)]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        // استدعاء خدمة الفروع لجلب صفحة الفروع المحددة
        var result = await _branchService.GetPagedAsync(pageNumber, pageSize, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل فرع محدد بالمعرف شاملاً بيانات الموقع وأرقام الهواتف التابعة له.
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الفرع التفصيلية أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Branches.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة الفروع للبحث عن تفاصيل الفرع بالمعرف المحدد
        var result = await _branchService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء فرع جديد للمستأجر مع أرقام الهواتف المبدئية.
    /// </summary>
    /// <param name="dto">بيانات الفرع الجديد المطلوب إضافته.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الفرع المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Branches.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto, CancellationToken ct)
    {
        // استدعاء خدمة الفروع لإنشاء الفرع الجديد والتحقق من عدم تكرار الاسم
        var result = await _branchService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء الفرع قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات الفرع ورسالة التأكيد
            return StatusCode(201, ApiResponse<BranchResponseDto>.Ok(result.Data!, "تم إنشاء الفرع بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات فرع قائم وقائمة أرقام هواتفه.
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع المراد تعديل بياناته.</param>
    /// <param name="dto">البيانات المحدثة للفرع والهواتف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الفرع بعد التحديث أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Branches.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateBranchDto dto, CancellationToken ct)
    {
        // استدعاء خدمة الفروع لتحديث بيانات الفرع المحدد وقائمة هواتفه
        var result = await _branchService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف فرع من النظام (حذف منطقي) في حال عدم ارتباطه بمستخدمين أو معاملات نشطة.
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح الحذف أو تبين مانع الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Branches.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة الفروع لتنفيذ الحذف المنطقي للفرع والتحقق من قيود الربط
        var result = await _branchService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة نشاط الفرع بين التفعيل والتعطيل (Toggle Active Status).
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع المراد تبديل حالته.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الفرع بالوضعية المحدثة بعد تبديل حالته.</returns>
    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Branches.Edit)]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة الفروع لتبديل حالة نشاط الفرع
        var result = await _branchService.ToggleActiveStatusAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
