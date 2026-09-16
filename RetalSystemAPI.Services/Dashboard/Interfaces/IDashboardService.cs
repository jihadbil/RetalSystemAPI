using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Dashboard;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Dashboard.Interfaces;

/// <summary>
/// واجهة خدمة لوحة التحكم واستخراج المؤشرات الإحصائية والمالية والتشغيلية.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// جلب ملخص شامل لجميع المؤشرات والإحصائيات ورسوم البيع والنواقص والفواتير الأخيرة.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري، في حال عدم التمرير يجلب لجميع الفروع التابعة للمستأجر)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة كائن الملخص الشامل</returns>
    Task<ServiceResult<DashboardSummaryDto>> GetSummaryAsync(Guid? branchId = null, CancellationToken ct = default);

    /// <summary>
    /// جلب مسار حركة المبيعات اليومية لعدد محدد من الأيام السابقة.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<DailySalesPointDto>>> GetSalesTrendAsync(int days = 7, Guid? branchId = null, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة النواقص المخزنية والأصناف الحرجة.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<LowStockItemDto>>> GetLowStockAlertsAsync(Guid? branchId = null, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة الأصناف الأكثر مبيعاً وتحقيقاً للإيرادات.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<TopSellingProductDto>>> GetTopSellingProductsAsync(int count = 5, Guid? branchId = null, CancellationToken ct = default);
}
