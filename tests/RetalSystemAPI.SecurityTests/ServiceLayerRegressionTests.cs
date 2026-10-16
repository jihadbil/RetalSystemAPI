using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.DTOs.MappingProfiles;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Accounting.Implementations;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Purchase.Implementations;
using RetalSystemAPI.Services.Sales.Implementations;
using RetalSystemAPI.Services.Users.Implementations;
using RetalSystemAPI.Services.Warehouses.Implementations;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

public sealed class ServiceLayerRegressionTests
{
    [Theory]
    [InlineData("SuperAdmin", false, false)]
    [InlineData("Admin", false, false)]
    [InlineData("User", false, true)]
    [InlineData("SuperAdmin", true, true)]
    public async Task Password_reset_respects_privileged_target_roles(string targetRole, bool callerIsSuperAdmin, bool allowed)
    {
        await using var f = await Fixture.CreateAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton(f.Db);
        services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
        await using var provider = services.BuildServiceProvider();
        var users = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = provider.GetRequiredService<RoleManager<IdentityRole>>();
        Assert.True((await roles.CreateAsync(new(targetRole))).Succeeded);
        if (targetRole != "User") Assert.True((await roles.CreateAsync(new("User"))).Succeeded);
        var target = new ApplicationUser { UserName = "target", TenantId = f.TenantId, BranchId = f.Branch.Id };
        Assert.True((await users.CreateAsync(target, "Before123!")).Succeeded);
        Assert.True((await users.AddToRoleAsync(target, targetRole)).Succeeded);
        var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        http.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new(ClaimTypes.NameIdentifier, "operator"),
            new(ClaimTypes.Role, callerIsSuperAdmin ? "SuperAdmin" : "User"),
            new(Permissions.ClaimType, Permissions.Users.ResetPassword)
        ], "test"));
        var service = new UserService(users, roles, f.Db, new ModelIntegrityTests.TestTenant(f.TenantId), http);
        var result = await service.ResetPasswordAsync(target.Id, new() { NewPassword = "After123!" });
        Assert.Equal(allowed, result.IsSuccess);
        Assert.Equal(allowed, await users.CheckPasswordAsync(target, "After123!"));
        Assert.Equal(!allowed, await users.CheckPasswordAsync(target, "Before123!"));
        if (!allowed)
        {
            Assert.Equal(ErrorCodes.Forbidden, result.ErrorCode);
            var downgrade = await service.UpdateUserAsync(target.Id, new() { BranchId = f.Branch.Id, Role = "User" });
            Assert.False(downgrade.IsSuccess);
            Assert.Equal(ErrorCodes.Forbidden, downgrade.ErrorCode);
            var deletion = await service.DeleteUserAsync(target.Id);
            Assert.False(deletion.IsSuccess);
            Assert.Equal(ErrorCodes.Forbidden, deletion.ErrorCode);
            Assert.True(await users.IsInRoleAsync(target, targetRole));
            Assert.NotNull(await users.FindByIdAsync(target.Id));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Transfers_require_storage_barcodes_at_creation_and_for_legacy_drafts(bool fromStorage)
    {
        await using var f = await Fixture.CreateAsync();
        var from = fromStorage ? f.Storage : f.Show;
        var to = fromStorage ? f.Show : f.Storage;
        var service = new StockTransferService(f.Uow, f.Mapper);
        var rejected = await service.CreateAsync(new()
        {
            TransferNumber = "NEW", FromWarehouseId = from.Id, ToWarehouseId = to.Id,
            Items = [new() { ProductId = f.Product.Id, Quantity = 3 }]
        });
        Assert.False(rejected.IsSuccess);
        var legacy = new StockTransfer
        {
            TransferNumber = "LEGACY", FromWarehouseId = from.Id, ToWarehouseId = to.Id,
            Status = StockTransferStatus.Draft,
            Items = [new() { ProductId = f.Product.Id, Quantity = 3 }]
        };
        f.Db.Add(legacy);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        Assert.False((await service.UpdateStatusAsync(legacy.Id, StockTransferStatus.Completed)).IsSuccess);
        Assert.Equal(10, (await f.Db.StorgeStocks.AsNoTracking().SingleAsync()).Quantity);
        Assert.Equal(0, (await f.Db.ShowroomStocks.AsNoTracking().SingleAsync()).Quantity);
        Assert.Equal(StockTransferStatus.Draft, (await f.Db.StockTransfers.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task Valid_transfer_moves_duplicate_lines_once_and_rejects_a_foreign_product_barcode()
    {
        await using var f = await Fixture.CreateAsync();
        var other = new Product { Name = "Other", CategoryId = f.Product.CategoryId };
        var barcode = new ProductBarCode { Product = other, BarCode = "OTHER", Title = "Other" };
        f.Db.Add(barcode);
        await f.Db.SaveChangesAsync();
        var service = new StockTransferService(f.Uow, f.Mapper);
        Assert.False((await service.CreateAsync(new()
        {
            TransferNumber = "WRONG", FromWarehouseId = f.Storage.Id, ToWarehouseId = f.Show.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = barcode.Id, Quantity = 1 }]
        })).IsSuccess);
        var created = await service.CreateAsync(new()
        {
            TransferNumber = "VALID", FromWarehouseId = f.Storage.Id, ToWarehouseId = f.Show.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1 },
                new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 2 }]
        });
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.True((await service.UpdateStatusAsync(created.Data!.Id, StockTransferStatus.Completed)).IsSuccess);
        Assert.False((await service.UpdateStatusAsync(created.Data.Id, StockTransferStatus.Completed)).IsSuccess);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(7, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(3, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Receipt_posts_an_unpaid_invoice_supplier_debt_and_stock_once_and_can_be_cancelled()
    {
        await using var f = await Fixture.CreateAsync();
        await AssertOrderReceiptAsync(f);
    }

    private static async Task AssertOrderReceiptAsync(Fixture f)
    {
        var order = new PurchaseOrder
        {
            BranchId = f.Branch.Id, WarehouseId = f.Storage.Id, SupplierId = f.Supplier.Id,
            OrderNumber = "PO", Status = PurchaseOrderStatus.Draft,
            Items = [new() { ProductBarCodeId = f.Barcode.Id, Quantity = 2, UnitPrice = 100, LineTotal = 200 }]
        };
        f.Db.Add(order);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var service = new PurchaseOrderService(f.Uow, f.Mapper, f.Purchases);
        var received = await service.UpdateStatusAsync(order.Id, PurchaseOrderStatus.Received);
        Assert.True(received.IsSuccess, received.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        var invoice = await f.Db.PurchaseInvoices.AsNoTracking().SingleAsync();
        Assert.Equal(InvoiceStatus.Pending, invoice.Status);
        Assert.Equal(PaymentMethod.Credit, invoice.PaymentMethod);
        Assert.Equal(0, invoice.PaidAmount);
        Assert.Equal(200, invoice.RemainingAmount);
        Assert.Null(invoice.PaymentSafeId);
        Assert.Equal(200, (await f.Db.Suppliers.AsNoTracking().SingleAsync()).CurrentBalance);
        Assert.Equal(200, await f.Db.SupplierTransactions.SumAsync(t => t.CreditAmount - t.DebitAmount));
        Assert.Empty(await f.Db.SafeTransactions.ToListAsync());
        Assert.Equal(12, (await f.Db.StorgeStocks.AsNoTracking().SingleAsync()).Quantity);
        Assert.Equal(25, (await f.Db.Products.AsNoTracking().SingleAsync()).AveragePrice);
        Assert.False((await service.UpdateStatusAsync(order.Id, PurchaseOrderStatus.Received)).IsSuccess);
        Assert.Single(await f.Db.PurchaseInvoices.ToListAsync());
        var cancelled = await f.Purchases.CancelAsync(invoice.Id);
        Assert.True(cancelled.IsSuccess, cancelled.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(0, (await f.Db.Suppliers.SingleAsync()).CurrentBalance);
        Assert.Equal(0, await f.Db.SupplierTransactions.SumAsync(t => t.CreditAmount - t.DebitAmount));
    }

    [Fact]
    public async Task Invalid_receipt_keeps_order_and_inventory_unposted()
    {
        await using var f = await Fixture.CreateAsync();
        var supplier = await f.Db.Suppliers.SingleAsync();
        supplier.IsActive = false;
        var order = new PurchaseOrder
        {
            BranchId = f.Branch.Id, WarehouseId = f.Storage.Id, SupplierId = supplier.Id,
            OrderNumber = "PO", Status = PurchaseOrderStatus.Draft,
            Items = [new() { ProductBarCodeId = f.Barcode.Id, Quantity = 2, UnitPrice = 10 }]
        };
        f.Db.Add(order);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var result = await new PurchaseOrderService(f.Uow, f.Mapper, f.Purchases).UpdateStatusAsync(order.Id, PurchaseOrderStatus.Received);
        Assert.False(result.IsSuccess);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        Assert.Equal(PurchaseOrderStatus.Draft, (await f.Db.PurchaseOrders.SingleAsync()).Status);
        Assert.Empty(await f.Db.PurchaseInvoices.ToListAsync());
        Assert.Empty(await f.Db.SupplierTransactions.ToListAsync());
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Discounted_sales_return_refunds_only_the_net_amount_without_customer_credit()
    {
        await using var f = await Fixture.CreateAsync();
        var invoice = f.MakeSale(1, 100, 80);
        invoice.Items.Single().DiscountAmount = 20;
        invoice.Items.Single().LineTotal = 80;
        f.Db.Add(invoice);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var result = await f.Returns.CreateAsync(f.SalesReturn(invoice.Id, 1));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(80, result.Data!.TotalAmount);
        Assert.Equal(80, result.Data.RefundedAmount);
        Assert.Equal(0, (await f.Db.Customers.AsNoTracking().SingleAsync()).CurrentBalance);
        Assert.Equal(920, (await f.Db.Safes.AsNoTracking().SingleAsync()).CurrentBalance);
    }

    [Fact]
    public async Task Partial_duplicate_returns_allocate_invoice_discount_tax_and_rounding_exactly()
    {
        await using var f = await Fixture.CreateAsync();
        var invoice = f.MakeSale(3, 1, 1);
        invoice.DiscountAmount = 2.1234m;
        invoice.TaxAmount = 0.1234m;
        f.Db.Add(invoice);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var first = await f.Returns.CreateAsync(f.SalesReturn(invoice.Id, 1));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(0.3333m, first.Data!.TotalAmount);
        var remaining = f.SalesReturn(invoice.Id, 1);
        remaining.Items.Add(new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1 });
        var second = await f.Returns.CreateAsync(remaining);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(0.6667m, second.Data!.TotalAmount);
        Assert.Equal(1, await f.Db.SalesReturns.SumAsync(r => r.TotalAmount));
        Assert.Equal(1, await f.Db.SalesReturns.SumAsync(r => r.RefundedAmount));
        Assert.False((await f.Returns.CreateAsync(f.SalesReturn(invoice.Id, 1))).IsSuccess);
    }

    [Fact]
    public async Task Sale_with_a_return_cannot_be_cancelled_or_reverse_stock_and_money_again()
    {
        await using var f = await Fixture.CreateAsync();
        var invoice = f.MakeSale(5, 10, 50);
        f.Db.Add(invoice);
        await f.Db.SaveChangesAsync();
        var returned = await f.Returns.CreateAsync(f.SalesReturn(invoice.Id, 2));
        Assert.True(returned.IsSuccess, returned.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        var result = await f.Sales.UpdateStatusAsync(invoice.Id, InvoiceStatus.Cancelled);
        Assert.False(result.IsSuccess);
        Assert.Equal(2, (await f.Db.ShowroomStocks.AsNoTracking().SingleAsync()).Quantity);
        Assert.Equal(980, (await f.Db.Safes.AsNoTracking().SingleAsync()).CurrentBalance);
        Assert.Equal(InvoiceStatus.Paid, (await f.Db.SalesInvoices.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task Purchase_return_uses_selected_storage_for_both_stock_and_document()
    {
        await using var f = await Fixture.CreateAsync();
        var other = new Warehouse { Name = "Other", BranchId = f.Branch.Id, Type = WarehouseType.Storge };
        var invoice = f.MakePurchase();
        f.Db.AddRange(invoice, new StorgeStock { Warehouse = other, ProductBarcodeId = f.Barcode.Id, Quantity = 10 });
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var request = f.PurchaseReturn(invoice.Id, 2);
        request.WarehouseId = other.Id;
        var result = await f.PurchaseReturns.CreateAsync(request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(other.Id, result.Data!.WarehouseId);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync(s => s.WarehouseId == f.Storage.Id)).Quantity);
        Assert.Equal(8, (await f.Db.StorgeStocks.SingleAsync(s => s.WarehouseId == other.Id)).Quantity);
        Assert.False((await f.Purchases.CancelAsync(invoice.Id)).IsSuccess);
    }

    [Fact]
    public async Task Purchase_breakdown_returns_use_barcode_quantities_prices_and_aggregate_duplicate_limits()
    {
        await using var f = await Fixture.CreateAsync();
        var secondBarcode = new ProductBarCode { ProductId = f.Product.Id, BarCode = "SECOND", Title = "Second" };
        f.Db.AddRange(secondBarcode, new StorgeStock { WarehouseId = f.Storage.Id, ProductBarcode = secondBarcode, Quantity = 10 });
        var invoice = f.MakePurchase();
        invoice.TotalAmount = invoice.SubTotal = 80;
        var line = invoice.Items.Single();
        line.ProductBarCodeId = null;
        line.Quantity = 5;
        line.UnitPrice = 16;
        line.LineTotal = 80;
        line.Breakdowns = [new() { ProductBarCodeId = f.Barcode.Id, PackageQuantity = 1, UnitsPerPackage = 2, Quantity = 2, UnitPrice = 10 },
            new() { ProductBarCode = secondBarcode, PackageQuantity = 1, UnitsPerPackage = 3, Quantity = 3, UnitPrice = 20 }];
        f.Db.Add(invoice);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var request = f.PurchaseReturn(invoice.Id, 1);
        request.Items.Add(new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1 });
        var first = await f.PurchaseReturns.CreateAsync(request);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(20, first.Data!.TotalAmount);
        Assert.False((await f.PurchaseReturns.CreateAsync(f.PurchaseReturn(invoice.Id, 1))).IsSuccess);
        var second = f.PurchaseReturn(invoice.Id, 3);
        second.Items[0].ProductBarCodeId = secondBarcode.Id;
        var result = await f.PurchaseReturns.CreateAsync(second);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(60, result.Data!.TotalAmount);
        Assert.Equal(80, await f.Db.PurchaseReturns.SumAsync(r => r.TotalAmount));
    }

    [Fact]
    public async Task Weighted_cost_uses_all_warehouses_and_combines_repeated_product_lines()
    {
        await using var f = await Fixture.CreateAsync();
        (await f.Db.StorgeStocks.SingleAsync()).Quantity = 60;
        (await f.Db.ShowroomStocks.SingleAsync()).Quantity = 40;
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var request = f.Purchase(1, 100);
        var result = await f.Purchases.CreateAsync(request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(10.8911m, (await f.Db.Products.AsNoTracking().SingleAsync()).AveragePrice);
        var duplicate = f.Purchase(1, 10);
        duplicate.Items.Add(new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 9, UnitPrice = 20 });
        var second = await f.Purchases.CreateAsync(duplicate);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(decimal.Round((101 * 10.8911m + 190) / 111, 4), (await f.Db.Products.AsNoTracking().SingleAsync()).AveragePrice);
    }

    [Fact]
    public async Task Purchase_cancellation_uses_one_tracked_graph_and_reverses_supplier_and_safe_entries()
    {
        await using var f = await Fixture.CreateAsync();
        var request = f.Purchase(2, 10);
        request.PaidAmount = 20;
        request.PaymentMethod = PaymentMethod.Cash;
        request.PaymentSafeId = f.Safe.Id;
        var created = await f.Purchases.CreateAsync(request);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        var cancelled = await f.Purchases.CancelAsync(created.Data!.Id);
        Assert.True(cancelled.IsSuccess, cancelled.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(1000, (await f.Db.Safes.SingleAsync()).CurrentBalance);
        Assert.Equal(0, (await f.Db.Suppliers.SingleAsync()).CurrentBalance);
        Assert.Equal(0, await f.Db.SupplierTransactions.SumAsync(t => t.CreditAmount - t.DebitAmount));
        Assert.Equal(0, await f.Db.SafeTransactions.SumAsync(t => t.Direction == TransactionDirection.Inbound ? t.Amount : -t.Amount));
        Assert.False((await f.Purchases.CancelAsync(created.Data.Id)).IsSuccess);
    }

    [Theory]
    [InlineData(CashSessionStatus.Open, true)]
    [InlineData(CashSessionStatus.PendingApproval, false)]
    public async Task Cash_sale_cancellation_uses_current_session_and_respects_frozen_drawers(CashSessionStatus status, bool allowed)
    {
        await using var f = await Fixture.CreateAsync();
        var safe = await f.Db.Safes.SingleAsync();
        safe.Type = SafeType.CashDrawer;
        safe.CurrentBalance = 1100;
        var terminal = new PosTerminal { BranchId = f.Branch.Id, DefaultCashDrawerId = safe.Id, Code = "POS", Name = "POS" };
        var session = new CashSession { BranchId = f.Branch.Id, PosTerminal = terminal, CashDrawerId = safe.Id, CashierUserId = "test-user", Status = status, OpeningFloat = 1000 };
        var invoice = f.MakeSale(1, 100, 100);
        invoice.CashSession = session;
        f.Db.AddRange(invoice, new SafeTransaction
        {
            TransactionNumber = "PAYMENT", SafeId = safe.Id, CashSession = session,
            Direction = TransactionDirection.Inbound, Amount = 100, BalanceAfter = 1100,
            DocumentType = FinancialDocumentType.SalesInvoice, DocumentId = invoice.Id
        });
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var result = await f.Sales.UpdateStatusAsync(invoice.Id, InvoiceStatus.Cancelled);
        Assert.Equal(allowed, result.IsSuccess);
        var movements = await f.Db.SafeTransactions.AsNoTracking().ToListAsync();
        Assert.Equal(allowed ? 2 : 1, movements.Count);
        Assert.All(movements, t => Assert.Equal(session.Id, t.CashSessionId));
        var expected = 1000 + movements.Sum(t => t.Direction == TransactionDirection.Inbound ? t.Amount : -t.Amount);
        Assert.Equal(expected, (await f.Db.Safes.AsNoTracking().SingleAsync()).CurrentBalance);
    }

    [Fact]
    public async Task Cancellation_after_original_shift_closed_posts_to_the_new_open_shift()
    {
        await using var f = await Fixture.CreateAsync();
        var safe = await f.Db.Safes.SingleAsync();
        safe.Type = SafeType.CashDrawer;
        safe.CurrentBalance = 1100;
        var terminal = new PosTerminal { BranchId = f.Branch.Id, DefaultCashDrawerId = safe.Id, Code = "POS", Name = "POS" };
        var oldSession = new CashSession
        {
            BranchId = f.Branch.Id, PosTerminal = terminal, CashDrawerId = safe.Id, CashierUserId = "test-user",
            Status = CashSessionStatus.Closed, OpeningFloat = 1000, ExpectedClosingBalance = 1100,
            CountedClosingBalance = 1100, ClosedAt = DateTime.UtcNow
        };
        var currentSession = new CashSession
        {
            BranchId = f.Branch.Id, PosTerminal = terminal, CashDrawerId = safe.Id, CashierUserId = "test-user",
            Status = CashSessionStatus.Open, OpeningFloat = 1100
        };
        var invoice = f.MakeSale(1, 100, 100);
        invoice.CashSession = oldSession;
        f.Db.AddRange(invoice, currentSession, new SafeTransaction
        {
            TransactionNumber = "OLD-PAYMENT", SafeId = safe.Id, CashSession = oldSession,
            Direction = TransactionDirection.Inbound, Amount = 100, BalanceAfter = 1100,
            DocumentType = FinancialDocumentType.SalesInvoice, DocumentId = invoice.Id
        });
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var result = await f.Sales.UpdateStatusAsync(invoice.Id, InvoiceStatus.Cancelled);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var reversal = await f.Db.SafeTransactions.AsNoTracking().SingleAsync(t => t.Direction == TransactionDirection.Outbound);
        Assert.Equal(currentSession.Id, reversal.CashSessionId);
        Assert.Equal(1000, (await f.Db.Safes.AsNoTracking().SingleAsync()).CurrentBalance);
        Assert.Equal(1100, (await f.Db.CashSessions.AsNoTracking().SingleAsync(s => s.Id == oldSession.Id)).ExpectedClosingBalance);
    }

    [SqlServerFact]
    public async Task Sql_server_receipt_posts_stock_weighted_cost_and_ledger_and_can_be_cancelled()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenantId), tenantId);
        await AssertOrderReceiptAsync(f);
    }

    internal sealed class Fixture : IAsyncDisposable
    {
        public Guid TenantId { get; private set; } = Guid.NewGuid();
        public AppDbContext Db { get; private set; } = null!;
        public UnitOfWork Uow { get; private set; } = null!;
        public IMapper Mapper { get; } = new MapperConfiguration(c => c.AddMaps(typeof(CatalogMappingProfile).Assembly), NullLoggerFactory.Instance).CreateMapper();
        public Branch Branch { get; } = new() { Name = "Branch", IsActive = true };
        public Warehouse Storage { get; private set; } = null!;
        public Warehouse Show { get; private set; } = null!;
        public Product Product { get; private set; } = null!;
        public ProductBarCode Barcode { get; private set; } = null!;
        public Customer Customer { get; } = new() { Name = "Customer", IsActive = true };
        public Supplier Supplier { get; } = new() { Name = "Supplier", IsActive = true };
        public Safe Safe { get; private set; } = null!;
        public PosCashService Pos => new(Db, new ModelIntegrityTests.TestUser());
        public FinancialPostingService Posting => new(Uow, Pos);
        public PurchaseInvoiceService Purchases => new(Uow, Mapper, Posting);
        public PurchaseReturnService PurchaseReturns => new(Uow, Mapper, Posting);
        public SalesInvoiceService Sales => new(Uow, Mapper, Posting, Pos);
        public SalesReturnService Returns => new(Uow, Mapper, null!, Posting);

        public static async Task<Fixture> CreateAsync(AppDbContext? context = null, Guid? tenantId = null)
        {
            var f = new Fixture();
            if (tenantId.HasValue) f.TenantId = tenantId.Value;
            f.Db = context ?? ModelIntegrityTests.CreateContext(f.TenantId);
            f.Uow = new(f.Db);
            f.Storage = new() { Name = "Storage", Branch = f.Branch, Type = WarehouseType.Storge, IsActive = true };
            f.Show = new() { Name = "Show", Branch = f.Branch, Type = WarehouseType.Show, IsActive = true };
            f.Product = new() { Name = "Product", Category = new() { Name = "Category" }, CostPrice = 10, AveragePrice = 10, SalePrice = 100 };
            f.Barcode = new() { Product = f.Product, BarCode = "BARCODE", Title = "Barcode" };
            f.Safe = new() { Branch = f.Branch, Name = "Safe", CurrentBalance = 1000, IsActive = true };
            if (context is not null) f.Db.Add(new Tenant { Id = f.TenantId, Name = "Tenant", PhoneNumber = "0912345678" });
            f.Db.AddRange(f.Branch, f.Storage, f.Show, f.Product, f.Barcode, f.Customer, f.Supplier, f.Safe,
                new StorgeStock { Warehouse = f.Storage, ProductBarcode = f.Barcode, Quantity = 10 },
                new ShowroomStock { Warehouse = f.Show, Product = f.Product, Quantity = 0 });
            await f.Db.SaveChangesAsync();
            f.Db.ChangeTracker.Clear();
            return f;
        }

        public SalesInvoice MakeSale(int quantity, decimal price, decimal total) => new()
        {
            InvoiceNumber = "SALE", BranchId = Branch.Id, WarehouseId = Show.Id, CustomerId = Customer.Id,
            Status = InvoiceStatus.Paid, PaymentMethod = PaymentMethod.Cash, SubTotal = quantity * price,
            TotalAmount = total, PaidAmount = total, PaymentSafeId = Safe.Id,
            Items = [new() { ProductId = Product.Id, ProductBarCodeId = Barcode.Id, Quantity = quantity, UnitPrice = price, UnitCost = 10, LineTotal = quantity * price }]
        };

        public PurchaseInvoice MakePurchase() => new()
        {
            InvoiceNumber = "PURCHASE", BranchId = Branch.Id, WarehouseId = Storage.Id, SupplierId = Supplier.Id,
            Status = InvoiceStatus.Pending, PaymentMethod = PaymentMethod.Credit, SubTotal = 50, TotalAmount = 50, RemainingAmount = 50,
            Items = [new() { ProductId = Product.Id, ProductBarCodeId = Barcode.Id, Quantity = 5, UnitPrice = 10, LineTotal = 50 }]
        };

        public CreatePurchaseInvoiceDto Purchase(decimal quantity, decimal price) => new()
        {
            InvoiceNumber = Guid.NewGuid().ToString("N"), SupplierId = Supplier.Id, BranchId = Branch.Id,
            WarehouseId = Storage.Id, PaymentMethod = PaymentMethod.Credit,
            Items = [new() { ProductId = Product.Id, ProductBarCodeId = Barcode.Id, Quantity = quantity, UnitPrice = price }]
        };

        public CreateSalesReturnDto SalesReturn(Guid invoiceId, int quantity) => new()
        {
            ReturnNumber = Guid.NewGuid().ToString("N"), OriginalInvoiceId = invoiceId, BranchId = Branch.Id,
            WarehouseId = Show.Id, Items = [new() { ProductId = Product.Id, ProductBarCodeId = Barcode.Id, Quantity = quantity }]
        };

        public CreatePurchaseReturnDto PurchaseReturn(Guid invoiceId, decimal quantity) => new()
        {
            ReturnNumber = Guid.NewGuid().ToString("N"), PurchaseInvoiceId = invoiceId, SupplierId = Supplier.Id,
            BranchId = Branch.Id, WarehouseId = Storage.Id, PaymentMethod = PaymentMethod.Credit,
            Items = [new() { ProductId = Product.Id, ProductBarCodeId = Barcode.Id, Quantity = quantity }]
        };

        public ValueTask DisposeAsync() => Uow.DisposeAsync();
    }
}
