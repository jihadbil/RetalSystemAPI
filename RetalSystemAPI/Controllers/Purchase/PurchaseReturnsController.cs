using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Purchase.Interfaces;

namespace RetalSystemAPI.Controllers.Purchase;

/// <summary>
/// متحكم إدارة مرتجعات المشتريات وإرجاع البضائع إلى الموردين وخصمها من أرصدة المخازن آلياً.
/// يوفر نقاط النهاية لإنشاء فواتير إرجاع المشتريات، الاستعلام عنها، والبحث برقم المرتجع.
/// </summary>
[Authorize]
[Route("api/purchase-returns")]
public class PurchaseReturnsController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة مرتجعات المشتريات.
    /// </summary>
    private readonly IPurchaseReturnService _purchaseReturnService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم مرتجعات المشتريات مع حقن خدمة المشتريات.
    /// </summary>
    /// <param name="purchaseReturnService">واجهة خدمة مرتجعات المشتريات.</param>
    public PurchaseReturnsController(IPurchaseReturnService purchaseReturnService)
    {
        // إسناد خدمة مرتجعات المشتريات المحقونة إلى الحقل الخاص
        _purchaseReturnService = purchaseReturnService;
    }

    /// <summary>
    /// استرجاع جميع مرتجعات المشتريات مع دعم الفلترة متعددة المعايير (الفرع، المستودع، المورد، السبب، وطريقة الدفع والتواريخ).
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة.</param>
    /// <param name="warehouseId">معرف المستودع للفلترة.</param>
    /// <param name="supplierId">معرف المورد للفلترة.</param>
    /// <param name="reason">سبب إرجاع المشتريات (تالف، مخالف للمواصفات، إلخ) للفلترة.</param>
    /// <param name="paymentMethod">طريقة استرداد القيمة للفلترة.</param>
    /// <param name="fromDate">تاريخ بداية الفترة للفلترة.</param>
    /// <param name="toDate">تاريخ نهاية الفترة للفلترة.</param>
    /// <param name="search">نص للبحث في رقم المرتجع أو الملاحظات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بمرتجعات المشتريات المطابقة للشروط المحددة.</returns>
    [HttpGet]
    [HasPermission(Permissions.PurchaseReturns.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] PurchaseReturnReason? reason = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة مرتجعات المشتريات لجلب كافة المرتجعات وفق شروط الفلترة المحددة
        var result = await _purchaseReturnService.GetAllAsync(branchId, warehouseId, supplierId, reason, paymentMethod, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية لمرتجعات المشتريات مع دعم الفلترة والبحث والترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="branchId">معرف الفرع للفلترة.</param>
    /// <param name="warehouseId">معرف المستودع للفلترة.</param>
    /// <param name="supplierId">معرف المورد للفلترة.</param>
    /// <param name="reason">سبب المرتجع للفلترة.</param>
    /// <param name="paymentMethod">طريقة الدفع للفلترة.</param>
    /// <param name="fromDate">من تاريخ.</param>
    /// <param name="toDate">إلى تاريخ.</param>
    /// <param name="search">نص البحث السريع.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات ملخص مرتجعات المشتريات وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.PurchaseReturns.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] PurchaseReturnReason? reason = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة مرتجعات المشتريات لجلب صفحة المرتجعات المحددة
        var result = await _purchaseReturnService.GetPagedAsync(pageNumber, pageSize, branchId, warehouseId, supplierId, reason, paymentMethod, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل مرتجع مشتريات محدد بالمعرف متضمناً قائمة الأصناف المرتجعة والمبالغ المستردة.
    /// </summary>
    /// <param name="id">المعرف الفريد لمرتجع المشتريات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات مرتجع المشتريات التفصيلية أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.PurchaseReturns.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        // استدعاء خدمة مرتجعات المشتريات للبحث عن تفاصيل المرتجع بالمعرف المحدد
        var result = await _purchaseReturnService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن مرتجع مشتريات ومطابقته بواسطة رقم المرتجع المميز (ReturnNumber).
    /// </summary>
    /// <param name="returnNumber">رقم المرتجع النصي المراد البحث عنه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات مرتجع المشتريات المطابق أو خطأ 404.</returns>
    [HttpGet("number/{returnNumber}")]
    [HasPermission(Permissions.PurchaseReturns.View)]
    public async Task<IActionResult> GetByReturnNumber([FromRoute] string returnNumber, CancellationToken ct = default)
    {
        // استدعاء خدمة مرتجعات المشتريات للبحث عن المرتجع برقم المرتجع
        var result = await _purchaseReturnService.GetByReturnNumberAsync(returnNumber, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء وإصدار مرتجع مشتريات جديد وخصم البضاعة المرتجعة من رصيد المستودع تلقائياً.
    /// </summary>
    /// <param name="dto">بيانات مرتجع المشتريات تشمل الفاتورة الأصلية وبنود الكميات المرتجعة والسبب.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المرتجع المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.PurchaseReturns.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseReturnDto dto, CancellationToken ct = default)
    {
        // استدعاء خدمة مرتجعات المشتريات لإنشاء المرتجع وتنفيذ خصم المخزون
        var result = await _purchaseReturnService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء المرتجع قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات المرتجع ورسالة التأكيد
            return StatusCode(201, ApiResponse<PurchaseReturnResponseDto>.Ok(result.Data!, "تم إنشاء مرتجع المشتريات وخصم البضاعة من المخزن بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف أو أرشفة مرتجع مشتريات من النظام وإعادة البضاعة إلى رصيد المخزن في حال إلغاء المرتجع.
    /// </summary>
    /// <param name="id">المعرف الفريد لمرتجع المشتريات المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح عملية الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.PurchaseReturns.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
    {
        // استدعاء خدمة مرتجعات المشتريات لتنفيذ حذف سجل المرتجع
        var result = await _purchaseReturnService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
