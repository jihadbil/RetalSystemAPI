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
/// متحكم إدارة فواتير المبيعات وحركات البيع وخصم المخزون التلقائي من صالات العرض والمخازن.
/// يوفر نقاط النهاية لإصدار فواتير المبيعات، الاستعلام متعدد المعايير، التحديث، وتغيير الحالة مع إدارة استعادة المخزون عند الإلغاء.
/// </summary>
[Authorize]
[Route("api/sales-invoices")]
public class SalesInvoicesController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإصدار فواتير المبيعات وحساباتها المالية.
    /// </summary>
    private readonly ISalesInvoiceService _salesInvoiceService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم فواتير المبيعات مع حقن خدمة المبيعات.
    /// </summary>
    /// <param name="salesInvoiceService">واجهة خدمة فواتير المبيعات.</param>
    public SalesInvoicesController(ISalesInvoiceService salesInvoiceService)
    {
        // إسناد خدمة فواتير المبيعات المحقونة إلى الحقل الخاص
        _salesInvoiceService = salesInvoiceService;
    }

    /// <summary>
    /// استرجاع جميع فواتير المبيعات مع دعم الفلترة متعددة المعايير (الفرع، المخزن، العميل، الحالة، طريقة الدفع، والتواريخ).
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة.</param>
    /// <param name="warehouseId">معرف المستودع للفلترة.</param>
    /// <param name="customerId">معرف العميل للفلترة.</param>
    /// <param name="status">حالة الفاتورة (مدفوعة، معلقة، ملغاة) للفلترة.</param>
    /// <param name="paymentMethod">طريقة الدفع (نقدي، شبكة، آجل) للفلترة.</param>
    /// <param name="fromDate">تاريخ بداية الفترة الزمنية للفلترة.</param>
    /// <param name="toDate">تاريخ نهاية الفترة الزمنية للفلترة.</param>
    /// <param name="search">نص للبحث في رقم الفاتورة أو الملاحظات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بفواتير المبيعات المطابقة للمعايير المحددة.</returns>
    [HttpGet]
    [HasPermission(Permissions.SalesInvoices.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة فواتير المبيعات لجلب كافة الفواتير وفق شروط الفلترة المحددة
        var result = await _salesInvoiceService.GetAllAsync(branchId, warehouseId, customerId, status, paymentMethod, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية لفواتير المبيعات مع دعم الفلترة متعددة المعايير والبحث والترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="branchId">معرف الفرع.</param>
    /// <param name="warehouseId">معرف المستودع.</param>
    /// <param name="customerId">معرف العميل.</param>
    /// <param name="status">حالة الفاتورة.</param>
    /// <param name="paymentMethod">طريقة الدفع.</param>
    /// <param name="fromDate">من تاريخ.</param>
    /// <param name="toDate">إلى تاريخ.</param>
    /// <param name="search">نص البحث السريع.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على ملخص فواتير المبيعات وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.SalesInvoices.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة فواتير المبيعات لجلب صفحة فواتير المبيعات المحددة
        var result = await _salesInvoiceService.GetPagedAsync(pageNumber, pageSize, branchId, warehouseId, customerId, status, paymentMethod, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل فاتورة مبيعات محددة بالمعرف متضمنة البنود والكميات والأسعار والضرائب والخصومات.
    /// </summary>
    /// <param name="id">المعرف الفريد لفاتورة المبيعات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات فاتورة المبيعات التفصيلية الشاملة أو كود 404.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.SalesInvoices.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة فواتير المبيعات للبحث عن تفاصيل الفاتورة بالمعرف المحدد
        var result = await _salesInvoiceService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن فاتورة مبيعات ومطابقتها بواسطة رقم الفاتورة المميز (InvoiceNumber).
    /// </summary>
    /// <param name="invoiceNumber">رقم الفاتورة النصي المراد البحث عنه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات فاتورة المبيعات المطابقة أو خطأ 404.</returns>
    [HttpGet("number/{invoiceNumber}")]
    [HasPermission(Permissions.SalesInvoices.View)]
    public async Task<IActionResult> GetByInvoiceNumber([FromRoute] string invoiceNumber, CancellationToken ct)
    {
        // استدعاء خدمة فواتير المبيعات للبحث عن الفاتورة برقمها
        var result = await _salesInvoiceService.GetByInvoiceNumberAsync(invoiceNumber, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء وإصدار فاتورة مبيعات جديدة وخصم الكميات المباعة من رصيد المخزون في صالة العرض تلقائياً.
    /// </summary>
    /// <param name="dto">بيانات فاتورة المبيعات تشمل العميل، الصالة، طريقة الدفع، والبنود المشتراة.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الفاتورة الصادرة مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.SalesInvoices.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceDto dto, CancellationToken ct)
    {
        // استدعاء خدمة فواتير المبيعات لإصدار الفاتورة وتنفيذ عمليات الحساب المالي وخصم المخزون
        var result = await _salesInvoiceService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إصدار الفاتورة قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات الفاتورة ورسالة التأكيد
            return StatusCode(201, ApiResponse<SalesInvoiceResponseDto>.Ok(result.Data!, "تم إصدار فاتورة المبيعات بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات فاتورة مبيعات قائمة (طريقة الدفع، المبلغ المدفوع، والملاحظات).
    /// </summary>
    /// <param name="id">المعرف الفريد للفاتورة المراد تعديلها.</param>
    /// <param name="dto">البيانات المالية والملاحظات المحدثة للفاتورة.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الفاتورة بعد التعديل أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.SalesInvoices.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSalesInvoiceDto dto, CancellationToken ct)
    {
        // استدعاء خدمة فواتير المبيعات لتحديث بيانات الفاتورة المحددة
        var result = await _salesInvoiceService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث حالة فاتورة المبيعات (وفي حال الإلغاء أو الإبطال يتم استعادة البضائع للمخزون تلقائياً).
    /// </summary>
    /// <param name="id">المعرف الفريد للفاتورة المستهدفة.</param>
    /// <param name="status">الحالة الجديدة المراد تطبيقها (مثل Canceled أو Paid).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح تغيير حالة الفاتورة ومعالجة أثرها المخزني.</returns>
    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.SalesInvoices.Edit)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] InvoiceStatus status, CancellationToken ct)
    {
        // استدعاء خدمة فواتير المبيعات لتحديث الحالة ومعالجة ارتداد المخزون عند الإلغاء
        var result = await _salesInvoiceService.UpdateStatusAsync(id, status, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف أو أرشفة فاتورة مبيعات من النظام.
    /// </summary>
    /// <param name="id">المعرف الفريد للفاتورة المراد حذفها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح عملية الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.SalesInvoices.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة فواتير المبيعات لتنفيذ عملية حذف الفاتورة
        var result = await _salesInvoiceService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
