using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Services.Dashboard.Interfaces;

namespace RetalSystemAPI.Controllers.Dashboard;

/// <summary>
/// متحكم لوحة التحكم الإحصائية والتحليلات ومؤشرات الأداء الرئيسية (Dashboard &amp; Live Analytics).
/// يوفر نقاط النهاية المجمعة للإحصائيات المالية، وحركات المبيعات، ورادار نواقص المخزون، والأصناف الأكثر مبيعاً.
/// </summary>
[Authorize]
[HasPermission(Permissions.Dashboard.View)]
[Route("api/dashboard")]
public class DashboardController : BaseApiController
{
    /// <summary>
    /// خدمة لوحة التحكم وتجميع البيانات والتحليلات الإحصائية.
    /// </summary>
    private readonly IDashboardService _dashboardService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم لوحة التحكم مع حقن خدمة التحليلات الإحصائية.
    /// </summary>
    /// <param name="dashboardService">واجهة خدمة لوحة التحكم والتحليلات.</param>
    public DashboardController(IDashboardService dashboardService)
    {
        // إسناد خدمة لوحة التحكم المحقونة إلى الحقل الخاص
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// استرجاع الملخص الإحصائي الشامل للوحة التحكم (مؤشرات مالية، أعداد المؤسسة، المخطط البياني، النواقص، والأصناف الأكثر مبيعاً).
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة الاختيارية، أو تركه فارغاً لجلب بيانات المنشأة ككل.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>كائن ملخص المؤشرات الشاملة للوحة التحكم.</returns>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] Guid? branchId = null, CancellationToken ct = default)
    {
        // استدعاء خدمة لوحة التحكم لتجميع المؤشرات المالية والإحصائية للفرع المحدد أو المنشأة بالكامل
        var result = await _dashboardService.GetSummaryAsync(branchId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع بيانات مسار حركة المبيعات والأرباح اليومية لعدد محدد من الأيام السابقة.
    /// </summary>
    /// <param name="days">عدد الأيام المطلوب تتبع مسارها الزمني (الافتراضي 7 أيام).</param>
    /// <param name="branchId">معرف الفرع لتصفية مسار المبيعات الخاصة به (اختياري).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالنقاط البيانية اليومية للمبيعات والأرباح عبر الفترة المحددة.</returns>
    [HttpGet("sales-trend")]
    public async Task<IActionResult> GetSalesTrend([FromQuery] int days = 7, [FromQuery] Guid? branchId = null, CancellationToken ct = default)
    {
        // استدعاء خدمة لوحة التحكم لاحتساب واستخراج مسار المبيعات والأرباح للأيام المحددة
        var result = await _dashboardService.GetSalesTrendAsync(days, branchId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة رادار النواقص وتنبيهات الأصناف الحرجة التي قاربت أرصدتها على النفاد في صالات العرض والمخازن.
    /// </summary>
    /// <param name="branchId">معرف الفرع لتصفية تنبيهات النواقص الخاصة به (اختياري).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالأصناف التي وصلت لحد إعادة الطلب مصحوبة بالأرصدة الحالية وحدود الأمان.</returns>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockAlerts([FromQuery] Guid? branchId = null, CancellationToken ct = default)
    {
        // استدعاء خدمة لوحة التحكم لجلب تنبيهات الأصناف التي تقل أرصدتها عن حد إعادة الطلب
        var result = await _dashboardService.GetLowStockAlertsAsync(branchId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة الأصناف الأكثر مبيعاً والأعلى تحقيقاً للإيرادات في الفترة الأخيرة.
    /// </summary>
    /// <param name="count">عدد الأصناف المطلوبة في القائمة (الافتراضي 5 أصناف).</param>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالأصناف المتصدرة للمبيعات مرتبة تنازلياً حسب الكميات والمبالغ المحققة.</returns>
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int count = 5, [FromQuery] Guid? branchId = null, CancellationToken ct = default)
    {
        // استدعاء خدمة لوحة التحكم لاحتساب وترتيب الأصناف الأعلى مبيعاً
        var result = await _dashboardService.GetTopSellingProductsAsync(count, branchId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
