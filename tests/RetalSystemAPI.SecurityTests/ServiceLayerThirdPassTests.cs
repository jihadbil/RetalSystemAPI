using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Accounting.Implementations;
using RetalSystemAPI.Services.Catalog.Implementations;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Dashboard.Implementations;
using Xunit;
using Fixture = RetalSystemAPI.SecurityTests.ServiceLayerRegressionTests.Fixture;

namespace RetalSystemAPI.SecurityTests;

public sealed class ServiceLayerThirdPassTests
{
    [SqlServerFact]
    public async Task Opening_rejects_a_concurrent_deposit_and_retry_uses_the_new_balance()
    {
        await AssertOpeningRaceAsync(deposit: true);
    }

    [SqlServerFact]
    public async Task Concurrent_opening_returns_a_controlled_failure_and_keeps_only_one_session()
    {
        await AssertOpeningRaceAsync(deposit: false);
    }

    private static async Task AssertOpeningRaceAsync(bool deposit)
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var safe = await f.Db.Safes.SingleAsync();
        safe.Type = SafeType.CashDrawer;
        var terminal = new PosTerminal { BranchId = f.Branch.Id, DefaultCashDrawerId = safe.Id, Name = "POS", Code = "POS" };
        f.Db.AddRange(terminal, new ApplicationUser { Id = "test-user", UserName = "cashier", TenantId = tenant, BranchId = f.Branch.Id },
            new PosDeviceCredential { PosTerminal = terminal, TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("device"))), ExpiresAt = DateTime.UtcNow.AddDays(1) });
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var options = (DbContextOptions<AppDbContext>)f.Db.GetService<IDbContextOptions>();
        var pause = new BeforeSessionSave(async () =>
        {
            await using var other = new AppDbContext(options, new ModelIntegrityTests.TestTenant(tenant));
            var pos = new PosCashService(other, new ModelIntegrityTests.TestUser());
            if (deposit)
            {
                await using var uow = new UnitOfWork(other);
                var result = await new SafeService(uow, f.Mapper, pos).AddTransactionAsync(new()
                { SafeId = f.Safe.Id, Direction = TransactionDirection.Inbound, Amount = 100, Description = "Concurrent deposit" });
                Assert.True(result.IsSuccess, result.ErrorMessage);
            }
            else
            {
                var result = await pos.OpenSessionAsync("device", new() { OpeningFloat = 1000 });
                Assert.True(result.IsSuccess, result.ErrorMessage);
            }
        });
        await using var opening = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>(options).AddInterceptors(pause).Options, new ModelIntegrityTests.TestTenant(tenant));
        var service = new PosCashService(opening, new ModelIntegrityTests.TestUser());
        var failed = await service.OpenSessionAsync("device", new() { OpeningFloat = 1000 });
        Assert.False(failed.IsSuccess);
        Assert.Equal(ErrorCodes.ConcurrencyError, failed.ErrorCode);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(deposit ? 0 : 1, await f.Db.CashSessions.CountAsync());
        Assert.Equal(deposit ? 1100 : 1000, (await f.Db.Safes.SingleAsync()).CurrentBalance);
        if (deposit)
        {
            Assert.True((await service.OpenSessionAsync("device", new() { OpeningFloat = 1100 })).IsSuccess);
            var closed = await service.CloseSessionAsync("device", new() { CountedClosingBalance = 1100 });
            Assert.True(closed.IsSuccess, closed.ErrorMessage);
            Assert.Equal(CashSessionStatus.Closed, closed.Data!.Status);
            Assert.Equal(0, closed.Data.Difference);
        }
    }

    [SqlServerFact]
    public async Task Legacy_purchase_draft_cannot_enter_the_posted_invoice_edit_path()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var draft = new PurchaseInvoice
        {
            InvoiceNumber = "DRAFT", BranchId = f.Branch.Id, WarehouseId = f.Storage.Id, SupplierId = f.Supplier.Id,
            Status = InvoiceStatus.Draft, PaymentMethod = PaymentMethod.Credit, SubTotal = 20, TotalAmount = 20, RemainingAmount = 20,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 2, UnitPrice = 10, LineTotal = 20 }]
        };
        f.Db.Add(draft);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        foreach (var withItems in new[] { true, false })
        {
            var result = await f.Purchases.UpdateAsync(draft.Id, new()
            {
                InvoiceNumber = "DRAFT", SupplierId = f.Supplier.Id, BranchId = f.Branch.Id, WarehouseId = f.Storage.Id,
                PaymentMethod = PaymentMethod.Credit, Status = InvoiceStatus.Pending,
                Items = withItems ? [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 2, UnitPrice = 10 }] : null
            });
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.ValidationError, result.ErrorCode);
        }
        Assert.Equal(InvoiceStatus.Draft, (await f.Db.PurchaseInvoices.SingleAsync()).Status);
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(0, (await f.Db.Suppliers.SingleAsync()).CurrentBalance);
        Assert.Empty(await f.Db.SupplierTransactions.ToListAsync());
    }

    [Theory]
    [InlineData(true, "missing")]
    [InlineData(false, "missing")]
    [InlineData(true, "wrong-type")]
    [InlineData(false, "wrong-type")]
    [InlineData(true, "inactive")]
    [InlineData(false, "inactive")]
    public async Task Invalid_single_opening_destination_rejects_before_saving_a_product(bool showroom, string invalid)
    {
        await using var f = await Fixture.CreateAsync();
        var target = showroom ? f.Show.Id : f.Storage.Id;
        if (invalid == "missing") target = Guid.NewGuid();
        if (invalid == "wrong-type") target = showroom ? f.Storage.Id : f.Show.Id;
        if (invalid == "inactive")
        {
            (await f.Db.Warehouses.SingleAsync(w => w.Id == target)).IsActive = false;
            await f.Db.SaveChangesAsync();
        }
        var request = Product(f);
        if (showroom) request.ShowroomWarehouseId = target;
        else request.StorageWarehouseId = target;
        var result = await new ProductService(f.Uow, f.Mapper).CreateAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Equal(1, await f.Db.Products.CountAsync());
        Assert.Equal(1, await f.Db.ProductBarCodes.CountAsync());
        Assert.Equal(10, await f.Db.StorgeStocks.SumAsync(s => s.Quantity));
    }

    [SqlServerFact]
    public async Task Opening_quantities_are_saved_only_in_the_selected_typed_warehouses()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var request = Product(f);
        request.ShowroomWarehouseId = f.Show.Id;
        request.StorageWarehouseId = f.Storage.Id;
        var created = await new ProductService(f.Uow, f.Mapper).CreateAsync(request);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Equal(5, await f.Db.ShowroomStocks.Where(s => s.ProductId == created.Data!.Id).SumAsync(s => s.Quantity));
        Assert.Equal(7, await f.Db.StorgeStocks.Where(s => s.ProductBarcode.ProductId == created.Data!.Id).SumAsync(s => s.Quantity));
        Assert.False(await f.Db.ShowroomStocks.AnyAsync(s => s.Warehouse.Type != WarehouseType.Show));
        Assert.False(await f.Db.StorgeStocks.AnyAsync(s => s.Warehouse.Type != WarehouseType.Storge));
    }

    [SqlServerFact]
    public async Task Missing_showrooms_never_create_showroom_stock_in_storage()
    {
        await AssertMissingWarehouseTypeAsync(showroom: true);
    }

    [SqlServerFact]
    public async Task Missing_storage_never_creates_barcode_stock_in_showrooms()
    {
        await AssertMissingWarehouseTypeAsync(showroom: false);
    }

    private static async Task AssertMissingWarehouseTypeAsync(bool showroom)
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var missing = showroom ? f.Show.Id : f.Storage.Id;
        (await f.Db.Warehouses.SingleAsync(w => w.Id == missing)).IsDeleted = true;
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var service = new ProductService(f.Uow, f.Mapper);
        var request = Product(f);
        Assert.False((await service.CreateAsync(request)).IsSuccess);
        Assert.Equal(1, await f.Db.Products.CountAsync());
        request.InitialShowroomQuantity = 0;
        request.BarCodes[0].InitialQuantity = 0;
        var created = await service.CreateAsync(request);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        if (showroom) Assert.False(await f.Db.ShowroomStocks.AnyAsync(s => s.ProductId == created.Data!.Id));
        else Assert.False(await f.Db.StorgeStocks.AnyAsync(s => s.ProductBarcode.ProductId == created.Data!.Id));
        var updated = await service.UpdateAsync(created.Data!.Id, new()
        {
            Id = created.Data.Id, Name = request.Name, CategoryId = request.CategoryId, CostPrice = 10, SalePrice = 100,
            BarCodes = [new() { BarCode = "ADDED", Title = "Added" }]
        });
        Assert.True(updated.IsSuccess, updated.ErrorMessage);
        Assert.False(await f.Db.ShowroomStocks.AnyAsync(s => s.Warehouse.Type != WarehouseType.Show));
        Assert.False(await f.Db.StorgeStocks.AnyAsync(s => s.Warehouse.Type != WarehouseType.Storge));
    }

    [SqlServerFact]
    public async Task Unused_barcode_and_zero_stocks_are_soft_deleted_and_code_can_be_reused()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var service = new ProductBarCodeService(f.Uow, f.Mapper);
        var added = await service.AddBarCodeAsync(f.Product.Id, new() { ProductId = f.Product.Id, BarCode = "UNUSED", Title = "Unused" });
        Assert.True(added.IsSuccess, added.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        Assert.True((await service.RemoveBarCodeAsync(added.Data!.Id)).IsSuccess);
        Assert.False(await f.Db.ProductBarCodes.AnyAsync(b => b.Id == added.Data.Id));
        Assert.True((await f.Db.ProductBarCodes.IgnoreQueryFilters().SingleAsync(b => b.Id == added.Data.Id)).IsDeleted);
        Assert.True((await f.Db.StorgeStocks.IgnoreQueryFilters().SingleAsync(s => s.ProductBarcodeId == added.Data.Id)).IsDeleted);
        var reused = await service.AddBarCodeAsync(f.Product.Id, new() { ProductId = f.Product.Id, BarCode = "UNUSED", Title = "Reused" });
        Assert.True(reused.IsSuccess, reused.ErrorMessage);
        Assert.NotEqual(added.Data.Id, reused.Data!.Id);
    }

    [SqlServerFact]
    public async Task Barcode_with_stock_or_purchase_breakdown_history_cannot_be_deleted()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var service = new ProductBarCodeService(f.Uow, f.Mapper);
        Assert.False((await service.RemoveBarCodeAsync(f.Barcode.Id)).IsSuccess);
        var request = f.Purchase(1, 10);
        request.Items[0].ProductBarCodeId = null;
        request.Items[0].Breakdowns = [new() { ProductBarCodeId = f.Barcode.Id, PackageQuantity = 1, UnitsPerPackage = 1, UnitPrice = 10 }];
        Assert.True((await f.Purchases.CreateAsync(request)).IsSuccess);
        f.Db.ChangeTracker.Clear();
        (await f.Db.StorgeStocks.SingleAsync()).Quantity = 0;
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var refused = await service.RemoveBarCodeAsync(f.Barcode.Id);
        Assert.False(refused.IsSuccess);
        Assert.Equal(ErrorCodes.BarCodeInUse, refused.ErrorCode);
        Assert.True(await f.Db.ProductBarCodes.AnyAsync(b => b.Id == f.Barcode.Id));
        Assert.True(await f.Db.StorgeStocks.AnyAsync());
    }

    [SqlServerFact]
    public async Task Dashboard_includes_header_discount_tax_and_full_return_using_original_cost()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var now = DateTimeOffset.Parse("2026-10-16T22:30:00Z");
        (await f.Db.ShowroomStocks.SingleAsync()).Quantity = 10;
        await f.Db.SaveChangesAsync();
        var request = Sale(f, now.UtcDateTime);
        request.DiscountAmount = 20;
        request.TaxAmount = 8;
        request.PaidAmount = 88;
        var sale = await f.Sales.CreateAsync(request);
        Assert.True(sale.IsSuccess, sale.ErrorMessage);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var dashboard = new DashboardService(f.Db, cache, new FixedClock(now));
        var summary = (await dashboard.GetSummaryAsync()).Data!;
        Assert.Equal(88, summary.Kpis.TodaySales);
        Assert.Equal(70, summary.Kpis.TodayProfit);
        Assert.Equal(0, summary.Kpis.YesterdaySales);
        Assert.Equal(new DateTime(2026, 10, 17), summary.DailySalesTrend[^1].Date);
        Assert.Equal(70, summary.TopSellingProducts.Single().TotalProfit);
        Assert.Equal(80, summary.TopSellingProducts.Single().TotalSales);
        (await f.Db.Products.SingleAsync()).AveragePrice = 99;
        await f.Db.SaveChangesAsync();
        var returned = f.SalesReturn(sale.Data!.Id, 1);
        returned.ReturnDate = now.UtcDateTime;
        Assert.True((await f.Returns.CreateAsync(returned)).IsSuccess);
        summary = (await dashboard.GetSummaryAsync()).Data!;
        Assert.Equal(0, summary.Kpis.TodaySales);
        Assert.Equal(0, summary.Kpis.ThisMonthSales);
        Assert.Equal(0, summary.Kpis.TodayProfit);
        Assert.Equal(0, summary.DailySalesTrend[^1].ProfitAmount);
        Assert.Equal(0, summary.TopSellingProducts.Single().QuantitySold);
        Assert.Equal(0, summary.TopSellingProducts.Single().TotalProfit);
    }

    [SqlServerFact]
    public async Task Partial_returns_reduce_the_period_of_the_return_and_deleted_returns_are_excluded()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var now = DateTimeOffset.Parse("2026-10-17T10:00:00Z");
        (await f.Db.ShowroomStocks.SingleAsync()).Quantity = 10;
        await f.Db.SaveChangesAsync();
        var request = Sale(f, now.UtcDateTime, 2);
        request.Items[0].DiscountAmount = 20;
        request.DiscountAmount = 20;
        request.TaxAmount = 16;
        request.PaidAmount = 176;
        var sale = await f.Sales.CreateAsync(request);
        Assert.True(sale.IsSuccess, sale.ErrorMessage);
        var returned = f.SalesReturn(sale.Data!.Id, 1);
        returned.ReturnDate = now.UtcDateTime;
        Assert.True((await f.Returns.CreateAsync(returned)).IsSuccess);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var dashboard = new DashboardService(f.Db, cache, new FixedClock(now));
        var summary = (await dashboard.GetSummaryAsync(f.Branch.Id)).Data!;
        Assert.Equal(88, summary.Kpis.TodaySales);
        Assert.Equal(70, summary.Kpis.TodayProfit);
        Assert.Equal(80, summary.TopSellingProducts.Single().TotalSales);
        var otherBranch = (await dashboard.GetSummaryAsync(Guid.NewGuid())).Data!;
        Assert.Equal(0, otherBranch.Kpis.TodaySales);
        Assert.Empty(otherBranch.TopSellingProducts);
        returned.ReturnNumber = Guid.NewGuid().ToString("N");
        returned.ReturnDate = now.AddDays(1).UtcDateTime;
        var nextReturn = await f.Returns.CreateAsync(returned);
        Assert.True(nextReturn.IsSuccess, nextReturn.ErrorMessage);
        Assert.Equal(88, (await dashboard.GetSummaryAsync()).Data!.Kpis.TodaySales);
        var tomorrow = new DashboardService(f.Db, cache, new FixedClock(now.AddDays(1)));
        summary = (await tomorrow.GetSummaryAsync()).Data!;
        Assert.Equal(-88, summary.Kpis.TodaySales);
        Assert.Equal(-70, summary.Kpis.TodayProfit);
        Assert.Equal(88, summary.Kpis.YesterdaySales);
        Assert.True((await f.Returns.DeleteAsync(nextReturn.Data!.Id)).IsSuccess);
        Assert.Equal(0, (await tomorrow.GetSummaryAsync()).Data!.Kpis.TodaySales);
    }

    [SqlServerFact]
    public async Task Returns_with_nullable_barcode_and_without_invoice_keep_metrics_tenant_scoped()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var now = DateTimeOffset.Parse("2026-10-17T10:00:00Z");
        var invoice = f.MakeSale(2, 100, 200);
        invoice.InvoiceDate = now.UtcDateTime;
        invoice.Items.Single().ProductBarCodeId = null;
        invoice.Items.Single().UnitCost = 20;
        (await f.Db.Tenants.SingleAsync()).Name = "First tenant";
        f.Db.Add(invoice);
        f.Db.AddRange(new SalesReturn
        {
            ReturnNumber = "LINKED", ReturnDate = now.UtcDateTime, OriginalInvoiceId = invoice.Id,
            BranchId = f.Branch.Id, WarehouseId = f.Show.Id, TotalAmount = 100,
            Items = [new() { ProductId = f.Product.Id, Quantity = 1, UnitPrice = 100, LineTotal = 100 }]
        }, new SalesReturn
        {
            ReturnNumber = "UNLINKED", ReturnDate = now.UtcDateTime, BranchId = f.Branch.Id, WarehouseId = f.Show.Id, TotalAmount = 40,
            Items = [new() { ProductId = f.Product.Id, Quantity = 1, UnitPrice = 40, LineTotal = 40 }]
        });
        await f.Db.SaveChangesAsync();
        var otherTenant = Guid.NewGuid();
        await using var other = await Fixture.CreateAsync(database.Context(otherTenant), otherTenant);
        var unrelated = other.MakeSale(1, 1000, 1000);
        unrelated.InvoiceDate = now.UtcDateTime;
        other.Db.Add(unrelated);
        await other.Db.SaveChangesAsync();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var summary = (await new DashboardService(f.Db, cache, new FixedClock(now)).GetSummaryAsync()).Data!;
        Assert.Equal(60, summary.Kpis.TodaySales);
        Assert.Equal(50, summary.Kpis.TodayProfit);
        Assert.Equal(0, summary.TopSellingProducts.Single().QuantitySold);
        Assert.Equal(50, summary.TopSellingProducts.Single().TotalProfit);
    }

    [SqlServerFact]
    public async Task Daily_bounds_cover_the_23_hour_spring_day()
    {
        await AssertDayBoundsAsync("America/New_York", "2026-03-08T15:00:00Z", "2026-03-08T05:00:00Z", "2026-03-09T04:00:00Z");
    }

    [SqlServerFact]
    public async Task Daily_bounds_cover_the_25_hour_autumn_day()
    {
        await AssertDayBoundsAsync("America/New_York", "2026-11-01T15:00:00Z", "2026-11-01T04:00:00Z", "2026-11-02T05:00:00Z");
    }

    [SqlServerFact]
    public async Task Daily_bounds_support_a_non_integer_hour_offset_and_month_boundary()
    {
        await AssertDayBoundsAsync("Asia/Kathmandu", "2026-10-31T18:30:00Z", "2026-10-31T18:15:00Z", "2026-11-01T18:15:00Z");
    }

    private static async Task AssertDayBoundsAsync(string zone, string instant, string dayStart, string dayEnd)
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        (await f.Db.Tenants.SingleAsync()).Timezone = zone;
        var start = DateTimeOffset.Parse(dayStart).UtcDateTime;
        var end = DateTimeOffset.Parse(dayEnd).UtcDateTime;
        foreach (var date in new[] { start.AddSeconds(-1), start, end.AddSeconds(-1), end })
        {
            var invoice = f.MakeSale(1, 100, 100);
            invoice.InvoiceNumber = Guid.NewGuid().ToString("N");
            invoice.InvoiceDate = date;
            f.Db.Add(invoice);
        }
        await f.Db.SaveChangesAsync();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var dashboard = new DashboardService(f.Db, cache, new FixedClock(DateTimeOffset.Parse(instant)));
        var summary = (await dashboard.GetSummaryAsync()).Data!;
        Assert.Equal(200, summary.Kpis.TodaySales);
        Assert.Equal(100, summary.Kpis.YesterdaySales);
        Assert.Equal(180, summary.Kpis.TodayProfit);
        Assert.Equal(2, summary.Kpis.TodayOrdersCount);
        var day = Assert.Single((await dashboard.GetSalesTrendAsync(1)).Data!);
        Assert.Equal(200, day.SalesAmount);
        Assert.Equal(180, day.ProfitAmount);
        if (zone == "Asia/Kathmandu") Assert.Equal(300, summary.Kpis.ThisMonthSales);
    }

    private static CreateProductDto Product(Fixture f) => new()
    {
        Name = "New product", CategoryId = f.Product.CategoryId, CostPrice = 10, SalePrice = 100,
        InitialShowroomQuantity = 5, BarCodes = [new() { BarCode = "NEW", Title = "New", InitialQuantity = 7 }]
    };

    private static CreateSalesInvoiceDto Sale(Fixture f, DateTime date, int quantity = 1) => new()
    {
        InvoiceNumber = Guid.NewGuid().ToString("N"), InvoiceDate = date, BranchId = f.Branch.Id, WarehouseId = f.Show.Id,
        CustomerId = f.Customer.Id, PaidAmount = quantity * 100, PaymentSafeId = f.Safe.Id, PaymentMethod = PaymentMethod.Cash,
        Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = quantity, UnitPrice = 100 }]
    };

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class BeforeSessionSave(Func<Task> action) : SaveChangesInterceptor
    {
        private bool fired;
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken ct = default)
        {
            if (!fired && data.Context!.ChangeTracker.Entries<CashSession>().Any(e => e.State == EntityState.Added))
            {
                fired = true;
                await action();
            }
            return result;
        }
    }
}
