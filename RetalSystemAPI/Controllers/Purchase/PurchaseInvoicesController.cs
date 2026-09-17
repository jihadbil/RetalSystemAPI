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
/// متحكم إدارة فواتير المشتريات المباشرة الواردة من الموردين وزيادة المخزون في المستودعات تلقائياً.
/// يوفر نقاط النهاية لإصدار فواتير المشتريات، الاستعلام عنها، تعديلها، وإلغائها مع عكس أثرها المخزني.
/// </summary>
[Authorize]
[Route("api/purchase-invoices")]
public class PurchaseInvoicesController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة فواتير المشتريات والقيود المخزنية والمالية المرتبطة بها.
    /// </summary>
    private readonly IPurchaseInvoiceService _purchaseInvoiceService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم فواتير المشتريات مع حقن خدمة المشتريات.
    /// </summary>
    /// <param name="purchaseInvoiceService">واجهة خدمة فواتير المشتريات.</param>
    public PurchaseInvoicesController(IPurchaseInvoiceService purchaseInvoiceService)
    {
        // إسناد خدمة فواتير المشتريات المحقونة إلى الحقل الخاص
        _purchaseInvoiceService = purchaseInvoiceService;
    }

    /// <summary>
    /// استرجاع قائمة صفحية بفواتير المشتريات مع إمكانية الفلترة بالمورد، الفرع، المستودع، الحالة، وطريقة الدفع.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="supplierId">معرف المورد للفلترة.</param>
    /// <param name="branchId">معرف الفرع للفلترة.</param>
    /// <param name="warehouseId">معرف المستودع للفلترة.</param>
    /// <param name="status">حالة الفاتورة للفلترة.</param>
    /// <param name="paymentMethod">طريقة الدفع للفلترة.</param>
    /// <param name="fromDate">تاريخ بداية الفترة الزمنية للفلترة.</param>
    /// <param name="toDate">تاريخ نهاية الفترة الزمنية للفلترة.</param>
    /// <param name="search">نص للبحث في رقم الفاتورة أو الملاحظات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على ملخص فواتير المشتريات وإجمالي السجلات.</returns>
    [HttpGet]
    [HasPermission(Permissions.PurchaseInvoices.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة فواتير المشتريات لجلب صفحة الفواتير مع تطبيق معايير التصفية والبحث
        var result = await _purchaseInvoiceService.GetPagedAsync(
            pageNumber, pageSize, supplierId, branchId, warehouseId, status, paymentMethod, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل فاتورة مشتريات محددة بالمعرف متضمنة بنود الأصناف والتكاليف والمورد والمستودع.
    /// </summary>
    /// <param name="id">المعرف الفريد لفاتورة المشتريات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات فاتورة المشتريات التفصيلية أو كود 404 في حال عدم العثور عليها.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.PurchaseInvoices.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        // استدعاء خدمة فواتير المشتريات للبحث عن تفاصيل الفاتورة بالمعرف المحدد
        var result = await _purchaseInvoiceService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء وإصدار فاتورة مشتريات جديدة مع زيادة وإضافة كميات الأصناف إلى رصيد المستودع المختار تلقائياً.
    /// </summary>
    /// <param name="dto">بيانات فاتورة المشتريات تشمل المورد، المستودع، البنود المشتراة، والأسعار.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الفاتورة المنشأة مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.PurchaseInvoices.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        // استدعاء خدمة فواتير المشتريات لإنشاء الفاتورة وتغذية أرصدة المخزون بالكميات المشتراة
        var result = await _purchaseInvoiceService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء فاتورة المشتريات قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات الفاتورة ورسالة التأكيد
            return StatusCode(201, ApiResponse<PurchaseInvoiceResponseDto>.Ok(result.Data!, "تم إنشاء فاتورة المشتريات وزيادة المخزون بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات فاتورة مشتريات قائمة (مثل الملاحظات أو البيانات المالية).
    /// </summary>
    /// <param name="id">المعرف الفريد للفاتورة المراد تعديلها.</param>
    /// <param name="dto">البيانات الجديدة للفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الفاتورة بعد التحديث أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.PurchaseInvoices.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        // استدعاء خدمة فواتير المشتريات لتحديث بيانات الفاتورة المحددة
        var result = await _purchaseInvoiceService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إلغاء فاتورة مشتريات وعكس أثرها المخزني بخصم الكميات التي تم إدخالها مسبقاً.
    /// </summary>
    /// <param name="id">المعرف الفريد لفاتورة المشتريات المراد إلغاؤها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح إلغاء الفاتورة وعكس المخزون.</returns>
    [HttpPatch("{id:guid}/cancel")]
    [HasPermission(Permissions.PurchaseInvoices.Edit)]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken ct = default)
    {
        // استدعاء خدمة فواتير المشتريات لإلغاء الفاتورة وعكس حركات المخزون
        var result = await _purchaseInvoiceService.CancelAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف أو أرشفة فاتورة مشتريات من النظام (حذف منطقي).
    /// </summary>
    /// <param name="id">المعرف الفريد لفاتورة المشتريات المراد حذفها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح عملية حذف الفاتورة.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.PurchaseInvoices.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
    {
        // استدعاء خدمة فواتير المشتريات لتنفيذ حذف سجل الفاتورة
        var result = await _purchaseInvoiceService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
