using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Services.Dashboard.Interfaces;

using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;

namespace RetalSystemAPI.Controllers.Dashboard;

/// <summary>
/// متحكم لوحة التحكم الإحصائية والتحليلات ومؤشرات الأداء الرئيسية (Dashboard & Live Analytics).
/// </summary>
[Authorize]
[HasPermission(Permissions.Dashboard.View)]
[Route("api/dashboard")]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// الحصول على الملخص الإحصائي الشامل للوحة التحكم (مؤشرات مالية، أعداد المؤسسة، المخطط البياني، النواقص، والأصناف الأكثر مبيعاً).
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] Guid? branchId = null, CancellationToken ct = default)
    {
        var result = await _dashboardService.GetSummaryAsync(branchId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على بيانات مسار حركة المبيعات والأرباح اليومية لعدد محدد من الأيام.
    /// </summary>
    /// <param name="days">عدد الأيام المطلوبة (الافتراضي 7 أيام)</param>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    [HttpGet("sales-trend")]
    public async Task<IActionResult> GetSalesTrend([FromQuery] int days = 7, [FromQuery] Guid? branchId = null, CancellationToken ct = default)
    {
        var result = await _dashboardService.GetSalesTrendAsync(days, branchId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة رادار النواقص والأصناف الحرجة في صالات العرض والمخازن.
    /// </summary>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockAlerts([FromQuery] Guid? branchId = null, CancellationToken ct = default)
    {
        var result = await _dashboardService.GetLowStockAlertsAsync(branchId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة الأصناف الأكثر مبيعاً والأعلى تحقيقاً للإيرادات.
    /// </summary>
    /// <param name="count">عدد الأصناف المطلوبة (الافتراضي 5)</param>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int count = 5, [FromQuery] Guid? branchId = null, CancellationToken ct = default)
    {
        var result = await _dashboardService.GetTopSellingProductsAsync(count, branchId, ct);
        return ToActionResult(result);
    }
}
