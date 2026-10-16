using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Accounting.Implementations;
using RetalSystemAPI.Services.Catalog.Implementations;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Customers.Implementations;
using RetalSystemAPI.Services.Dashboard.Implementations;
using RetalSystemAPI.Services.Sales.Implementations;
using RetalSystemAPI.Services.Suppliers.Implementations;
using RetalSystemAPI.Services.Warehouses.Implementations;
using Xunit;
using Fixture = RetalSystemAPI.SecurityTests.ServiceLayerRegressionTests.Fixture;

namespace RetalSystemAPI.SecurityTests;

public sealed class ServiceLayerSecondPassTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Draft_creation_is_rejected_before_stock_or_financial_posting(bool sales)
    {
        await using var f = await Fixture.CreateAsync();
        if (sales)
        {
            var request = Sale(f, f.Show.Id);
            request.Status = InvoiceStatus.Draft;
            Assert.False((await f.Sales.CreateAsync(request)).IsSuccess);
        }
        else
        {
            var request = f.Purchase(1, 10);
            request.Status = InvoiceStatus.Draft;
            Assert.False((await f.Purchases.CreateAsync(request)).IsSuccess);
        }
        Assert.Empty(await f.Db.SalesInvoices.ToListAsync());
        Assert.Empty(await f.Db.PurchaseInvoices.ToListAsync());
        Assert.Empty(await f.Db.SafeTransactions.ToListAsync());
        Assert.Empty(await f.Db.CustomerTransactions.ToListAsync());
        Assert.Empty(await f.Db.SupplierTransactions.ToListAsync());
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(0, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_storage_barcode_rejects_create_and_update_without_partial_changes(bool updating)
    {
        await using var f = await Fixture.CreateAsync();
        var product = new Product { Name = "No barcode", CategoryId = f.Product.CategoryId };
        f.Db.Add(product);
        await f.Db.SaveChangesAsync();
        var request = f.Purchase(1, 10);
        if (updating)
        {
            var created = await f.Purchases.CreateAsync(request);
            Assert.True(created.IsSuccess, created.ErrorMessage);
            var changed = Update(f, created.Data!);
            changed.Items = [new() { ProductId = product.Id, Quantity = 1, UnitPrice = 10 }];
            Assert.False((await f.Purchases.UpdateAsync(created.Data!.Id, changed)).IsSuccess);
        }
        else
        {
            request.Items[0].ProductId = product.Id;
            request.Items[0].ProductBarCodeId = null;
            Assert.False((await f.Purchases.CreateAsync(request)).IsSuccess);
        }
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        Assert.Equal(updating ? 1 : 0, await f.Db.PurchaseInvoices.CountAsync());
        Assert.Equal(updating ? 11 : 10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(updating ? 10 : 0, (await f.Db.Suppliers.SingleAsync()).CurrentBalance);
        Assert.Empty(await f.Db.PurchaseInvoiceItems.Where(i => i.ProductId == product.Id).ToListAsync());
    }

    [Fact]
    public async Task Resolved_default_barcode_is_persisted_and_reversed_from_the_same_stock()
    {
        await using var f = await Fixture.CreateAsync();
        var request = f.Purchase(2, 10);
        request.Items[0].ProductBarCodeId = null;
        var created = await f.Purchases.CreateAsync(request);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Equal(f.Barcode.Id, created.Data!.Items.Single().ProductBarCodeId);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(f.Barcode.Id, (await f.Db.PurchaseInvoiceItems.SingleAsync()).ProductBarCodeId);
        Assert.True((await f.Purchases.CancelAsync(created.Data.Id)).IsSuccess);
        Assert.Equal(10, (await f.Db.StorgeStocks.AsNoTracking().SingleAsync()).Quantity);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_purchase_with_returns_cannot_be_rewritten_or_repriced(bool replaceLines)
    {
        await using var f = await Fixture.CreateAsync();
        var created = await f.Purchases.CreateAsync(f.Purchase(5, 10));
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.True((await f.PurchaseReturns.CreateAsync(f.PurchaseReturn(created.Data!.Id, 2))).IsSuccess);
        var request = Update(f, created.Data);
        request.DiscountAmount = 10;
        request.Items = replaceLines ? [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1, UnitPrice = 10 }] : null;
        Assert.False((await f.Purchases.UpdateAsync(created.Data.Id, request)).IsSuccess);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        Assert.Equal(5, (await f.Db.PurchaseInvoiceItems.SingleAsync()).Quantity);
        Assert.Equal(50, (await f.Db.PurchaseInvoices.SingleAsync()).TotalAmount);
        Assert.Equal(30, (await f.Db.Suppliers.SingleAsync()).CurrentBalance);
        Assert.Equal(13, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
    }

    [Theory]
    [InlineData(WarehouseType.Show)]
    [InlineData(WarehouseType.Storge)]
    public async Task Resold_return_cannot_be_deleted_and_stock_money_and_document_remain_consistent(WarehouseType type)
    {
        await using var f = await Fixture.CreateAsync();
        await AssertResoldReturnCannotBeDeleted(f, type);
    }

    private static async Task AssertResoldReturnCannotBeDeleted(Fixture f, WarehouseType type)
    {
        var warehouse = type == WarehouseType.Show ? f.Show : f.Storage;
        var original = f.MakeSale(1, 100, 100);
        original.WarehouseId = warehouse.Id;
        f.Db.Add(original);
        await f.Db.SaveChangesAsync();
        var returned = await f.Returns.CreateAsync(f.SalesReturn(original.Id, 1));
        Assert.True(returned.IsSuccess, returned.ErrorMessage);
        var quantity = type == WarehouseType.Show ? 1 : 11;
        var sold = await f.Sales.CreateAsync(Sale(f, warehouse.Id, quantity));
        Assert.True(sold.IsSuccess, sold.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        var safeBefore = (await f.Db.Safes.AsNoTracking().SingleAsync()).CurrentBalance;
        var deleted = await f.Returns.DeleteAsync(returned.Data!.Id);
        Assert.False(deleted.IsSuccess);
        Assert.Equal(ErrorCodes.InsufficientStock, deleted.ErrorCode);
        Assert.True(await f.Db.SalesReturns.AsNoTracking().AnyAsync(r => r.Id == returned.Data.Id));
        Assert.Equal(safeBefore, (await f.Db.Safes.AsNoTracking().SingleAsync()).CurrentBalance);
        Assert.Equal(0, type == WarehouseType.Show
            ? (await f.Db.ShowroomStocks.AsNoTracking().SingleAsync()).Quantity
            : (await f.Db.StorgeStocks.AsNoTracking().SingleAsync()).Quantity);
    }

    [Theory]
    [InlineData(WarehouseType.Show)]
    [InlineData(WarehouseType.Storge)]
    public async Task Unspent_return_can_be_reversed_with_its_stock_and_refund(WarehouseType type)
    {
        await using var f = await Fixture.CreateAsync();
        var original = f.MakeSale(1, 100, 100);
        original.WarehouseId = type == WarehouseType.Show ? f.Show.Id : f.Storage.Id;
        f.Db.Add(original);
        await f.Db.SaveChangesAsync();
        var returned = await f.Returns.CreateAsync(f.SalesReturn(original.Id, 1));
        Assert.True(returned.IsSuccess, returned.ErrorMessage);
        Assert.True((await f.Returns.DeleteAsync(returned.Data!.Id)).IsSuccess);
        f.Db.ChangeTracker.Clear();
        Assert.Empty(await f.Db.SalesReturns.ToListAsync());
        Assert.Equal(1000, (await f.Db.Safes.SingleAsync()).CurrentBalance);
        Assert.Equal(0, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(0, (await f.Db.Customers.SingleAsync()).CurrentBalance);
    }

    [Fact]
    public async Task Initial_barcode_stock_requires_one_destination_and_is_never_multiplied()
    {
        await using var f = await Fixture.CreateAsync();
        var other = new Warehouse { Name = "Other", BranchId = f.Branch.Id, Type = WarehouseType.Storge, IsActive = true };
        f.Db.Add(other);
        await f.Db.SaveChangesAsync();
        var service = new ProductBarCodeService(f.Uow, f.Mapper);
        var request = new RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode.CreateProductBarCodeDto
        { ProductId = f.Product.Id, BarCode = "NEW", Title = "New", InitialQuantity = 4 };
        Assert.False((await service.AddBarCodeAsync(f.Product.Id, request)).IsSuccess);
        Assert.False(await f.Db.ProductBarCodes.AnyAsync(b => b.BarCode == "NEW"));
        request.WarehouseId = other.Id;
        var created = await service.AddBarCodeAsync(f.Product.Id, request);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        var stocks = await f.Db.StorgeStocks.Where(s => s.ProductBarcodeId == created.Data!.Id).ToListAsync();
        Assert.Equal(2, stocks.Count);
        Assert.Equal(4, stocks.Sum(s => s.Quantity));
        Assert.Equal(4, stocks.Single(s => s.WarehouseId == other.Id).Quantity);
        Assert.Equal(0, stocks.Single(s => s.WarehouseId == f.Storage.Id).Quantity);
    }

    [Fact]
    public async Task Single_storage_accepts_initial_stock_without_an_explicit_destination()
    {
        await using var f = await Fixture.CreateAsync();
        var result = await new ProductBarCodeService(f.Uow, f.Mapper).AddBarCodeAsync(f.Product.Id,
            new() { ProductId = f.Product.Id, BarCode = "NEW", Title = "New", InitialQuantity = 4 });
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(4, (await f.Db.StorgeStocks.AsNoTracking().SingleAsync(s => s.ProductBarcodeId == result.Data!.Id)).Quantity);
    }

    [Fact]
    public async Task Showroom_purchase_can_be_returned_after_transfer_to_storage()
    {
        await using var f = await Fixture.CreateAsync();
        var request = f.Purchase(3, 10);
        request.WarehouseId = f.Show.Id;
        var purchased = await f.Purchases.CreateAsync(request);
        Assert.True(purchased.IsSuccess, purchased.ErrorMessage);
        var transfers = new StockTransferService(f.Uow, f.Mapper);
        var transfer = await transfers.CreateAsync(new()
        {
            TransferNumber = "TO-STORAGE", FromWarehouseId = f.Show.Id, ToWarehouseId = f.Storage.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 3 }]
        });
        Assert.True(transfer.IsSuccess, transfer.ErrorMessage);
        Assert.True((await transfers.UpdateStatusAsync(transfer.Data!.Id, StockTransferStatus.Completed)).IsSuccess);
        var returned = await f.PurchaseReturns.CreateAsync(f.PurchaseReturn(purchased.Data!.Id, 1));
        Assert.True(returned.IsSuccess, returned.ErrorMessage);
        Assert.Equal(f.Storage.Id, returned.Data!.WarehouseId);
        Assert.Equal(0, (await f.Db.ShowroomStocks.AsNoTracking().SingleAsync()).Quantity);
        Assert.Equal(12, (await f.Db.StorgeStocks.AsNoTracking().SingleAsync()).Quantity);
        Assert.Equal(20, (await f.Db.Suppliers.AsNoTracking().SingleAsync()).CurrentBalance);
        var anotherBranch = new Branch { Name = "Other branch" };
        var foreignStorage = new Warehouse { Name = "Other branch storage", Branch = anotherBranch, Type = WarehouseType.Storge };
        f.Db.Add(foreignStorage);
        await f.Db.SaveChangesAsync();
        var invalid = f.PurchaseReturn(purchased.Data.Id, 1);
        invalid.WarehouseId = foreignStorage.Id;
        Assert.False((await f.PurchaseReturns.CreateAsync(invalid)).IsSuccess);
    }

    [Fact]
    public async Task Adjustment_history_is_retained_and_corrections_use_another_document()
    {
        await using var f = await Fixture.CreateAsync();
        var service = new StockAdjustmentService(f.Uow, f.Mapper);
        var created = await service.CreateAsync(new()
        {
            AdjustmentNumber = "ADJ", WarehouseId = f.Storage.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, ActualQuantity = 7 }]
        });
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.False((await service.DeleteAsync(created.Data!.Id)).IsSuccess);
        Assert.Equal(7, (await f.Db.StorgeStocks.AsNoTracking().SingleAsync()).Quantity);
        var corrected = await service.CreateAsync(new()
        {
            AdjustmentNumber = "REV-ADJ", WarehouseId = f.Storage.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, ActualQuantity = 10 }]
        });
        Assert.True(corrected.IsSuccess, corrected.ErrorMessage);
        Assert.Equal(2, await f.Db.StockAdjustments.CountAsync());
        Assert.Equal(10, (await f.Db.StorgeStocks.AsNoTracking().SingleAsync()).Quantity);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Empty_drawer_cannot_be_deleted_while_linked_to_a_terminal_or_shift(bool withSession)
    {
        await using var f = await Fixture.CreateAsync();
        var safe = await f.Db.Safes.SingleAsync();
        safe.Type = SafeType.CashDrawer;
        safe.CurrentBalance = 0;
        var terminal = new PosTerminal { BranchId = f.Branch.Id, DefaultCashDrawerId = safe.Id, Code = "POS", Name = "POS", IsActive = true };
        f.Db.Add(terminal);
        if (withSession) f.Db.Add(new CashSession
        { BranchId = f.Branch.Id, PosTerminal = terminal, CashDrawerId = safe.Id, CashierUserId = "test-user", Status = CashSessionStatus.Open });
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        Assert.False((await new SafeService(f.Uow, f.Mapper, f.Pos).DeleteAsync(safe.Id)).IsSuccess);
        Assert.True(await f.Db.Safes.AnyAsync(s => s.Id == safe.Id));
        if (withSession)
        {
            var id = await f.Db.CashSessions.Select(s => s.Id).SingleAsync();
            Assert.True((await f.Pos.ResolveOpenSessionForPostingAsync(id, f.Branch.Id)).IsSuccess);
        }
    }

    [Fact]
    public async Task Unlinked_empty_safe_can_still_be_deleted()
    {
        await using var f = await Fixture.CreateAsync();
        (await f.Db.Safes.SingleAsync()).CurrentBalance = 0;
        await f.Db.SaveChangesAsync();
        Assert.True((await new SafeService(f.Uow, f.Mapper, f.Pos).DeleteAsync(f.Safe.Id)).IsSuccess);
        Assert.Empty(await f.Db.Safes.ToListAsync());
    }

    [Fact]
    public async Task Cancellation_revalues_remaining_stock_and_reverses_supplier_debt()
    {
        await using var f = await Fixture.CreateAsync();
        await AssertCostReversal(f);
    }

    private static async Task AssertCostReversal(Fixture f)
    {
        var created = await f.Purchases.CreateAsync(f.Purchase(2, 100));
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Equal(25, (await f.Db.Products.AsNoTracking().SingleAsync()).AveragePrice);
        f.Db.ChangeTracker.Clear();
        Assert.True((await f.Purchases.CancelAsync(created.Data!.Id)).IsSuccess);
        f.Db.ChangeTracker.Clear();
        var product = await f.Db.Products.SingleAsync();
        Assert.Equal(10, product.AveragePrice);
        Assert.Equal(10, product.CostPrice);
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(0, (await f.Db.Suppliers.SingleAsync()).CurrentBalance);
    }

    [SqlServerFact]
    public async Task Sql_server_cancel_revalues_cost_and_resold_return_cannot_create_negative_stock()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        await AssertCostReversal(f);
        await AssertResoldReturnCannotBeDeleted(f, WarehouseType.Storge);
    }

    [SqlServerFact]
    public async Task Dashboard_debts_reflect_payments_and_opening_balances_with_branch_ledger_filtering()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var customer = await f.Db.Customers.SingleAsync();
        customer.OpeningBalance = customer.CurrentBalance = 40;
        var supplier = await f.Db.Suppliers.SingleAsync();
        supplier.OpeningBalance = supplier.CurrentBalance = 25;
        (await f.Db.ShowroomStocks.SingleAsync()).Quantity = 10;
        await f.Db.SaveChangesAsync();
        var sale = Sale(f, f.Show.Id);
        sale.PaidAmount = 0;
        sale.CustomerId = customer.Id;
        sale.PaymentMethod = PaymentMethod.Credit;
        Assert.True((await f.Sales.CreateAsync(sale)).IsSuccess);
        Assert.True((await f.Purchases.CreateAsync(f.Purchase(1, 100))).IsSuccess);
        Assert.True((await new CustomerService(f.Uow, f.Mapper, f.Pos).AddPaymentAsync(new()
        { CustomerId = customer.Id, BranchId = f.Branch.Id, SafeId = f.Safe.Id, Amount = 100, PaymentMethod = PaymentMethod.Cash })).IsSuccess);
        Assert.True((await new SupplierService(f.Uow, f.Mapper, f.Pos).AddPaymentAsync(new()
        { SupplierId = supplier.Id, BranchId = f.Branch.Id, SafeId = f.Safe.Id, Amount = 100, PaymentMethod = PaymentMethod.Cash })).IsSuccess);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var dashboard = new DashboardService(f.Db, cache);
        var all = await dashboard.GetSummaryAsync();
        Assert.True(all.IsSuccess, all.ErrorMessage);
        Assert.Equal(40, all.Data!.Kpis.TotalReceivables);
        Assert.Equal(25, all.Data.Kpis.TotalPayables);
        var branch = await dashboard.GetSummaryAsync(f.Branch.Id);
        Assert.True(branch.IsSuccess, branch.ErrorMessage);
        Assert.Equal(0, branch.Data!.Kpis.TotalReceivables);
        Assert.Equal(0, branch.Data.Kpis.TotalPayables);
    }

    [SqlServerFact]
    public async Task Sale_is_rolled_back_if_its_shift_closes_after_validation_before_save()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var sessionId = await SeedPos(f);
        var options = Options(f);
        var pause = new BeforeSave(db => db.ChangeTracker.Entries<SalesInvoice>().Any(e => e.State == EntityState.Added), async () =>
        {
            await using var closing = new AppDbContext(options, new ModelIntegrityTests.TestTenant(tenant));
            var closed = await new PosCashService(closing, new ModelIntegrityTests.TestUser()).CloseSessionAsync("device", new() { CountedClosingBalance = 1000 });
            Assert.True(closed.IsSuccess, closed.ErrorMessage);
        });
        await using var selling = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>(options).AddInterceptors(pause).Options, new ModelIntegrityTests.TestTenant(tenant));
        await using var uow = new UnitOfWork(selling);
        var pos = new PosCashService(selling, new ModelIntegrityTests.TestUser());
        var request = Sale(f, f.Show.Id);
        request.InvoiceNumber = "POS-RACE";
        request.CashSessionId = sessionId;
        var result = await new SalesInvoiceService(uow, f.Mapper, new FinancialPostingService(uow, pos), pos).CreateAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ConcurrencyError, result.ErrorCode);
        f.Db.ChangeTracker.Clear();
        Assert.Empty(await f.Db.SalesInvoices.ToListAsync());
        Assert.Empty(await f.Db.SafeTransactions.ToListAsync());
        Assert.Equal(1000, (await f.Db.Safes.SingleAsync()).CurrentBalance);
        Assert.Equal(CashSessionStatus.Closed, (await f.Db.CashSessions.SingleAsync()).Status);
        Assert.Equal(10, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [SqlServerFact]
    public async Task Stale_shift_closing_is_rejected_and_retry_includes_a_concurrent_sale()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var sessionId = await SeedPos(f);
        var options = Options(f);
        var pause = new BeforeSave(db => db.ChangeTracker.Entries<CashSession>().Any(e => e.State == EntityState.Modified), async () =>
        {
            await using var selling = new AppDbContext(options, new ModelIntegrityTests.TestTenant(tenant));
            await using var uow = new UnitOfWork(selling);
            var pos = new PosCashService(selling, new ModelIntegrityTests.TestUser());
            var request = Sale(f, f.Show.Id);
            request.InvoiceNumber = "POS-RACE";
            request.CashSessionId = sessionId;
            var sold = await new SalesInvoiceService(uow, f.Mapper, new FinancialPostingService(uow, pos), pos).CreateAsync(request);
            Assert.True(sold.IsSuccess, sold.ErrorMessage);
        });
        await using var closing = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>(options).AddInterceptors(pause).Options, new ModelIntegrityTests.TestTenant(tenant));
        var closeService = new PosCashService(closing, new ModelIntegrityTests.TestUser());
        var result = await closeService.CloseSessionAsync("device", new() { CountedClosingBalance = 1000 });
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ConcurrencyError, result.ErrorCode);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(CashSessionStatus.Open, (await f.Db.CashSessions.SingleAsync()).Status);
        Assert.Equal(1100, (await f.Db.Safes.SingleAsync()).CurrentBalance);
        var retry = await closeService.CloseSessionAsync("device", new() { CountedClosingBalance = 1100 });
        Assert.True(retry.IsSuccess, retry.ErrorMessage);
        Assert.Equal(1100, retry.Data!.ExpectedClosingBalance);
        Assert.Equal(CashSessionStatus.Closed, retry.Data.Status);
    }

    private static CreateSalesInvoiceDto Sale(Fixture f, Guid warehouseId, int quantity = 1) => new()
    {
        InvoiceNumber = Guid.NewGuid().ToString("N"), BranchId = f.Branch.Id, WarehouseId = warehouseId,
        PaidAmount = quantity * 100, PaymentSafeId = f.Safe.Id, PaymentMethod = PaymentMethod.Cash,
        Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = quantity, UnitPrice = 100 }]
    };

    private static UpdatePurchaseInvoiceDto Update(Fixture f, PurchaseInvoiceResponseDto invoice) => new()
    {
        InvoiceNumber = invoice.InvoiceNumber, BranchId = f.Branch.Id, WarehouseId = f.Storage.Id,
        SupplierId = f.Supplier.Id, Status = InvoiceStatus.Pending, PaymentMethod = PaymentMethod.Credit
    };

    private static DbContextOptions<AppDbContext> Options(Fixture f) => (DbContextOptions<AppDbContext>)f.Db.GetService<IDbContextOptions>();

    private static async Task<Guid> SeedPos(Fixture f)
    {
        var safe = await f.Db.Safes.SingleAsync();
        safe.Type = SafeType.CashDrawer;
        (await f.Db.ShowroomStocks.SingleAsync()).Quantity = 10;
        var terminal = new PosTerminal { BranchId = f.Branch.Id, DefaultCashDrawerId = safe.Id, Name = "POS", Code = "POS", IsActive = true };
        var session = new CashSession
        { BranchId = f.Branch.Id, PosTerminal = terminal, CashDrawerId = safe.Id, CashierUserId = "test-user", Status = CashSessionStatus.Open, OpeningFloat = 1000, ExpectedClosingBalance = 1000 };
        f.Db.AddRange(session, new PosDeviceCredential
        { PosTerminal = terminal, TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("device"))), ExpiresAt = DateTime.UtcNow.AddDays(1) });
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        return session.Id;
    }

    private sealed class BeforeSave(Func<DbContext, bool> when, Func<Task> action) : SaveChangesInterceptor
    {
        private bool fired;
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data,
            InterceptionResult<int> result, CancellationToken ct = default)
        {
            if (!fired && when(data.Context!))
            {
                fired = true;
                await action();
            }
            return result;
        }
    }
}
