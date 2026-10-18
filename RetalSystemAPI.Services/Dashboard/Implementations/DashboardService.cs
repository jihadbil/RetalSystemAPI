using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models.DTOs.Dashboard;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Dashboard.Interfaces;

namespace RetalSystemAPI.Services.Dashboard.Implementations;

/// <summary>
/// تنفيذ خدمة لوحة التحكم واستخراج المؤشرات الإحصائية والمالية والتشغيلية المجمعة.
/// </summary>
public class DashboardService : IDashboardService
{
    // سياق قاعدة البيانات لتنفيذ الاستعلامات والتجميعات المباشرة عالية الكفاءة
    private readonly AppDbContext _dbContext;

    // خدمة التخزين المؤقت في الذاكرة لتفادي تكرار مسح الجداول الكبيرة للعدادات الثابتة نسبياً
    private readonly IMemoryCache _cache;

    // مؤشرات المبيعات بعد الخصومات والمرتجعات وبتوقيت المستأجر.
    private readonly DashboardSalesMetrics _metrics;

    /// <summary>
    /// تهيئة خدمة لوحة التحكم مع حقن سياق قاعدة البيانات والتخزين المؤقت.
    /// </summary>
    /// <param name="dbContext">سياق قاعدة البيانات الرئيسي</param>
    /// <param name="cache">خدمة التخزين المؤقت في الذاكرة</param>
    public DashboardService(AppDbContext dbContext, IMemoryCache cache, TimeProvider? timeProvider = null)
    {
        // تعيين مرجع سياق قاعدة البيانات
        _dbContext = dbContext;
        // تعيين مرجع التخزين المؤقت
        _cache = cache;
        _metrics = new DashboardSalesMetrics(dbContext, timeProvider ?? TimeProvider.System);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<DashboardSummaryDto>> GetSummaryAsync(Guid? branchId = null, CancellationToken ct = default)
    {
        // إنشاء كائن نتيجة الملخص الشامل للوحة التحكم
        var summary = new DashboardSummaryDto();

        var calendar = await _metrics.GetCalendarAsync(ct);
        var todayStart = calendar.Utc(calendar.Today);
        var tomorrowStart = calendar.Utc(calendar.Today.AddDays(1));
        var localMonthStart = new DateTime(calendar.Today.Year, calendar.Today.Month, 1);
        var monthStart = calendar.Utc(localMonthStart);
        var nextMonthStart = calendar.Utc(localMonthStart.AddMonths(1));
        // 1. إعداد استعلام فواتير المبيعات الأساسي مع استبعاد الملغاة والمردودة
        var salesQuery = _dbContext.SalesInvoices
            .AsNoTracking()
            .Where(i => i.Status != InvoiceStatus.Draft && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided);

        // إعداد استعلام فواتير المشتريات الأساسي مع استبعاد الملغاة والمردودة
        var purchasesQuery = _dbContext.PurchaseInvoices
            .AsNoTracking()
            .Where(p => p.Status != InvoiceStatus.Draft && p.Status != InvoiceStatus.Cancelled && p.Status != InvoiceStatus.Voided);

        // تطبيق فلترة الفرع إن تم تحديده
        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            // فلترة المبيعات بحسب الفرع
            salesQuery = salesQuery.Where(i => i.BranchId == branchId.Value);

            // فلترة المشتريات بحسب الفرع
            purchasesQuery = purchasesQuery.Where(p => p.BranchId == branchId.Value);
        }

        var trend = await _metrics.DaysAsync(7, branchId, calendar, ct);
        var today = trend[^1];
        decimal todaySales = today.SalesAmount;
        decimal todayProfit = today.ProfitAmount;
        int todayOrdersCount = today.OrdersCount;
        decimal yesterdaySales = trend[^2].SalesAmount;
        decimal averageOrderValue = todayOrdersCount > 0 ? Math.Round(todaySales / todayOrdersCount, 2) : 0;
        decimal growthPercentage = yesterdaySales > 0
            ? Math.Round((todaySales - yesterdaySales) / yesterdaySales * 100, 1)
            : todaySales > 0 ? 100 : 0;
        decimal thisMonthSales = await _metrics.NetSalesAsync(monthStart, nextMonthStart, branchId, ct);
        // الحساب الموحد يشمل الرصيد الافتتاحي والسداد والمرتجعات.
        // على مستوى الفرع نحسب صافي قيوده؛ الأرصدة الافتتاحية غير المنسوبة لفرع تظهر في الإجمالي فقط.
        decimal totalReceivables = branchId.HasValue && branchId.Value != Guid.Empty
            ? await _dbContext.CustomerTransactions.AsNoTracking().Where(t => t.BranchId == branchId.Value)
                .GroupBy(t => t.CustomerId).Select(g => g.Sum(t => t.DebitAmount - t.CreditAmount))
                .Where(balance => balance > 0).SumAsync(ct)
            : await _dbContext.Customers.AsNoTracking().Where(c => c.CurrentBalance > 0).SumAsync(c => c.CurrentBalance, ct);
        // حساب إجمالي مشتريات اليوم
        decimal todayPurchases = await purchasesQuery
            .Where(p => p.InvoiceDate >= todayStart && p.InvoiceDate < tomorrowStart)
            .SumAsync(p => p.TotalAmount, ct);

        // حساب إجمالي مشتريات الشهر الحالي
        decimal thisMonthPurchases = await purchasesQuery
            .Where(p => p.InvoiceDate >= monthStart && p.InvoiceDate < nextMonthStart)
            .SumAsync(p => p.TotalAmount, ct);

        decimal totalPayables = branchId.HasValue && branchId.Value != Guid.Empty
            ? await _dbContext.SupplierTransactions.AsNoTracking().Where(t => t.BranchId == branchId.Value)
                .GroupBy(t => t.SupplierId).Select(g => g.Sum(t => t.CreditAmount - t.DebitAmount))
                .Where(balance => balance > 0).SumAsync(ct)
            : await _dbContext.Suppliers.AsNoTracking().Where(s => s.CurrentBalance > 0).SumAsync(s => s.CurrentBalance, ct);
        // تعبئة كائن مؤشرات الأداء الرئيسية KPIs
        summary.Kpis = new DashboardKpiDto
        {
            TodaySales = todaySales,
            YesterdaySales = yesterdaySales,
            SalesGrowthPercentage = growthPercentage,
            TodayOrdersCount = todayOrdersCount,
            AverageOrderValue = averageOrderValue,
            TodayProfit = todayProfit,
            TodayPurchases = todayPurchases,
            ThisMonthSales = thisMonthSales,
            ThisMonthPurchases = thisMonthPurchases,
            TotalReceivables = totalReceivables,
            TotalPayables = totalPayables
        };

        // 3. استعلام أعداد الكيانات والمؤسسة مع تخزين مؤقت خفيف (60 ثانية) لتفادي تكرار مسح الجداول الكبيرة
        string cacheKey = $"DashboardCounts_{_dbContext.CurrentTenantId}";
        if (!_cache.TryGetValue(cacheKey, out DashboardCountsDto? cachedCounts) || cachedCounts == null)
        {
            cachedCounts = new DashboardCountsDto
            {
                // إجمالي عدد المنتجات
                ProductsCount = await _dbContext.Products.CountAsync(ct),
                // إجمالي عدد التصنيفات
                CategoriesCount = await _dbContext.Categories.CountAsync(ct),
                // إجمالي عدد الفروع
                BranchesCount = await _dbContext.Branches.CountAsync(ct),
                // إجمالي عدد المستودعات
                WarehousesCount = await _dbContext.Warehouses.CountAsync(ct),
                // إجمالي عدد العملاء
                CustomersCount = await _dbContext.Customers.CountAsync(ct),
                // إجمالي عدد الموردين
                SuppliersCount = await _dbContext.Suppliers.CountAsync(ct),
                // إجمالي عدد المستأجرين
                TenantsCount = await _dbContext.Tenants.CountAsync(ct)
            };

            _cache.Set(cacheKey, cachedCounts, TimeSpan.FromSeconds(60));
        }
        summary.Counts = cachedCounts;

        // 4. استخراج مسار حركة المبيعات لآخر 7 أيام
        var trendResult = trend;
        summary.DailySalesTrend = trendResult;

        // 5. استخراج المنتجات الأكثر مبيعاً
        var topProductsResult = await GetTopSellingProductsInternalAsync(5, branchId, ct);
        summary.TopSellingProducts = topProductsResult;

        // 6. استخراج رادار النواقص والمخزون الحرج
        var lowStockResult = await GetLowStockAlertsInternalAsync(branchId, ct);
        summary.LowStockAlerts = lowStockResult;

        // 7. جلب آخر 7 فواتير مبيعات منفذة
        var rawRecentInvoices = await salesQuery
            .OrderByDescending(i => i.InvoiceDate)
            .Take(7)
            .Include(i => i.Customer)
            .Include(i => i.Branch)
            .ToListAsync(ct);

        // تحويل الفواتير الأخيرة إلى نماذج العرض
        summary.RecentInvoices = rawRecentInvoices.Select(i => new RecentInvoiceDto
        {
            // معرف الفاتورة
            Id = i.Id,
            // رقم الفاتورة
            InvoiceNumber = i.InvoiceNumber,
            // تاريخ الفاتورة
            InvoiceDate = i.InvoiceDate,
            // اسم العميل أو الزبون النقدي
            CustomerName = i.Customer != null ? i.Customer.Name : "زبون نقدي عام",
            // اسم الفرع
            BranchName = i.Branch != null ? i.Branch.Name : string.Empty,
            // إجمالي المبلغ
            TotalAmount = i.TotalAmount,
            // المبلغ المدفوع
            PaidAmount = i.PaidAmount,
            // المبلغ المتبقي
            RemainingAmount = i.RemainingAmount,
            // طريقة السداد مترجمة
            PaymentMethod = i.PaymentMethod switch
            {
                PaymentMethod.Cash => "نقدي",
                PaymentMethod.CreditCard => "بطاقة مصرفية",
                PaymentMethod.BankTransfer => "تحويل بنكي",
                PaymentMethod.Credit => "آجل",
                PaymentMethod.Cheque => "صك",
                _ => i.PaymentMethod.ToString()
            },
            // حالة الفاتورة مترجمة
            Status = i.Status switch
            {
                InvoiceStatus.Paid => "مدفوعة",
                InvoiceStatus.PartiallyPaid => "سداد جزئي",
                InvoiceStatus.Pending => "معلقة",
                InvoiceStatus.Draft => "مسودة",
                _ => i.Status.ToString()
            }
        }).ToList();

        // إرجاع النتيجة الناجحة للملخص الشامل
        return ServiceResult<DashboardSummaryDto>.Success(summary);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<DailySalesPointDto>>> GetSalesTrendAsync(int days = 7, Guid? branchId = null, CancellationToken ct = default)
    {
        // استدعاء الدالة الداخلية لحساب مسار المبيعات
        var list = await GetSalesTrendInternalAsync(days, branchId, ct);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<DailySalesPointDto>>.Success(list);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<LowStockItemDto>>> GetLowStockAlertsAsync(Guid? branchId = null, CancellationToken ct = default)
    {
        // استدعاء الدالة الداخلية لجلب تنبيهات النواقص
        var list = await GetLowStockAlertsInternalAsync(branchId, ct);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<LowStockItemDto>>.Success(list);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<TopSellingProductDto>>> GetTopSellingProductsAsync(int count = 5, Guid? branchId = null, CancellationToken ct = default)
    {
        // استدعاء الدالة الداخلية لجلب الأصناف الأكثر طلباً
        var list = await GetTopSellingProductsInternalAsync(count, branchId, ct);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<TopSellingProductDto>>.Success(list);
    }

    // ── دوال داخلية مساعدة ──────────────────────────────────────────

    /// <summary>
    /// حساب مسار المبيعات والأرباح اليومية عبر استعلامات تجميعية SQL عالية الأداء.
    /// </summary>
    /// <param name="days">عدد الأيام</param>
    /// <param name="branchId">معرف الفرع للفلترة</param>
    /// <param name="ct">رمز الإلغاء</param>
    private async Task<List<DailySalesPointDto>> GetSalesTrendInternalAsync(int days, Guid? branchId, CancellationToken ct)
    {
        return await _metrics.DaysAsync(days, branchId, await _metrics.GetCalendarAsync(ct), ct);
    }

    /// <summary>
    /// استخراج قائمة المنتجات الأكثر مبيعاً والأعلى ربحاً بحسب إجمالي قيمة المبيعات.
    /// </summary>
    /// <param name="count">عدد المنتجات المطلوب إرجاعها</param>
    /// <param name="branchId">معرف الفرع للفلترة</param>
    /// <param name="ct">رمز الإلغاء</param>
    private async Task<List<TopSellingProductDto>> GetTopSellingProductsInternalAsync(int count, Guid? branchId, CancellationToken ct)
    {
        return await _metrics.ProductsAsync(count, branchId, ct);
    }

    /// <summary>
    /// استخراج الأصناف الحرجة التي اقتربت من النفاد أو تجاوزت الحد الأدنى للمخزون في الصالات والمخازن.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة</param>
    /// <param name="ct">رمز الإلغاء</param>
    private async Task<List<LowStockItemDto>> GetLowStockAlertsInternalAsync(Guid? branchId, CancellationToken ct)
    {
        var showroomQuery = _dbContext.ShowroomStocks.AsNoTracking();
        var storgeQuery = _dbContext.StorgeStocks.AsNoTracking();

        // تطبيق فلترة الفرع إن تم تحديده
        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            showroomQuery = showroomQuery.Where(s => s.Warehouse.BranchId == branchId.Value);
            storgeQuery = storgeQuery.Where(s => s.Warehouse.BranchId == branchId.Value);
        }

        // جلب أعلى 10 أصناف منخفضة من صالة العرض بإسقاط SQL مباشر وخفيف
        var showroomRecords = await showroomQuery
            .Where(s => s.Quantity <= s.MinStockLevel || s.Quantity <= 5)
            .OrderBy(s => s.Quantity)
            .Take(10)
            .Select(s => new LowStockItemDto
            {
                ProductId = s.ProductId,
                ProductName = s.Product.Name,
                CategoryName = s.Product.Category != null ? s.Product.Category.Name : "عام",
                WarehouseName = s.Warehouse.Name + " (صالة)",
                CurrentQuantity = s.Quantity,
                MinStockLevel = s.MinStockLevel,
                CostPrice = s.Product.CostPrice,
                SalePrice = s.Product.SalePrice,
                Status = s.Quantity <= 0 ? "نفد" : (s.Quantity <= Math.Max(1, s.MinStockLevel / 2) ? "حرج" : "منخفض")
            })
            .ToListAsync(ct);

        // جلب أعلى 10 أصناف منخفضة من المستودعات بإسقاط SQL مباشر وخفيف
        var storgeRecords = await storgeQuery
            .Where(s => s.ProductBarcode != null && s.ProductBarcode.Product != null && (s.Quantity <= s.MinStockLevel || s.Quantity <= 5))
            .OrderBy(s => s.Quantity)
            .Take(10)
            .Select(s => new LowStockItemDto
            {
                ProductId = s.ProductBarcode!.ProductId,
                ProductName = s.ProductBarcode.Product!.Name + " (" + s.ProductBarcode.Title + ")",
                CategoryName = s.ProductBarcode.Product.Category != null ? s.ProductBarcode.Product.Category.Name : "عام",
                WarehouseName = s.Warehouse.Name + " (مخزن)",
                CurrentQuantity = s.Quantity,
                MinStockLevel = s.MinStockLevel,
                CostPrice = s.ProductBarcode.Product.CostPrice,
                SalePrice = s.ProductBarcode.Product.SalePrice,
                Status = s.Quantity <= 0 ? "نفد" : (s.Quantity <= Math.Max(1, s.MinStockLevel / 2) ? "حرج" : "منخفض")
            })
            .ToListAsync(ct);

        // دمج النتائج وترتيبها تصاعدياً واقتطاع أول 10 عناصر فقط
        return showroomRecords
            .Concat(storgeRecords)
            .OrderBy(x => x.CurrentQuantity)
            .Take(10)
            .ToList();
    }
}
