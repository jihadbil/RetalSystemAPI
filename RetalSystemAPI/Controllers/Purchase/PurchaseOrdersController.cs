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
/// متحكم إدارة طلبيات وأوامر الشراء من الموردين وتتبع دورة حياتها واعتمادها.
/// يوفر نقاط النهاية لإنشاء أوامر الشراء، استعراضها بصفحات، تحديث حالتها، وتعديلها.
/// </summary>
[Authorize]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة أوامر الشراء.
    /// </summary>
    private readonly IPurchaseOrderService _purchaseOrderService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم أوامر الشراء مع حقن خدمة المشتريات.
    /// </summary>
    /// <param name="purchaseOrderService">واجهة خدمة أوامر الشراء.</param>
    public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
    {
        // إسناد خدمة أوامر الشراء المحقونة إلى الحقل الخاص
        _purchaseOrderService = purchaseOrderService;
    }

    /// <summary>
    /// استرجاع جميع الطلبيات وأوامر الشراء مع إمكانية الفلترة بالفرع، المخزن، أو حالة الطلبية.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة.</param>
    /// <param name="warehouseId">معرف المستودع للفلترة.</param>
    /// <param name="status">حالة أمر الشراء (مسودة، معتمد، مستلم، ملغي) للفلترة.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بأوامر الشراء المطابقة للمعايير المحددة.</returns>
    [HttpGet]
    [HasPermission(Permissions.PurchaseOrders.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] PurchaseOrderStatus? status = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة أوامر الشراء لجلب كافة الأوامر وفق شروط التصفية
        var result = await _purchaseOrderService.GetAllAsync(branchId, warehouseId, status, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية بالطلبيات وأوامر الشراء مع دعم الفلترة والبحث والترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="branchId">معرف الفرع.</param>
    /// <param name="warehouseId">معرف المستودع.</param>
    /// <param name="status">حالة أمر الشراء.</param>
    /// <param name="search">نص البحث السريع في رقم الطلبية أو اسم المورد.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات ملخص أوامر الشراء وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.PurchaseOrders.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] PurchaseOrderStatus? status = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة أوامر الشراء لجلب صفحة أوامر الشراء المحددة
        var result = await _purchaseOrderService.GetPagedAsync(pageNumber, pageSize, branchId, warehouseId, status, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل طلبية معينة بالمعرف متضمنة قائمة أصناف الشراء والكميات وأسعار الشراء التقديرية.
    /// </summary>
    /// <param name="id">المعرف الفريد لأمر الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات أمر الشراء التفصيلية أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.PurchaseOrders.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة أوامر الشراء للبحث عن تفاصيل أمر الشراء بالمعرف المحدد
        var result = await _purchaseOrderService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء طلبية أو أمر شراء جديد وتعيينها بحالة مسودة (Draft) مبدئياً.
    /// </summary>
    /// <param name="dto">بيانات أمر الشراء تشمل المورد، المستودع، وبنود الأصناف والكميات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات أمر الشراء المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.PurchaseOrders.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto, CancellationToken ct)
    {
        // استدعاء خدمة أوامر الشراء لإنشاء الأمر الجديد والتحقق من صحة المورد والأصناف
        var result = await _purchaseOrderService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء أمر الشراء قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات الأمر ورسالة التأكيد
            return StatusCode(201, ApiResponse<PurchaseOrderResponseDto>.Ok(result.Data!, "تم إنشاء أمر الشراء بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث أمر شراء قائم وأصنافه ما دام في حالة مسودة (Draft).
    /// </summary>
    /// <param name="id">المعرف الفريد لأمر الشراء المراد تعديله.</param>
    /// <param name="dto">البيانات والبنود الجديدة لأمر الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات أمر الشراء بعد التحديث أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.PurchaseOrders.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePurchaseOrderDto dto, CancellationToken ct)
    {
        // استدعاء خدمة أوامر الشراء لتحديث بيانات أمر الشراء المحدد
        var result = await _purchaseOrderService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث وتغيير حالة أمر الشراء (مثل: مسودة Draft، اعتماد Submitted، تم الاستلام Received، ملغية Canceled).
    /// </summary>
    /// <param name="id">المعرف الفريد لأمر الشراء المستهدف.</param>
    /// <param name="status">الحالة الجديدة المراد تطبيقها على أمر الشراء.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح تغيير حالة أمر الشراء.</returns>
    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.PurchaseOrders.Approve)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] PurchaseOrderStatus status, CancellationToken ct)
    {
        // استدعاء خدمة أوامر الشراء لتحديث حالة أمر الشراء والتحقق من صحة التسلسل المنطقي
        var result = await _purchaseOrderService.UpdateStatusAsync(id, status, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف أو أرشفة أمر شراء من النظام (يشترط ألا يكون مستلماً أو مرتبطاً بفاتورة شراء).
    /// </summary>
    /// <param name="id">المعرف الفريد لأمر الشراء المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح حذف أمر الشراء.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.PurchaseOrders.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة أوامر الشراء لتنفيذ حذف أمر الشراء
        var result = await _purchaseOrderService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
