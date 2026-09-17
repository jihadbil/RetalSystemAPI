using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Controllers.Warehouses;

/// <summary>
/// متحكم إدارة رصيد المخزون الفعلي ومراقبة مستويات البضائع (مخازن التخزين وصالات العرض ورادار النواقص).
/// يوفر نقاط النهاية للاستعلام عن الأرصدة بالباركود أو بالمنتج، وتعيين حدود الأمان وتنبيهات إعادة الطلب.
/// </summary>
[Authorize]
[Route("api/stock")]
public class StockController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة ومراقبة حركة وأرصدة المخزون.
    /// </summary>
    private readonly IStockService _stockService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم المخزون مع حقن خدمة المخزون.
    /// </summary>
    /// <param name="stockService">واجهة خدمة المخزون.</param>
    public StockController(IStockService stockService)
    {
        // إسناد خدمة المخزون المحقونة إلى الحقل الخاص
        _stockService = stockService;
    }

    // ── Storge Stock Endpoints (مخزون التخزين بالباركود) ──────

    /// <summary>
    /// استرجاع جميع الكميات المخزنية لمخزن تخزين محدد بالمعرف (مخزون بالباركود).
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد لمخزن التخزين المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالأرصدة المخزنية لكافة الباركودات في المخزن المحدد.</returns>
    [HttpGet("storge/{warehouseId:guid}")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetStorgeStocksByWarehouse([FromRoute] Guid warehouseId, CancellationToken ct)
    {
        // استدعاء خدمة المخزون لجلب كافة أرصدة التخزين في المستودع المحدد
        var result = await _stockService.GetStorgeStocksByWarehouseAsync(warehouseId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع الكميات المخزنية لمخزن تخزين محدد مقسمة لصفحات مع دعم البحث والفلترة بالباركود.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد لمخزن التخزين.</param>
    /// <param name="pageNumber">رقم الصفحة المطلوبة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="searchTerm">نص البحث السريع في الباركود أو اسم المنتج.</param>
    /// <param name="exactBarcode">تحديد ما إذا كان البحث بالباركود الدقيق والمطابق تماماً.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على أرصدة الباركودات وإجمالي السجلات.</returns>
    [HttpGet("storge/{warehouseId:guid}/paged")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetPagedStorgeStocksByWarehouse(
        [FromRoute] Guid warehouseId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool exactBarcode = false,
        CancellationToken ct = default)
    {
        // استدعاء خدمة المخزون لجلب صفحة أرصدة التخزين المحددة مع تطبيق خيارات البحث
        var result = await _stockService.GetPagedStorgeStocksByWarehouseAsync(warehouseId, pageNumber, pageSize, searchTerm, exactBarcode, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع رصيد باركود محدد تفصيلياً في مخزن تخزين معين.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد لمخزن التخزين.</param>
    /// <param name="productBarcodeId">المعرف الفريد لباركود المنتج المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الرصيد الفعلي والحد الأدنى للباركود أو خطأ 404.</returns>
    [HttpGet("storge/{warehouseId:guid}/barcode/{productBarcodeId:guid}")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetStorgeStock([FromRoute] Guid warehouseId, [FromRoute] Guid productBarcodeId, CancellationToken ct)
    {
        // استدعاء خدمة المخزون للبحث عن رصيد باركود محدد داخل المخزن المعني
        var result = await _stockService.GetStorgeStockAsync(warehouseId, productBarcodeId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تعيين أو تحديث رصيد التخزين والحد الأدنى للمخزون لباركود معين يدوياً من قبل الإدارة.
    /// </summary>
    /// <param name="dto">بيانات تعيين رصيد التخزين تشمل المعرفات والكمية وحد الطلب.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الرصيد المحدثة بعد التعديل.</returns>
    [HttpPost("storge")]
    [HasPermission(Permissions.Stock.Adjust)]
    public async Task<IActionResult> SetStorgeStock([FromBody] SetStorgeStockDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المخزون لضبط وتعيين رصيد التخزين والحد الأدنى للباركود
        var result = await _stockService.SetStorgeStockAsync(dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تنبيهات وأرصدة المخزون المنخفضة (التي تقل عن الحد الأدنى للأمان) لمخازن التخزين.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع للفلترة (اختياري)، أو تركه فارغاً لجميع المستودعات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بتنبيهات النواقص لمخازن التخزين.</returns>
    [HttpGet("storge/low-stock")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetLowStorgeStockAlerts([FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
    {
        // استدعاء خدمة المخزون لجلب تنبيهات الباركودات التي بلغت مستوى النواقص في مخازن التخزين
        var result = await _stockService.GetLowStorgeStockAlertsAsync(warehouseId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    // ── Showroom Stock Endpoints (مخزون صالات العرض بالمنتج) ──

    /// <summary>
    /// استرجاع جميع الكميات والأرصدة المخزنية لصالة عرض محددة بالمعرف (مخزون مجمع بالمنتج).
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد لصالة العرض المستهدفة.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالأرصدة المخزنية لجميع المنتجات في صالة العرض المحددة.</returns>
    [HttpGet("showroom/{warehouseId:guid}")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetShowroomStocksByWarehouse([FromRoute] Guid warehouseId, CancellationToken ct)
    {
        // استدعاء خدمة المخزون لجلب كافة أرصدة صالة العرض المحددة
        var result = await _stockService.GetShowroomStocksByWarehouseAsync(warehouseId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع الكميات المخزنية لصالة عرض محددة مقسمة لصفحات مع دعم البحث النصي والترقيم.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد لصالة العرض.</param>
    /// <param name="pageNumber">رقم الصفحة المطلوبة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="searchTerm">نص البحث السريع في اسم المنتج أو كوده.</param>
    /// <param name="exactBarcode">تحديد ما إذا كان البحث بباركود دقيق.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على أرصدة منتجات صالة العرض وإجمالي السجلات.</returns>
    [HttpGet("showroom/{warehouseId:guid}/paged")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetPagedShowroomStocksByWarehouse(
        [FromRoute] Guid warehouseId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool exactBarcode = false,
        CancellationToken ct = default)
    {
        // استدعاء خدمة المخزون لجلب صفحة أرصدة صالة العرض المحددة مع تطبيق خيارات البحث
        var result = await _stockService.GetPagedShowroomStocksByWarehouseAsync(warehouseId, pageNumber, pageSize, searchTerm, exactBarcode, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع رصيد منتج محدد تفصيلياً في صالة عرض معينة.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد لصالة العرض.</param>
    /// <param name="productId">المعرف الفريد للمنتج المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الرصيد الفعلي والحد الأدنى للمنتج في الصالة المحددة.</returns>
    [HttpGet("showroom/{warehouseId:guid}/product/{productId:guid}")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetShowroomStock([FromRoute] Guid warehouseId, [FromRoute] Guid productId, CancellationToken ct)
    {
        // استدعاء خدمة المخزون للبحث عن رصيد منتج محدد داخل صالة العرض المعنية
        var result = await _stockService.GetShowroomStockAsync(warehouseId, productId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تعيين أو تحديث رصيد صالة العرض والحد الأدنى لمنتج معين يدوياً من قبل الإدارة.
    /// </summary>
    /// <param name="dto">بيانات تعيين رصيد صالة العرض تشمل معرف الصالة والمنتج والكمية وحد الطلب.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الرصيد المحدثة بعد التعديل.</returns>
    [HttpPost("showroom")]
    [HasPermission(Permissions.Stock.Adjust)]
    public async Task<IActionResult> SetShowroomStock([FromBody] SetShowroomStockDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المخزون لضبط وتعيين رصيد صالة العرض والحد الأدنى للمنتج
        var result = await _stockService.SetShowroomStockAsync(dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تنبيهات وأرصدة المخزون المنخفضة (التي تقل عن الحد الأدنى للأمان) لصالات العرض.
    /// </summary>
    /// <param name="warehouseId">معرف صالة العرض للفلترة (اختياري)، أو تركه فارغاً لجميع الصالات.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بتنبيهات النواقص لصالات العرض.</returns>
    [HttpGet("showroom/low-stock")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetLowShowroomStockAlerts([FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
    {
        // استدعاء خدمة المخزون لجلب تنبيهات المنتجات التي بلغت مستوى النواقص في صالات العرض
        var result = await _stockService.GetLowShowroomStockAlertsAsync(warehouseId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
