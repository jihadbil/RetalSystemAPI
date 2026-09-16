using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Models.DTOs.Dashboard;

/// <summary>
/// الكائن المجمع لبيانات ومؤشرات لوحة التحكم الشاملة.
/// </summary>
public class DashboardSummaryDto
{
    /// <summary>
    /// المؤشرات المالية والتشغيلية الفورية (KPIs).
    /// </summary>
    public DashboardKpiDto Kpis { get; set; } = new();

    /// <summary>
    /// الإحصائيات العددية للمؤسسة.
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
    /// رادار النواقص والأصناف الحرجة في المخزون.
    /// </summary>
    public List<LowStockItemDto> LowStockAlerts { get; set; } = new();

    /// <summary>
    /// قائمة آخر فواتير المبيعات الصادرة في النظام.
    /// </summary>
    public List<RecentInvoiceDto> RecentInvoices { get; set; } = new();
}

/// <summary>
/// بطاقات مؤشرات الأداء الحيوية والمالية.
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
/// نقطة بيانات في الرسم البياني اليومي.
/// </summary>
public class DailySalesPointDto
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public decimal SalesAmount { get; set; }
    public decimal ProfitAmount { get; set; }
    public int OrdersCount { get; set; }
}

/// <summary>
/// بيانات الصنف الأكثر مبيعاً.
/// </summary>
public class TopSellingProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal PercentageOfTotalSales { get; set; }
}

/// <summary>
/// عنصر في رادار النواقص والمخزون الحرج.
/// </summary>
public class LowStockItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Status { get; set; } = "منخفض"; // "نفد" أو "حرج" أو "منخفض"
}

/// <summary>
/// ملخص فاتورة حديثة.
/// </summary>
public class RecentInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
