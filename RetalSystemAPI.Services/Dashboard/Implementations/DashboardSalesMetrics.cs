using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models.DTOs.Dashboard;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Services.Dashboard.Implementations;

/// <summary>
/// فئة مساعدة داخلية لحساب مؤشرات وإحصائيات المبيعات للوحة التحكم مع مراعاة المنطقة الزمنية للمستأجر وفروقات التوقيت الصيفي.
/// </summary>
/// <param name="db">سياق قاعدة البيانات للاستعلام المباشر عن فواتير ومرتجعات المبيعات</param>
/// <param name="clock">مزود التوقيت الموحد للنظام</param>
internal sealed class DashboardSalesMetrics(AppDbContext db, TimeProvider clock)
{
    /// <summary>
    /// سجل تقويم زمني يحدد المنطقة الزمنية وتاريخ اليوم المحلي للمستأجر مع دوال التحويل لتوقيت UTC.
    /// </summary>
    /// <param name="Zone">المنطقة الزمنية المعتمدة للمستأجر</param>
    /// <param name="Today">تاريخ بداية اليوم الحالي وفق المنطقة الزمنية للمستأجر</param>
    internal sealed record Calendar(TimeZoneInfo Zone, DateTime Today)
    {
        /// <summary>
        /// تحويل تاريخ ووقت محلي إلى التوقيت العالمي المنسق UTC مع معالجة الأوقات الملتبسة وغير الصالحة الناتجة عن التوقيت الصيفي.
        /// </summary>
        /// <param name="date">التاريخ والوقت المحلي</param>
        /// <returns>التاريخ والوقت المقابل بالتوقيت العالمي UTC</returns>
        public DateTime Utc(DateTime date)
        {
            // تحديد نوع التاريخ كغير محدد لتمكين التحويل وفق المنطقة الزمنية المخصصة
            var local = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);

            // تقديم الوقت دقيقة بدقيقة في حال كان الوقت غير صالح بسبب القفزة الصيفية
            while (Zone.IsInvalidTime(local)) local = local.AddMinutes(1);

            // في حال وجود التباس زمني نأخذ أقصى إزاحة زمنية متاحة، وإلا نحول الوقت إلى UTC مباشرة
            return Zone.IsAmbiguousTime(local)
                ? new DateTimeOffset(local, Zone.GetAmbiguousTimeOffsets(local).Max()).UtcDateTime
                : TimeZoneInfo.ConvertTimeToUtc(local, Zone);
        }
    }

    /// <summary>
    /// استخراج التقويم الزمني للمستأجر الحالي وقراءة منطقته الزمنية من الإعدادات.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>كائن التقويم الزمني المهيأ بالمنطقة الزمنية وتاريخ اليوم</returns>
    public async Task<Calendar> GetCalendarAsync(CancellationToken ct)
    {
        // استعلام اسم المنطقة الزمنية المسجلة للمستأجر الحالي مع اعتماد توقيت طرابلس كقيمة افتراضية
        var zoneId = await db.Tenants.AsNoTracking()
            .Where(t => t.Id == db.CurrentTenantId)
            .Select(t => t.Timezone)
            .FirstOrDefaultAsync(ct) ?? "Africa/Tripoli";

        // متغير لتخزين كائن المنطقة الزمنية
        TimeZoneInfo zone;

        // محاولة العثور على المنطقة الزمنية في النظام مع الرجوع لتوقيت UTC في حال عدم العثور عليها
        try
        {
            // البحث عن المنطقة الزمنية بالاسم
            zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // الرجوع لتوقيت UTC في حال عدم العثور على المنطقة
            zone = TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            // الرجوع لتوقيت UTC في حال وجود خلل في تعريف المنطقة
            zone = TimeZoneInfo.Utc;
        }

        // إنشاء كائن التقويم بحساب تاريخ اليوم المحلي للمنطقة الزمنية
        return new(zone, TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).Date);
    }

    /// <summary>
    /// بناء استعلام فواتير المبيعات الصالحة مستبعداً المسودات والفواتير الملغاة والباطلة، ومفلترة بالفرع عند التحديد.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <returns>استعلام قابل للتركيب IQueryable لفواتير المبيعات</returns>
    public IQueryable<SalesInvoice> Invoices(Guid? branchId) =>
        // استعلام قراءة فقط لفواتير المبيعات
        db.SalesInvoices.AsNoTracking()
            // استبعاد فواتير المسودة والملغاة والباطلة
            .Where(i => i.Status != InvoiceStatus.Draft && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided)
            // تصفية حسب الفرع في حال كان المعرف محدداً وغير فارغ
            .Where(i => !branchId.HasValue || branchId == Guid.Empty || i.BranchId == branchId);

    /// <summary>
    /// حساب صافي المبيعات (إجمالي المبيعات - إجمالي المرتجعات) لفترة زمنية محددة.
    /// </summary>
    /// <param name="start">تاريخ بداية الفترة بالتوقيت العالمي</param>
    /// <param name="end">تاريخ نهاية الفترة بالتوقيت العالمي</param>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>صافي قيمة المبيعات خلال الفترة</returns>
    public async Task<decimal> NetSalesAsync(DateTime start, DateTime end, Guid? branchId, CancellationToken ct)
    {
        // حساب إجمالي مبيعات الفواتير الصالحة ضمن النطاق الزمني
        var sales = await Invoices(branchId)
            .Where(i => i.InvoiceDate >= start && i.InvoiceDate < end)
            .SumAsync(i => i.TotalAmount, ct);

        // حساب إجمالي مرتجعات المبيعات ضمن نفس النطاق الزمني والفرع
        var returns = await db.SalesReturns.AsNoTracking()
            .Where(r => (!branchId.HasValue || branchId == Guid.Empty || r.BranchId == branchId) && r.ReturnDate >= start && r.ReturnDate < end)
            .SumAsync(r => r.TotalAmount, ct);

        // إرجاع صافي المبيعات بعد طرح المرتجعات
        return sales - returns;
    }

    /// <summary>
    /// بناء شجرة تعبيرات (Expression Tree) لتوليد عبارة SQL CASE لتجميع التواريخ في حزم يومية دقيقة تأخذ بالحسبان فروقات التوقيت الصيفي.
    /// </summary>
    /// <typeparam name="T">نوع الكيان المستعلم عنه</typeparam>
    /// <param name="date">تعبير استخراج خاصية التاريخ من الكيان</param>
    /// <param name="boundaries">مصفوفة الحدود الزمنية الفاصلة بين الأيام بالتوقيت العالمي</param>
    /// <returns>تعبير دالة يرجع مؤشر رقم اليوم من 0 إلى n</returns>
    private static Expression<Func<T, int>> DayBucket<T>(Expression<Func<T, DateTime>> date, DateTime[] boundaries)
    {
        // القيمة الافتراضية للحزمة الأخيرة
        Expression bucket = Expression.Constant(boundaries.Length - 2);

        // بناء شروط CASE المتداخلة عكسياً من اليوم الأخير إلى اليوم الأول
        for (var day = boundaries.Length - 3; day >= 0; day--)
        {
            // بناء شرط: إذا كان التاريخ أقل من نهاية اليوم الحالي يتم إرجاع رقم اليوم الحالي، وإلا ينتقل للحزمة التالية
            bucket = Expression.Condition(
                Expression.LessThan(date.Body, Expression.Constant(boundaries[day + 1])),
                Expression.Constant(day),
                bucket);
        }

        // إنشاء وإرجاع تعبير Lambda النهائي
        return Expression.Lambda<Func<T, int>>(bucket, date.Parameters);
    }

    /// <summary>
    /// استخراج نقاط المبيعات اليومية لعدد محدد من الأيام تتضمن المبيعات والأرباح والمرتجعات وعدد الطلبات.
    /// </summary>
    /// <param name="days">عدد الأيام المطلوبة في الإحصائية</param>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="calendar">التقويم الزمني للمستأجر</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بنقاط المبيعات اليومية مع أسماء الأيام وصافي القيم</returns>
    public async Task<List<DailySalesPointDto>> DaysAsync(int days, Guid? branchId, Calendar calendar, CancellationToken ct)
    {
        // حصر عدد الأيام المطلوبة بين يوم واحد وسنة كاملة 366 يوماً
        days = Math.Clamp(days, 1, 366);

        // حساب تاريخ أول يوم في نطاق الاستعلام
        var firstDay = calendar.Today.AddDays(1 - days);

        // إنشاء مصفوفة الحدود الزمنية اليومية بالتوقيت العالمي UTC
        var boundaries = Enumerable.Range(0, days + 1).Select(d => calendar.Utc(firstDay.AddDays(d))).ToArray();

        // بداية الفترة بالتوقيت العالمي
        var start = boundaries[0];

        // نهاية الفترة بالتوقيت العالمي
        var end = boundaries[^1];

        // استعلام وتجميع المبيعات وعدد الطلبات لكل يوم
        var sales = await Invoices(branchId)
            // تصفية الفترة الزمنية
            .Where(i => i.InvoiceDate >= start && i.InvoiceDate < end)
            // التجميع باستخدام تعبير الحزم اليومية
            .GroupBy(DayBucket<SalesInvoice>(i => i.InvoiceDate, boundaries))
            // اختيار رقم اليوم وإجمالي المبلغ وعدد الطلبات
            .Select(g => new { Day = g.Key, Amount = g.Sum(i => i.TotalAmount), Orders = g.Count() })
            // تحويل النتيجة لقاموس بمفتاح رقم اليوم
            .ToDictionaryAsync(x => x.Day, ct);

        // استعلام وتجميع أرباح بنود المبيعات (الإيراد - التكلفة) لكل يوم
        var profits = await SaleLines(branchId)
            // تصفية الفترة الزمنية
            .Where(i => i.Date >= start && i.Date < end)
            // التجميع بالحزم اليومية
            .GroupBy(DayBucket<MetricLine>(i => i.Date, boundaries))
            // حساب مجمل الربح
            .Select(g => new { Day = g.Key, Profit = g.Sum(i => i.Revenue - i.Cost) })
            // تحويل لقاموس
            .ToDictionaryAsync(x => x.Day, ct);

        // استعلام وتجميع المرتجعات وأثرها على الأرباح لكل يوم
        var returns = await ReturnLines(branchId)
            // تصفية الفترة الزمنية
            .Where(i => i.Date >= start && i.Date < end)
            // التجميع بالحزم اليومية
            .GroupBy(DayBucket<MetricLine>(i => i.Date, boundaries))
            // حساب مبلغ المرتجع وأثره على الأرباح
            .Select(g => new { Day = g.Key, Amount = g.Sum(i => i.Total), Profit = g.Sum(i => i.Revenue - i.Cost) })
            // تحويل لقاموس
            .ToDictionaryAsync(x => x.Day, ct);

        // دمج النتائج وتوليد قائمة النقاط اليومية لكافة الأيام المطلوبة
        return Enumerable.Range(0, days).Select(day => new DailySalesPointDto
        {
            // التاريخ المحلي لليوم
            Date = firstDay.AddDays(day),
            // اسم اليوم باللغة العربية
            DayName = firstDay.AddDays(day).ToString("dddd", System.Globalization.CultureInfo.GetCultureInfo("ar-SA")),
            // صافي المبيعات بعد طرح المرتجع
            SalesAmount = (sales.GetValueOrDefault(day)?.Amount ?? 0) - (returns.GetValueOrDefault(day)?.Amount ?? 0),
            // صافي الأرباح بعد طرح أرباح المرتجعات
            ProfitAmount = decimal.Round((profits.GetValueOrDefault(day)?.Profit ?? 0) - (returns.GetValueOrDefault(day)?.Profit ?? 0), 4),
            // عدد الطلبات
            OrdersCount = sales.GetValueOrDefault(day)?.Orders ?? 0
        }).ToList();
    }

    /// <summary>
    /// استخراج قائمة المنتجات الأكثر مبيعاً مع كمياتها وإيراداتها وأرباحها ونسبتها من إجمالي المبيعات.
    /// </summary>
    /// <param name="count">العدد المطلوب للمنتجات الأكثر مبيعاً</param>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة الأصناف الأكثر مبيعاً مرتبة تنازلياً بالإيراد</returns>
    public async Task<List<TopSellingProductDto>> ProductsAsync(int count, Guid? branchId, CancellationToken ct)
    {
        // استعلام وتجميع الكميات والإيرادات وتكاليف المنتجات المباعة
        var sold = await SaleLines(branchId)
            // تجميع حسب المنتج واسمه وتصنيفه
            .GroupBy(i => new { i.ProductId, i.ProductName, i.CategoryName })
            // حساب المجاميع
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.CategoryName,
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Revenue),
                Cost = g.Sum(i => i.Cost)
            })
            // جلب البيانات
            .ToListAsync(ct);

        // استعلام وتجميع حركات المرتجعات للمنتجات بإشارة سالبة لطرحها من المبيعات
        var returned = await ReturnLines(branchId)
            // تجميع حسب المنتج واسمه وتصنيفه
            .GroupBy(i => new { i.ProductId, i.ProductName, i.CategoryName })
            // حساب المجاميع بإشارة سالبة
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.CategoryName,
                Quantity = -g.Sum(i => i.Quantity),
                Revenue = -g.Sum(i => i.Revenue),
                Cost = -g.Sum(i => i.Cost)
            })
            // جلب البيانات
            .ToListAsync(ct);

        // دمج المبيعات والمرتجعات واستخراج صافي الكمية والإيراد والربح لكل منتج
        var top = sold.Concat(returned)
            // تجميع بالمعرف للمنتج
            .GroupBy(i => i.ProductId)
            // بناء كائن DTO للمنتج الأكثر مبيعاً
            .Select(g => new TopSellingProductDto
            {
                ProductId = g.Key,
                ProductName = g.First().ProductName,
                CategoryName = g.First().CategoryName,
                QuantitySold = g.Sum(i => i.Quantity),
                TotalSales = decimal.Round(g.Sum(i => i.Revenue), 4),
                TotalProfit = decimal.Round(g.Sum(i => i.Revenue - i.Cost), 4)
            })
            // ترتيب تنازلي حسب إجمالي المبيعات ثم بالمعرف
            .OrderByDescending(i => i.TotalSales).ThenBy(i => i.ProductId)
            // أخذ العدد المطلوب وحصره بين 1 و 100
            .Take(Math.Clamp(count, 1, 100))
            // تحويل لقائمة
            .ToList();

        // حساب إجمالي مبيعات كافة المنتجات المتصدرة لحساب النسب المئوية
        var overall = top.Sum(i => i.TotalSales);

        // حساب النسبة المئوية لمبيعات كل صنف من إجمالي المبيعات
        foreach (var product in top)
        {
            // تعيين النسبة المئوية مقربة لخانات عشرية واحدة
            product.PercentageOfTotalSales = overall > 0 ? Math.Round(product.TotalSales / overall * 100, 1) : 0;
        }

        // إرجاع قائمة المنتجات المتصدرة
        return top;
    }

    /// <summary>
    /// استعلام مسطح لبنود المبيعات وتوزيع خصومات وتر الترويسة على البنود واستبعاد الضريبة من صافي الإيراد.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <returns>استعلام بنود المبيعات القياسية</returns>
    private IQueryable<MetricLine> SaleLines(Guid? branchId) =>
        // تفكيك بنود الفواتير الصالحة إلى أسطر مفردة
        Invoices(branchId).SelectMany(i => i.Items, (invoice, item) => new MetricLine
        {
            // تاريخ الفاتورة
            Date = invoice.InvoiceDate,
            // معرف المنتج
            ProductId = item.ProductId,
            // اسم المنتج
            ProductName = item.Product.Name,
            // اسم التصنيف أو "عام" في حال عدم وجود تصنيف
            CategoryName = item.Product.Category != null ? item.Product.Category.Name : "عام",
            // الكمية المباعة
            Quantity = item.Quantity,
            // إجمالي السطر
            Total = item.LineTotal,
            // حساب الإيراد بعد توزيع خصم الترويسة واستبعاد الضريبة لأنها التزام وليست ربحاً
            Revenue = invoice.TotalAmount - invoice.TaxAmount + invoice.DiscountAmount > 0
                ? item.LineTotal * (invoice.TotalAmount - invoice.TaxAmount) / (invoice.TotalAmount - invoice.TaxAmount + invoice.DiscountAmount)
                : 0,
            // إجمالي تكلفة البند
            Cost = item.Quantity * item.UnitCost
        });

    /// <summary>
    /// استعلام مسطح لبنود المرتجعات ومطابقتها بتكاليف الفواتير الأصلية لاحتساب الأثر المالي الدقيق.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <returns>استعلام بنود المرتجعات القياسية</returns>
    private IQueryable<MetricLine> ReturnLines(Guid? branchId)
    {
        // استعلام متوسط تكلفة الوحدة لبنود الفواتير الأصلية
        var costs = db.SalesInvoiceItems.AsNoTracking()
            // تجميع بالمعرف والباركود
            .GroupBy(i => new { InvoiceId = (Guid?)i.SalesInvoiceId, i.ProductId, i.ProductBarCodeId })
            // حساب متوسط التكلفة المرجح بالكميات
            .Select(g => new
            {
                g.Key.InvoiceId,
                g.Key.ProductId,
                g.Key.ProductBarCodeId,
                UnitCost = (decimal?)(g.Sum(i => i.Quantity * i.UnitCost) / g.Sum(i => (decimal)i.Quantity))
            });

        // ربط بنود المرتجعات بالتكاليف الأصلية للفاتورة
        return from item in db.SalesReturnItems.AsNoTracking()
               // تصفية حسب الفرع
               where !branchId.HasValue || branchId == Guid.Empty || item.SalesReturn.BranchId == branchId
               // مطابقة مع تكلفة الفاتورة الأصلية
               join cost in costs on new { InvoiceId = item.SalesReturn.OriginalInvoiceId, item.ProductId, item.ProductBarCodeId }
                   equals new { cost.InvoiceId, cost.ProductId, cost.ProductBarCodeId } into matches
               // ربط خارجي أيسر Left Join
               from cost in matches.DefaultIfEmpty()
               // بناء كائن السطر القياسي
               select new MetricLine
               {
                   Date = item.SalesReturn.ReturnDate,
                   ProductId = item.ProductId,
                   ProductName = item.Product.Name,
                   CategoryName = item.Product.Category != null ? item.Product.Category.Name : "عام",
                   Quantity = item.Quantity,
                   Total = item.LineTotal,
                   Revenue = item.SalesReturn.OriginalInvoice != null && item.SalesReturn.OriginalInvoice.TotalAmount > 0
                       ? item.LineTotal * (item.SalesReturn.OriginalInvoice.TotalAmount - item.SalesReturn.OriginalInvoice.TaxAmount) / item.SalesReturn.OriginalInvoice.TotalAmount
                       : item.LineTotal,
                   Cost = item.Quantity * (cost.UnitCost ?? (item.Product.AveragePrice > 0 ? item.Product.AveragePrice : item.Product.CostPrice))
               };
    }

    /// <summary>
    /// كائن داخلي لتمثيل سطر حركة قياسي (مبيعات أو مرتجعات) موحد لحساب الإيرادات والتكاليف.
    /// </summary>
    private sealed class MetricLine
    {
        /// <summary>تاريخ الحركة</summary>
        public DateTime Date { get; init; }
        /// <summary>معرف المنتج</summary>
        public Guid ProductId { get; init; }
        /// <summary>اسم المنتج</summary>
        public string ProductName { get; init; } = "";
        /// <summary>اسم التصنيف</summary>
        public string CategoryName { get; init; } = "";
        /// <summary>الكمية</summary>
        public int Quantity { get; init; }
        /// <summary>إجمالي المبلغ</summary>
        public decimal Total { get; init; }
        /// <summary>صافي الإيراد</summary>
        public decimal Revenue { get; init; }
        /// <summary>إجمالي التكلفة</summary>
        public decimal Cost { get; init; }
    }
}
