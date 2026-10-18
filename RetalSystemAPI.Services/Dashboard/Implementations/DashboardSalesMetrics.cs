using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models.DTOs.Dashboard;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Services.Dashboard.Implementations;

internal sealed class DashboardSalesMetrics(AppDbContext db, TimeProvider clock)
{
    internal sealed record Calendar(TimeZoneInfo Zone, DateTime Today)
    {
        public DateTime Utc(DateTime date)
        {
            var local = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            while (Zone.IsInvalidTime(local)) local = local.AddMinutes(1);
            return Zone.IsAmbiguousTime(local)
                ? new DateTimeOffset(local, Zone.GetAmbiguousTimeOffsets(local).Max()).UtcDateTime
                : TimeZoneInfo.ConvertTimeToUtc(local, Zone);
        }
    }

    public async Task<Calendar> GetCalendarAsync(CancellationToken ct)
    {
        var zoneId = await db.Tenants.AsNoTracking().Where(t => t.Id == db.CurrentTenantId)
            .Select(t => t.Timezone).FirstOrDefaultAsync(ct) ?? "Africa/Tripoli";
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId); }
        catch (TimeZoneNotFoundException) { zone = TimeZoneInfo.Utc; }
        catch (InvalidTimeZoneException) { zone = TimeZoneInfo.Utc; }
        return new(zone, TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).Date);
    }

    public IQueryable<SalesInvoice> Invoices(Guid? branchId) => db.SalesInvoices.AsNoTracking()
        .Where(i => i.Status != InvoiceStatus.Draft && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided)
        .Where(i => !branchId.HasValue || branchId == Guid.Empty || i.BranchId == branchId);

    public async Task<decimal> NetSalesAsync(DateTime start, DateTime end, Guid? branchId, CancellationToken ct)
    {
        var sales = await Invoices(branchId).Where(i => i.InvoiceDate >= start && i.InvoiceDate < end).SumAsync(i => i.TotalAmount, ct);
        var returns = await db.SalesReturns.AsNoTracking()
            .Where(r => (!branchId.HasValue || branchId == Guid.Empty || r.BranchId == branchId) && r.ReturnDate >= start && r.ReturnDate < end)
            .SumAsync(r => r.TotalAmount, ct);
        return sales - returns;
    }

    // Build SQL CASE buckets from each local midnight's UTC instant. Unlike a
    // fixed offset or DateDiffDay over UTC, this also handles DST and half hours.
    private static Expression<Func<T, int>> DayBucket<T>(Expression<Func<T, DateTime>> date, DateTime[] boundaries)
    {
        Expression bucket = Expression.Constant(boundaries.Length - 2);
        for (var day = boundaries.Length - 3; day >= 0; day--)
            bucket = Expression.Condition(Expression.LessThan(date.Body, Expression.Constant(boundaries[day + 1])), Expression.Constant(day), bucket);
        return Expression.Lambda<Func<T, int>>(bucket, date.Parameters);
    }

    public async Task<List<DailySalesPointDto>> DaysAsync(int days, Guid? branchId, Calendar calendar, CancellationToken ct)
    {
        days = Math.Clamp(days, 1, 366);
        var firstDay = calendar.Today.AddDays(1 - days);
        var boundaries = Enumerable.Range(0, days + 1).Select(d => calendar.Utc(firstDay.AddDays(d))).ToArray();
        var start = boundaries[0];
        var end = boundaries[^1];
        var sales = await Invoices(branchId).Where(i => i.InvoiceDate >= start && i.InvoiceDate < end)
            .GroupBy(DayBucket<SalesInvoice>(i => i.InvoiceDate, boundaries))
            .Select(g => new { Day = g.Key, Amount = g.Sum(i => i.TotalAmount), Orders = g.Count() }).ToDictionaryAsync(x => x.Day, ct);
        var profits = await SaleLines(branchId).Where(i => i.Date >= start && i.Date < end)
            .GroupBy(DayBucket<MetricLine>(i => i.Date, boundaries))
            .Select(g => new { Day = g.Key, Profit = g.Sum(i => i.Revenue - i.Cost) }).ToDictionaryAsync(x => x.Day, ct);
        var returns = await ReturnLines(branchId).Where(i => i.Date >= start && i.Date < end)
            .GroupBy(DayBucket<MetricLine>(i => i.Date, boundaries))
            .Select(g => new { Day = g.Key, Amount = g.Sum(i => i.Total), Profit = g.Sum(i => i.Revenue - i.Cost) }).ToDictionaryAsync(x => x.Day, ct);
        return Enumerable.Range(0, days).Select(day => new DailySalesPointDto
        {
            Date = firstDay.AddDays(day),
            DayName = firstDay.AddDays(day).ToString("dddd", System.Globalization.CultureInfo.GetCultureInfo("ar-SA")),
            SalesAmount = (sales.GetValueOrDefault(day)?.Amount ?? 0) - (returns.GetValueOrDefault(day)?.Amount ?? 0),
            ProfitAmount = decimal.Round((profits.GetValueOrDefault(day)?.Profit ?? 0) - (returns.GetValueOrDefault(day)?.Profit ?? 0), 4),
            OrdersCount = sales.GetValueOrDefault(day)?.Orders ?? 0
        }).ToList();
    }

    public async Task<List<TopSellingProductDto>> ProductsAsync(int count, Guid? branchId, CancellationToken ct)
    {
        var sold = await SaleLines(branchId).GroupBy(i => new { i.ProductId, i.ProductName, i.CategoryName })
            .Select(g => new { g.Key.ProductId, g.Key.ProductName, g.Key.CategoryName, Quantity = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.Revenue), Cost = g.Sum(i => i.Cost) }).ToListAsync(ct);
        var returned = await ReturnLines(branchId).GroupBy(i => new { i.ProductId, i.ProductName, i.CategoryName })
            .Select(g => new { g.Key.ProductId, g.Key.ProductName, g.Key.CategoryName, Quantity = -g.Sum(i => i.Quantity), Revenue = -g.Sum(i => i.Revenue), Cost = -g.Sum(i => i.Cost) }).ToListAsync(ct);
        var top = sold.Concat(returned).GroupBy(i => i.ProductId).Select(g => new TopSellingProductDto
        {
            ProductId = g.Key, ProductName = g.First().ProductName, CategoryName = g.First().CategoryName,
            QuantitySold = g.Sum(i => i.Quantity), TotalSales = decimal.Round(g.Sum(i => i.Revenue), 4),
            TotalProfit = decimal.Round(g.Sum(i => i.Revenue - i.Cost), 4)
        }).OrderByDescending(i => i.TotalSales).ThenBy(i => i.ProductId).Take(Math.Clamp(count, 1, 100)).ToList();
        var overall = top.Sum(i => i.TotalSales);
        foreach (var product in top)
            product.PercentageOfTotalSales = overall > 0 ? Math.Round(product.TotalSales / overall * 100, 1) : 0;
        return top;
    }

    private IQueryable<MetricLine> SaleLines(Guid? branchId) => Invoices(branchId).SelectMany(i => i.Items, (invoice, item) => new MetricLine
    {
        Date = invoice.InvoiceDate, ProductId = item.ProductId, ProductName = item.Product.Name,
        CategoryName = item.Product.Category != null ? item.Product.Category.Name : "عام", Quantity = item.Quantity,
        Total = item.LineTotal,
        // Tax is a liability, not profit. Allocate the header discount over lines.
        Revenue = invoice.TotalAmount - invoice.TaxAmount + invoice.DiscountAmount > 0
            ? item.LineTotal * (invoice.TotalAmount - invoice.TaxAmount) / (invoice.TotalAmount - invoice.TaxAmount + invoice.DiscountAmount) : 0,
        Cost = item.Quantity * item.UnitCost
    });

    private IQueryable<MetricLine> ReturnLines(Guid? branchId)
    {
        var costs = db.SalesInvoiceItems.AsNoTracking().GroupBy(i => new { InvoiceId = (Guid?)i.SalesInvoiceId, i.ProductId, i.ProductBarCodeId })
            .Select(g => new { g.Key.InvoiceId, g.Key.ProductId, g.Key.ProductBarCodeId, UnitCost = (decimal?)(g.Sum(i => i.Quantity * i.UnitCost) / g.Sum(i => (decimal)i.Quantity)) });
        return from item in db.SalesReturnItems.AsNoTracking()
               where !branchId.HasValue || branchId == Guid.Empty || item.SalesReturn.BranchId == branchId
               join cost in costs on new { InvoiceId = item.SalesReturn.OriginalInvoiceId, item.ProductId, item.ProductBarCodeId }
                   equals new { cost.InvoiceId, cost.ProductId, cost.ProductBarCodeId } into matches
               from cost in matches.DefaultIfEmpty()
               select new MetricLine
               {
                   Date = item.SalesReturn.ReturnDate, ProductId = item.ProductId, ProductName = item.Product.Name,
                   CategoryName = item.Product.Category != null ? item.Product.Category.Name : "عام", Quantity = item.Quantity,
                   Total = item.LineTotal,
                   Revenue = item.SalesReturn.OriginalInvoice != null && item.SalesReturn.OriginalInvoice.TotalAmount > 0
                       ? item.LineTotal * (item.SalesReturn.OriginalInvoice.TotalAmount - item.SalesReturn.OriginalInvoice.TaxAmount) / item.SalesReturn.OriginalInvoice.TotalAmount
                       : item.LineTotal,
                   Cost = item.Quantity * (cost.UnitCost ?? (item.Product.AveragePrice > 0 ? item.Product.AveragePrice : item.Product.CostPrice))
               };
    }

    private sealed class MetricLine
    {
        public DateTime Date { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = "";
        public string CategoryName { get; init; } = "";
        public int Quantity { get; init; }
        public decimal Total { get; init; }
        public decimal Revenue { get; init; }
        public decimal Cost { get; init; }
    }
}
