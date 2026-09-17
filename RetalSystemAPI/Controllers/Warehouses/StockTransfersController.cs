using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Controllers.Warehouses;

/// <summary>
/// متحكم إدارة أوامر التحويل المخزني ونقل البضائع بين المستودعات وصالات العرض وتعديل أرصدة الطرفين.
/// يوفر نقاط النهاية لإنشاء أوامر التحويل، استعراضها، البحث برقم التحويل، واعتمادها أو إلغائها.
/// </summary>
[Authorize]
[Route("api/stock-transfers")]
public class StockTransfersController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة عمليات التحويل المخزني.
    /// </summary>
    private readonly IStockTransferService _stockTransferService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم التحويلات المخزنية مع حقن خدمة التحويل.
    /// </summary>
    /// <param name="stockTransferService">واجهة خدمة التحويل المخزني.</param>
    public StockTransfersController(IStockTransferService stockTransferService)
    {
        // إسناد خدمة التحويل المخزني المحقونة إلى الحقل الخاص
        _stockTransferService = stockTransferService;
    }

    /// <summary>
    /// استرجاع جميع أوامر التحويل المخزني مع إمكانية الفلترة بالمستودع المحول منه وإليه والحالة والفترة الزمنية.
    /// </summary>
    /// <param name="fromWarehouseId">معرف المستودع المصدر للفلترة.</param>
    /// <param name="toWarehouseId">معرف المستودع الوجهة للفلترة.</param>
    /// <param name="status">حالة أمر التحويل (مسودة، مكتمل، ملغي) للفلترة.</param>
    /// <param name="fromDate">تاريخ بداية الفترة الزمنية للفلترة.</param>
    /// <param name="toDate">تاريخ نهاية الفترة الزمنية للفلترة.</param>
    /// <param name="search">نص للبحث في رقم التحويل أو الملاحظات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بأوامر التحويل المخزني المطابقة للمعايير المحددة.</returns>
    [HttpGet]
    [HasPermission(Permissions.StockTransfers.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? fromWarehouseId = null,
        [FromQuery] Guid? toWarehouseId = null,
        [FromQuery] StockTransferStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة التحويل المخزني لجلب كافة الأوامر وفقاً لمعايير الفلترة المحددة
        var result = await _stockTransferService.GetAllAsync(fromWarehouseId, toWarehouseId, status, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية لأوامر التحويل المخزني مع دعم الفلترة والبحث والترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="fromWarehouseId">المستودع المصدر.</param>
    /// <param name="toWarehouseId">المستودع الوجهة.</param>
    /// <param name="status">حالة التحويل.</param>
    /// <param name="fromDate">من تاريخ.</param>
    /// <param name="toDate">إلى تاريخ.</param>
    /// <param name="search">نص البحث السريع.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات ملخص أوامر التحويل وإجمالي السجلات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.StockTransfers.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? fromWarehouseId = null,
        [FromQuery] Guid? toWarehouseId = null,
        [FromQuery] StockTransferStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة التحويل المخزني لجلب صفحة أوامر التحويل المحددة
        var result = await _stockTransferService.GetPagedAsync(pageNumber, pageSize, fromWarehouseId, toWarehouseId, status, fromDate, toDate, search, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع التفاصيل الكاملة لأمر تحويل مخزني محدد بالمعرف متضمناً قائمة الأصناف المحولة.
    /// </summary>
    /// <param name="id">المعرف الفريد لأمر التحويل المخزني.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات أمر التحويل التفصيلية أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.StockTransfers.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة التحويل المخزني للبحث عن تفاصيل أمر التحويل بالمعرف المحدد
        var result = await _stockTransferService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// البحث عن أمر تحويل مخزني ومطابقته بواسطة رقم أمر التحويل المميز (TransferNumber).
    /// </summary>
    /// <param name="transferNumber">رقم التحويل النصي المراد البحث عنه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات أمر التحويل المطابق أو خطأ 404.</returns>
    [HttpGet("number/{transferNumber}")]
    [HasPermission(Permissions.StockTransfers.View)]
    public async Task<IActionResult> GetByTransferNumber([FromRoute] string transferNumber, CancellationToken ct)
    {
        // استدعاء خدمة التحويل المخزني للبحث عن الأمر برقم التحويل
        var result = await _stockTransferService.GetByTransferNumberAsync(transferNumber, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء أمر تحويل مخزني جديد كمسودة مبدئية (Draft) مع التحقق من كفاية الأرصدة بالمستودع المصدر.
    /// </summary>
    /// <param name="dto">بيانات أمر التحويل الجديد تشمل المستودعين وتفاصيل البنود والكميات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات أمر التحويل المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.StockTransfers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateStockTransferDto dto, CancellationToken ct)
    {
        // استدعاء خدمة التحويل المخزني لإنشاء أمر التحويل والتحقق من صحة المستودعات والأرصدة
        var result = await _stockTransferService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء أمر التحويل قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع بيانات الأمر ورسالة التأكيد
            return StatusCode(201, ApiResponse<StockTransferResponseDto>.Ok(result.Data!, "تم إنشاء أمر التحويل المخزني بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات وملاحظات وأصناف أمر تحويل مخزني قائم ما زال في حالة المسودة (Draft).
    /// </summary>
    /// <param name="id">المعرف الفريد لأمر التحويل المراد تعديله.</param>
    /// <param name="dto">البيانات والبنود المحدثة للتحويل.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات أمر التحويل بعد التحديث أو كود الخطأ المناسب.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.StockTransfers.Create)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateStockTransferDto dto, CancellationToken ct)
    {
        // استدعاء خدمة التحويل المخزني لتحديث بيانات أمر التحويل المحدد
        var result = await _stockTransferService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث حالة أمر التحويل المخزني (عند التحديث إلى Completed يتم خصم المخزون من المصدر وإضافته للوجهة فعلياً).
    /// </summary>
    /// <param name="id">المعرف الفريد لأمر التحويل المستهدف.</param>
    /// <param name="status">الحالة الجديدة المراد تطبيقها (Completed أو Canceled).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح تغيير الحالة وترحيل المخزون.</returns>
    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.StockTransfers.Approve)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] StockTransferStatus status, CancellationToken ct)
    {
        // استدعاء خدمة التحويل المخزني لتحديث الحالة وتنفيذ النقل الفعلي للمخزون
        var result = await _stockTransferService.UpdateStatusAsync(id, status, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف أو أرشفة أمر تحويل مخزني (يشترط ألا يكون مرحلاً ومكتملاً بالفعل).
    /// </summary>
    /// <param name="id">المعرف الفريد لأمر التحويل المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح الحذف أو تبين مانع الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.StockTransfers.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة التحويل المخزني لتنفيذ حذف أمر التحويل وفحص حالته
        var result = await _stockTransferService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
