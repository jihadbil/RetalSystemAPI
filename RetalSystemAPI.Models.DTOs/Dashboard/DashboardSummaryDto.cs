using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Models.DTOs.Dashboard;

/// <summary>
/// الكائن المجمع لبيانات ومؤشرات لوحة التحكم الشاملة (Dashboard Summary DTO).
/// يجمع مؤشرات الأداء الحيوية، الإحصائيات العددية، الرسوم البيانية، وقوائم التنبيهات والأصناف الأكثر مبيعاً.
/// </summary>
public class DashboardSummaryDto
{
    /// <summary>
    /// المؤشرات المالية والتشغيلية الفورية (KPIs).
    /// </summary>
    public DashboardKpiDto Kpis { get; set; } = new();

    /// <summary>
    /// الإحصائيات العددية للمؤسسة وسجلات الكيانات.
    /// </summary>
    public DashboardCountsDto Counts { get; set; } = new();

    /// <summary>
    /// مسار حركة المبيعات اليومية لآخر 7 أو 30 يوماً للرسم البياني.
    /// </summary>
    public List<DailySalesPointDto> DailySalesTrend { get; set; } = new();

    /// <summary>
    /// قائمة الأصناف الأكثر مبيعاً والأعلى إيراداً.
    /// </summary>
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();

    /// <summary>
    /// رادار النواقص والأصناف الحرجة في المخزون المتجاوزة لحد الطلب الأدنى.
    /// </summary>
    public List<LowStockItemDto> LowStockAlerts { get; set; } = new();

    /// <summary>
    /// قائمة آخر فواتير المبيعات الصادرة في النظام.
    /// </summary>
    public List<RecentInvoiceDto> RecentInvoices { get; set; } = new();
}

/// <summary>
/// بطاقات مؤشرات الأداء الحيوية والمالية الفورية (KPIs).
/// </summary>
public class DashboardKpiDto
{
    /// <summary>إجمالي مبيعات اليوم</summary>
    public decimal TodaySales { get; set; }

    /// <summary>إجمالي مبيعات الأمس (للمقارنة وحساب النمو)</summary>
    public decimal YesterdaySales { get; set; }

    /// <summary>نسبة النمو في المبيعات مقارنة بالأمس</summary>
    public decimal SalesGrowthPercentage { get; set; }

    /// <summary>عدد فواتير المبيعات المنفذة اليوم</summary>
    public int TodayOrdersCount { get; set; }

    /// <summary>متوسط قيمة الفاتورة اليوم</summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>صافي أرباح مبيعات اليوم التقديرية (الإيراد - التكلفة)</summary>
    public decimal TodayProfit { get; set; }

    /// <summary>إجمالي فواتير المشتريات المباشرة اليوم</summary>
    public decimal TodayPurchases { get; set; }

    /// <summary>إجمالي مبيعات الشهر الحالي</summary>
    public decimal ThisMonthSales { get; set; }

    /// <summary>إجمالي مشتريات الشهر الحالي</summary>
    public decimal ThisMonthPurchases { get; set; }

    /// <summary>إجمالي ديون العملاء المتبقية غير المسددة</summary>
    public decimal TotalReceivables { get; set; }

    /// <summary>إجمالي مستحقات الموردين المتبقية غير المسددة</summary>
    public decimal TotalPayables { get; set; }
}

/// <summary>
/// أعداد السجلات والكيانات الأساسية في النظام.
/// </summary>
public class DashboardCountsDto
{
    /// <summary>إجمالي عدد الأصناف والمنتجات</summary>
    public int ProductsCount { get; set; }

    /// <summary>إجمالي عدد التصنيفات</summary>
    public int CategoriesCount { get; set; }

    /// <summary>إجمالي عدد الفروع</summary>
    public int BranchesCount { get; set; }

    /// <summary>إجمالي عدد المخازن وصالات العرض</summary>
    public int WarehousesCount { get; set; }

    /// <summary>إجمالي عدد العملاء</summary>
    public int CustomersCount { get; set; }

    /// <summary>إجمالي عدد الموردين</summary>
    public int SuppliersCount { get; set; }

    /// <summary>إجمالي عدد المستأجرين (في حال تعدد المنظمات)</summary>
    public int TenantsCount { get; set; }
}

/// <summary>
/// نقطة بيانات في مسار الرسم البياني اليومي للمبيعات والأرباح.
/// </summary>
public class DailySalesPointDto
{
    /// <summary>تاريخ اليوم</summary>
    public DateTime Date { get; set; }

    /// <summary>اسم اليوم (مثال: السبت، الأحد)</summary>
    public string DayName { get; set; } = string.Empty;

    /// <summary>إجمالي المبيعات المحققة في هذا اليوم</summary>
    public decimal SalesAmount { get; set; }

    /// <summary>إجمالي صافي الأرباح المحققة في هذا اليوم</summary>
    public decimal ProfitAmount { get; set; }

    /// <summary>عدد الفواتير المنفذة في هذا اليوم</summary>
    public int OrdersCount { get; set; }
}

/// <summary>
/// بيانات وإحصائيات الصنف الأكثر مبيعاً في النظام.
/// </summary>
public class TopSellingProductDto
{
    /// <summary>المعرف الفريد للمنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>اسم الفئة أو التصنيف</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>الكمية الإجمالية المباعة</summary>
    public int QuantitySold { get; set; }

    /// <summary>إجمالي قيمة مبيعات هذا الصنف</summary>
    public decimal TotalSales { get; set; }

    /// <summary>إجمالي صافي الأرباح المحققة من الصنف</summary>
    public decimal TotalProfit { get; set; }

    /// <summary>النسبة المئوية لمساهمة الصنف في إجمالي المبيعات</summary>
    public decimal PercentageOfTotalSales { get; set; }
}

/// <summary>
/// عنصر في رادار تنبيهات النواقص والمخزون الحرج.
/// </summary>
public class LowStockItemDto
{
    /// <summary>المعرف الفريد للمنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>اسم التصنيف</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>اسم المستودع أو الفرع المحتوي على الرصيد</summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>الرصيد الفعلي الحالي المتوفر بالمستودع</summary>
    public int CurrentQuantity { get; set; }

    /// <summary>الحد الأدنى المطلوب للمخزون (حد الطلب)</summary>
    public int MinStockLevel { get; set; }

    /// <summary>سعر تكلفة الصنف</summary>
    public decimal CostPrice { get; set; }

    /// <summary>سعر بيع الصنف</summary>
    public decimal SalePrice { get; set; }

    /// <summary>حالة العجز (نفد / حرج / منخفض)</summary>
    public string Status { get; set; } = "منخفض";
}

/// <summary>
/// ملخص بيانات فاتورة مبيعات حديثة في لوحة التحكم.
/// </summary>
public class RecentInvoiceDto
{
    /// <summary>المعرف الفريد للفاتورة</summary>
    public Guid Id { get; set; }

    /// <summary>رقم الفاتورة التسلسلي</summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>تاريخ وتوقيت إصدار الفاتورة</summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>اسم العميل إن وجد</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>اسم الفرع المصدر للفاتورة</summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>إجمالي قيمة الفاتورة</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المدفوع</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>المبلغ المتبقي</summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>طريقة الدفع (نقدي / آجل / تحويل بنكي)</summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>حالة الفاتورة (مدفوعة / معلقة / مسودة)</summary>
    public string Status { get; set; } = string.Empty;
}
