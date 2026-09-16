using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    private readonly AppDbContext _dbContext;
    private static readonly CultureInfo ArabicCulture = new CultureInfo("ar-SA");

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<DashboardSummaryDto>> GetSummaryAsync(Guid? branchId = null, CancellationToken ct = default)
    {
        var summary = new DashboardSummaryDto();

        var localNow = DateTime.Now;
        var todayStart = localNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var yesterdayStart = todayStart.AddDays(-1);
        var monthStart = new DateTime(localNow.Year, localNow.Month, 1, 0, 0, 0);

        // 1. استعلام الفواتير الأساسية
        var salesQuery = _dbContext.SalesInvoices
            .AsNoTracking()
            .Where(i => i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided);

        var purchasesQuery = _dbContext.PurchaseInvoices
            .AsNoTracking()
            .Where(p => p.Status != InvoiceStatus.Cancelled && p.Status != InvoiceStatus.Voided);

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            salesQuery = salesQuery.Where(i => i.BranchId == branchId.Value);
            purchasesQuery = purchasesQuery.Where(p => p.BranchId == branchId.Value);
        }

        // 2. مؤشرات مبيعات وأرباح اليوم — تجميع داخل SQL دون تحميل الفواتير وبنودها في الذاكرة
        var todayQuery = salesQuery
            .Where(i => i.InvoiceDate >= todayStart && i.InvoiceDate < tomorrowStart);

        decimal todaySales = await todayQuery.SumAsync(i => i.TotalAmount, ct);
        int todayOrdersCount = await todayQuery.CountAsync(ct);
        decimal todayProfit = await todayQuery
            .SelectMany(i => i.Items)
            .SumAsync(item => item.LineTotal - (item.Quantity * item.UnitCost), ct);
        decimal averageOrderValue = todayOrdersCount > 0 ? Math.Round(todaySales / todayOrdersCount, 2) : 0;

        // مبيعات الأمس ونسبة النمو
        decimal yesterdaySales = await salesQuery
            .Where(i => i.InvoiceDate >= yesterdayStart && i.InvoiceDate < todayStart)
            .SumAsync(i => i.TotalAmount, ct);

        decimal growthPercentage = 0;
        if (yesterdaySales > 0)
        {
            growthPercentage = Math.Round(((todaySales - yesterdaySales) / yesterdaySales) * 100, 1);
        }
        else if (todaySales > 0)
        {
            growthPercentage = 100;
        }

        // مبيعات ومشتريات الشهر والديون
        decimal thisMonthSales = await salesQuery
            .Where(i => i.InvoiceDate >= monthStart)
            .SumAsync(i => i.TotalAmount, ct);

        decimal totalReceivables = await salesQuery
            .Where(i => i.RemainingAmount > 0)
            .SumAsync(i => i.RemainingAmount, ct);

        decimal todayPurchases = await purchasesQuery
            .Where(p => p.InvoiceDate >= todayStart && p.InvoiceDate < tomorrowStart)
            .SumAsync(p => p.TotalAmount, ct);

        decimal thisMonthPurchases = await purchasesQuery
            .Where(p => p.InvoiceDate >= monthStart)
            .SumAsync(p => p.TotalAmount, ct);

        decimal totalPayables = await purchasesQuery
            .Where(p => p.RemainingAmount > 0)
            .SumAsync(p => p.RemainingAmount, ct);

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

        // 3. أعداد الكيانات والمؤسسة
        summary.Counts = new DashboardCountsDto
        {
            ProductsCount = await _dbContext.Products.CountAsync(ct),
            CategoriesCount = await _dbContext.Categories.CountAsync(ct),
            BranchesCount = await _dbContext.Branches.CountAsync(ct),
            WarehousesCount = await _dbContext.Warehouses.CountAsync(ct),
            CustomersCount = await _dbContext.Customers.CountAsync(ct),
            SuppliersCount = await _dbContext.Suppliers.CountAsync(ct),
            TenantsCount = await _dbContext.Tenants.CountAsync(ct)
        };

        // 4. مسار حركة المبيعات لآخر 7 أيام
        var trendResult = await GetSalesTrendInternalAsync(7, branchId, ct);
        summary.DailySalesTrend = trendResult;

        // 5. الأصناف الأكثر مبيعاً
        var topProductsResult = await GetTopSellingProductsInternalAsync(5, branchId, ct);
        summary.TopSellingProducts = topProductsResult;

        // 6. رادار النواقص والمخزون الحرج
        var lowStockResult = await GetLowStockAlertsInternalAsync(branchId, ct);
        summary.LowStockAlerts = lowStockResult;

        // 7. آخر الفواتير المنفذة
        var rawRecentInvoices = await salesQuery
            .OrderByDescending(i => i.InvoiceDate)
            .Take(7)
            .Include(i => i.Customer)
            .Include(i => i.Branch)
            .ToListAsync(ct);

        summary.RecentInvoices = rawRecentInvoices.Select(i => new RecentInvoiceDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            InvoiceDate = i.InvoiceDate,
            CustomerName = i.Customer != null ? i.Customer.Name : "زبون نقدي عام",
            BranchName = i.Branch != null ? i.Branch.Name : string.Empty,
            TotalAmount = i.TotalAmount,
            PaidAmount = i.PaidAmount,
            RemainingAmount = i.RemainingAmount,
            PaymentMethod = i.PaymentMethod switch
            {
                PaymentMethod.Cash => "نقدي",
                PaymentMethod.CreditCard => "بطاقة مصرفية",
                PaymentMethod.BankTransfer => "تحويل بنكي",
                PaymentMethod.Credit => "آجل",
                PaymentMethod.Cheque => "صك",
                _ => i.PaymentMethod.ToString()
            },
            Status = i.Status switch
            {
                InvoiceStatus.Paid => "مدفوعة",
                InvoiceStatus.PartiallyPaid => "سداد جزئي",
                InvoiceStatus.Pending => "معلقة",
                InvoiceStatus.Draft => "مسودة",
                _ => i.Status.ToString()
            }
        }).ToList();

        return ServiceResult<DashboardSummaryDto>.Success(summary);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<DailySalesPointDto>>> GetSalesTrendAsync(int days = 7, Guid? branchId = null, CancellationToken ct = default)
    {
        var list = await GetSalesTrendInternalAsync(days, branchId, ct);
        return ServiceResult<IReadOnlyList<DailySalesPointDto>>.Success(list);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<LowStockItemDto>>> GetLowStockAlertsAsync(Guid? branchId = null, CancellationToken ct = default)
    {
        var list = await GetLowStockAlertsInternalAsync(branchId, ct);
        return ServiceResult<IReadOnlyList<LowStockItemDto>>.Success(list);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<TopSellingProductDto>>> GetTopSellingProductsAsync(int count = 5, Guid? branchId = null, CancellationToken ct = default)
    {
        var list = await GetTopSellingProductsInternalAsync(count, branchId, ct);
        return ServiceResult<IReadOnlyList<TopSellingProductDto>>.Success(list);
    }

    // ── دوال داخلية مساعدة ──────────────────────────────────────────

    private async Task<List<DailySalesPointDto>> GetSalesTrendInternalAsync(int days, Guid? branchId, CancellationToken ct)
    {
        var localNow = DateTime.Now;
        var startDate = localNow.Date.AddDays(-(days - 1));

        var query = _dbContext.SalesInvoices
            .AsNoTracking()
            .Where(i => i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided && i.InvoiceDate >= startDate);

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            query = query.Where(i => i.BranchId == branchId.Value);
        }

        // تجميع المبيعات وعدد الفواتير لكل يوم داخل SQL — رقم اليوم = عدد حدود الأيام المنقضية منذ بداية النطاق
        // (يطابق تماماً النافذة [currentDay, nextDay) التي كان يجري حسابها سابقاً في الذاكرة)
        var salesByDay = await query
            .GroupBy(i => EF.Functions.DateDiffDay(startDate, i.InvoiceDate))
            .Select(g => new
            {
                DayIndex = g.Key,
                Sales = g.Sum(x => x.TotalAmount),
                Orders = g.Count()
            })
            .ToListAsync(ct);

        // تجميع الأرباح من بنود الفواتير لكل يوم داخل SQL (JOIN داخلي ثم SUM)
        var profitByDay = await query
            .SelectMany(i => i.Items, (inv, item) => new
            {
                DayIndex = EF.Functions.DateDiffDay(startDate, inv.InvoiceDate),
                Profit = item.LineTotal - (item.Quantity * item.UnitCost)
            })
            .GroupBy(x => x.DayIndex)
            .Select(g => new
            {
                DayIndex = g.Key,
                Profit = g.Sum(x => x.Profit)
            })
            .ToListAsync(ct);

        var salesDict = salesByDay.ToDictionary(x => x.DayIndex);
        var ordersDict = salesByDay.ToDictionary(x => x.DayIndex, x => x.Orders);
        var profitDict = profitByDay.ToDictionary(x => x.DayIndex, x => x.Profit);

        var result = new List<DailySalesPointDto>();

        for (int i = 0; i < days; i++)
        {
            var currentDay = startDate.AddDays(i);

            salesDict.TryGetValue(i, out var daySales);
            ordersDict.TryGetValue(i, out var dayOrders);
            profitDict.TryGetValue(i, out var dayProfit);

            string arabicDayName = currentDay.ToString("dddd", ArabicCulture);

            result.Add(new DailySalesPointDto
            {
                Date = currentDay,
                DayName = arabicDayName,
                SalesAmount = daySales?.Sales ?? 0,
                ProfitAmount = dayProfit,
                OrdersCount = dayOrders
            });
        }

        return result;
    }

    private async Task<List<TopSellingProductDto>> GetTopSellingProductsInternalAsync(int count, Guid? branchId, CancellationToken ct)
    {
        var itemsQuery = _dbContext.SalesInvoiceItems
            .AsNoTracking()
            .Where(item => item.SalesInvoice.Status != InvoiceStatus.Cancelled && item.SalesInvoice.Status != InvoiceStatus.Voided);

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            itemsQuery = itemsQuery.Where(item => item.SalesInvoice.BranchId == branchId.Value);
        }

        var topGrouped = await itemsQuery
            .GroupBy(item => new
            {
                item.ProductId,
                ProductName = item.Product.Name,
                CategoryName = item.Product.Category != null ? item.Product.Category.Name : "عام"
            })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.CategoryName,
                QuantitySold = g.Sum(x => x.Quantity),
                TotalSales = g.Sum(x => x.LineTotal),
                TotalProfit = g.Sum(x => x.LineTotal - (x.Quantity * x.UnitCost))
            })
            .OrderByDescending(x => x.TotalSales)
            .Take(count)
            .ToListAsync(ct);

        decimal overallSales = topGrouped.Sum(x => x.TotalSales);

        return topGrouped.Select(x => new TopSellingProductDto
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            CategoryName = x.CategoryName,
            QuantitySold = x.QuantitySold,
            TotalSales = x.TotalSales,
            TotalProfit = x.TotalProfit,
            PercentageOfTotalSales = overallSales > 0 ? Math.Round((x.TotalSales / overallSales) * 100, 1) : 0
        }).ToList();
    }

    private async Task<List<LowStockItemDto>> GetLowStockAlertsInternalAsync(Guid? branchId, CancellationToken ct)
    {
        var showroomQuery = _dbContext.ShowroomStocks
            .AsNoTracking()
            .Include(s => s.Product)
                .ThenInclude(p => p.Category)
            .Include(s => s.Warehouse)
            .Where(s => s.Quantity <= s.MinStockLevel || s.Quantity <= 5);

        var storgeQuery = _dbContext.StorgeStocks
            .AsNoTracking()
            .Include(s => s.ProductBarcode)
                .ThenInclude(b => b.Product)
                    .ThenInclude(p => p!.Category)
            .Include(s => s.Warehouse)
            .Where(s => s.Quantity <= s.MinStockLevel || s.Quantity <= 5);

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            showroomQuery = showroomQuery.Where(s => s.Warehouse.BranchId == branchId.Value);
            storgeQuery = storgeQuery.Where(s => s.Warehouse.BranchId == branchId.Value);
        }

        var showroomRecords = await showroomQuery
            .OrderBy(s => s.Quantity)
            .Take(10)
            .ToListAsync(ct);

        var storgeRecords = await storgeQuery
            .OrderBy(s => s.Quantity)
            .Take(10)
            .ToListAsync(ct);

        var combinedList = new List<LowStockItemDto>();

        foreach (var s in showroomRecords)
        {
            string status = s.Quantity <= 0 ? "نفد" : (s.Quantity <= Math.Max(1, s.MinStockLevel / 2) ? "حرج" : "منخفض");
            combinedList.Add(new LowStockItemDto
            {
                ProductId = s.ProductId,
                ProductName = s.Product.Name,
                CategoryName = s.Product.Category != null ? s.Product.Category.Name : "عام",
                WarehouseName = s.Warehouse.Name + " (صالة)",
                CurrentQuantity = s.Quantity,
                MinStockLevel = s.MinStockLevel,
                CostPrice = s.Product.CostPrice,
                SalePrice = s.Product.SalePrice,
                Status = status
            });
        }

        foreach (var s in storgeRecords)
        {
            if (s.ProductBarcode?.Product == null) continue;
            var prod = s.ProductBarcode.Product;
            string status = s.Quantity <= 0 ? "نفد" : (s.Quantity <= Math.Max(1, s.MinStockLevel / 2) ? "حرج" : "منخفض");
            combinedList.Add(new LowStockItemDto
            {
                ProductId = prod.Id,
                ProductName = $"{prod.Name} ({s.ProductBarcode.Title})",
                CategoryName = prod.Category != null ? prod.Category.Name : "عام",
                WarehouseName = s.Warehouse.Name + " (مخزن)",
                CurrentQuantity = s.Quantity,
                MinStockLevel = s.MinStockLevel,
                CostPrice = prod.CostPrice,
                SalePrice = prod.SalePrice,
                Status = status
            });
        }

        return combinedList.OrderBy(x => x.CurrentQuantity).Take(10).ToList();
    }
}
