using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Suppliers.Interfaces;

namespace RetalSystemAPI.Controllers.Suppliers;

/// <summary>
/// متحكم إدارة الموردين وأرقام هواتفهم وبيانات الاتصال والتعاملات التجارية.
/// يوفر نقاط النهاية للاستعلام، الإضافة، التحديث، الحذف، وإدارة جهات الاتصال الخاصة بالموردين.
/// </summary>
[Authorize]
[Route("api/suppliers")]
public class SuppliersController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة بيانات الموردين في النظام.
    /// </summary>
    private readonly ISupplierService _supplierService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم الموردين مع حقن خدمة الموردين.
    /// </summary>
    /// <param name="supplierService">واجهة خدمة الموردين.</param>
    public SuppliersController(ISupplierService supplierService)
    {
        // إسناد خدمة الموردين المحقونة إلى الحقل الخاص
        _supplierService = supplierService;
    }

    /// <summary>
    /// استرجاع قائمة كاملة بجميع الموردين ملخصين.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة ملخصة بجميع الموردين المسجلين للمستأجر الحالي.</returns>
    [HttpGet]
    [HasPermission(Permissions.Suppliers.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // استدعاء خدمة الموردين لجلب كافة الموردين المسجلين
        var result = await _supplierService.GetAllAsync(ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية للموردين مع دعم البحث بالاسم التجاري أو أرقام الهواتف.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوبة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="search">نص اختياري للبحث في الاسم أو الهاتف أو البريد.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات ملخص الموردين وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.Suppliers.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة الموردين لجلب صفحة الموردين بناءً على معايير البحث والترقيم
        var result = await _supplierService.GetPagedAsync(pageNumber, pageSize, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل المورد كاملة مع قائمة أرقام هواتفه استناداً إلى معرفه الفريد.
    /// </summary>
    /// <param name="id">المعرف الفريد للمورد المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المورد التفصيلية أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Suppliers.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة الموردين للبحث عن تفاصيل المورد بالمعرف المحدد
        var result = await _supplierService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة مورد جديد في النظام مع إمكانية إرفاق أرقام هواتفه الأولية.
    /// </summary>
    /// <param name="dto">بيانات المورد الجديد المطلوب إضافته.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المورد المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Suppliers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto, CancellationToken ct)
    {
        // استدعاء خدمة الموردين لإنشاء المورد والتحقق من عدم تكرار الاسم التجاري
        var result = await _supplierService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إضافة المورد قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات المورد ورسالة التأكيد
            return StatusCode(201, ApiResponse<SupplierResponseDto>.Ok(result.Data!, "تم إضافة المورد بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات مورد قائم في النظام.
    /// </summary>
    /// <param name="id">المعرف الفريد للمورد المراد تعديل بياناته.</param>
    /// <param name="dto">البيانات الجديدة للمورد.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المورد بعد التعديل أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSupplierDto dto, CancellationToken ct)
    {
        // استدعاء خدمة الموردين لتحديث بيانات المورد المحدد بالمعرف
        var result = await _supplierService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مورد من النظام في حال عدم ارتباطه بأوامر شراء أو فواتير توريد سابقة.
    /// </summary>
    /// <param name="id">المعرف الفريد للمورد المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح الحذف أو توضح مانع الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Suppliers.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة الموردين لتنفيذ حذف المورد والتحقق من قيود المشتريات المرتبطة
        var result = await _supplierService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة رقم هاتف جديد إلى قائمة هواتف المورد.
    /// </summary>
    /// <param name="supplierId">المعرف الفريد للمورد المستهدف.</param>
    /// <param name="dto">بيانات رقم الهاتف الجديد ونوعه واسم جهة الاتصال.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المورد المحدثة بعد إضافة رقم الهاتف الجديد.</returns>
    [HttpPost("{supplierId:guid}/phones")]
    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<IActionResult> AddPhone([FromRoute] Guid supplierId, [FromBody] SupplierPhoneDto dto, CancellationToken ct)
    {
        // استدعاء خدمة الموردين لإضافة رقم الهاتف لسجل المورد المحدد
        var result = await _supplierService.AddPhoneAsync(supplierId, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف رقم هاتف محدد من قائمة أرقام هواتف المورد.
    /// </summary>
    /// <param name="supplierId">المعرف الفريد للمورد المستهدف.</param>
    /// <param name="phoneId">المعرف الفريد لرقم الهاتف المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح حذف رقم الهاتف.</returns>
    [HttpDelete("{supplierId:guid}/phones/{phoneId:guid}")]
    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<IActionResult> DeletePhone([FromRoute] Guid supplierId, [FromRoute] Guid phoneId, CancellationToken ct)
    {
        // استدعاء خدمة الموردين لحذف رقم الهاتف بالمعرفات المحددة
        var result = await _supplierService.DeletePhoneAsync(supplierId, phoneId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// توليد كشف حساب مالي تفصيلي للمورد خلال فترة محددة شاملاً الرصيد السابق وحركات الفترة ومجاميع المدين والدائن.
    /// </summary>
    /// <param name="id">المعرف الفريد للمورد.</param>
    /// <param name="fromDate">تاريخ بداية الفترة الاختياري.</param>
    /// <param name="toDate">تاريخ نهاية الفترة الاختياري.</param>
    /// <param name="branchId">معرف الفرع الاختياري للفلترة.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>تقرير كشف الحساب المالي للمورد.</returns>
    [HttpGet("{id:guid}/statement")]
    [HasPermission(Permissions.Suppliers.View)]
    public async Task<IActionResult> GetStatement(
        [FromRoute] Guid id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? branchId = null,
        CancellationToken ct = default)
    {
        var filter = new SupplierStatementFilterDto
        {
            SupplierId = id,
            FromDate = fromDate,
            ToDate = toDate,
            BranchId = branchId
        };
        var result = await _supplierService.GetStatementAsync(filter, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تسجيل سند صرف وسداد دفعة للمورد مع خصمها من الخزينة المحددة وتحديث رصيد المورد ذرّياً.
    /// </summary>
    /// <param name="dto">بيانات سداد المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات حركة السداد المسجلة.</returns>
    [HttpPost("payments")]
    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<IActionResult> AddPayment([FromBody] CreateSupplierPaymentDto dto, CancellationToken ct = default)
    {
        var result = await _supplierService.AddPaymentAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تسجيل تسوية رصيد حساب المورد (تعديل التزام دائن / مدين).
    /// </summary>
    /// <param name="dto">بيانات تسوية حساب المورد.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات حركة التسوية المسجلة.</returns>
    [HttpPost("adjustments")]
    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<IActionResult> AddAdjustment([FromBody] CreateSupplierAdjustmentDto dto, CancellationToken ct = default)
    {
        var result = await _supplierService.AddAdjustmentAsync(dto, ct);
        return ToActionResult(result);
    }
}
