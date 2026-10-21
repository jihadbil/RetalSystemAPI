using System.Reflection;
using System.Diagnostics;
using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RetalSystemAPI.Controllers.Catalog;
using RetalSystemAPI.Controllers.Common;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Common.Models;
using Xunit;
using Xunit.Abstractions;
using Fixture = RetalSystemAPI.SecurityTests.ServiceLayerRegressionTests.Fixture;

namespace RetalSystemAPI.SecurityTests;

public sealed class LookupWorkflowTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Adjustment_quantity_requires_both_creation_and_approval_permissions()
    {
        var filters = typeof(LookupsController).GetMethod(nameof(LookupsController.AdjustmentQuantity))!.GetCustomAttributes<HasPermissionAttribute>().ToList();
        Assert.Equal(2, filters.Count);
        var allowed = FilterContext(Permissions.StockAdjustments.Create);
        ((ClaimsIdentity)allowed.HttpContext.User.Identity!).AddClaim(new(Permissions.ClaimType, Permissions.StockAdjustments.Approve));
        foreach (var filter in filters) { allowed.Result = null; await filter.OnAuthorizationAsync(allowed); Assert.Null(allowed.Result); }
        var denied = FilterContext(Permissions.StockAdjustments.Create);
        var rejected = false;
        foreach (var filter in filters) { denied.Result = null; await filter.OnAuthorizationAsync(denied); rejected |= denied.Result is ObjectResult { StatusCode: 403 }; }
        Assert.True(rejected);
    }

    [SqlServerFact]
    public async Task Adjustment_workflow_reads_quantity_and_cost_without_unrelated_stock_or_product_view_permissions()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var controller = Controller(f, Permissions.StockAdjustments.Create);
        ((ClaimsIdentity)controller.User.Identity!).AddClaim(new(Permissions.ClaimType, Permissions.StockAdjustments.Approve));
        var result = await controller.AdjustmentQuantity(f.Storage.Id, f.Product.Id, f.Barcode.Id, default);
        var quantity = Assert.IsType<ApiResponse<LookupsController.QuantityOption>>(Assert.IsAssignableFrom<ObjectResult>(result).Value).Data!;
        Assert.Equal(10, quantity.Quantity);
        var showroom = await controller.AdjustmentQuantity(f.Show.Id, f.Product.Id, null, default);
        Assert.Equal(0, Assert.IsType<ApiResponse<LookupsController.QuantityOption>>(Assert.IsAssignableFrom<ObjectResult>(showroom).Value).Data!.Quantity);
        Assert.Equal(404, Assert.IsAssignableFrom<ObjectResult>(await controller.AdjustmentQuantity(Guid.NewGuid(), f.Product.Id, f.Barcode.Id, default)).StatusCode);
        Assert.Equal(10, Assert.Single(Data<LookupsController.ProductOption>(await controller.Products(new() { Id = f.Product.Id }, default)).Items).CostPrice);
    }

    [SqlServerFact]
    public async Task POS_selection_filters_account_types_and_branch_and_pages_terminal_metadata()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var bank = new Safe { Name = "Bank", BranchId = f.Branch.Id, Type = SafeType.Bank, CurrentBalance = 1234 };
        var drawer = new Safe { Name = "Drawer", BranchId = f.Branch.Id, Type = SafeType.CashDrawer };
        var foreignBranch = new Branch { Name = "Other branch" };
        f.Db.AddRange(bank, drawer, new Safe { Name = "Other bank", Branch = foreignBranch, Type = SafeType.Bank });
        await f.Db.SaveChangesAsync();
        f.Db.PosTerminals.Add(new PosTerminal { Code = "POS", Name = "Terminal", BranchId = f.Branch.Id, DefaultCashDrawerId = drawer.Id });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var controller = Controller(f, Permissions.Pos.View);
        var banks = Data<LookupsController.Option>(await controller.PosPaymentAccounts(new() { BranchId = f.Branch.Id }, default));
        Assert.Equal(bank.Id, Assert.Single(banks.Items).Id);
        Assert.DoesNotContain("Balance", JsonSerializer.Serialize(banks));
        var drawers = Data<LookupsController.Option>(await controller.CashDrawers(new() { BranchId = f.Branch.Id }, default));
        Assert.Equal(drawer.Id, Assert.Single(drawers.Items).Id);
        var terminals = Data<LookupsController.TerminalOption>(await controller.PosTerminals(new() { BranchId = f.Branch.Id, PageSize = 1 }, default));
        Assert.Equal(drawer.Id, Assert.Single(terminals.Items).DefaultCashDrawerId);
        Assert.Empty(Data<LookupsController.TerminalOption>(await controller.PosTerminals(new() { BranchId = foreignBranch.Id }, default)).Items);
    }

    [SqlServerFact]
    public async Task Catalog_directories_page_rows_and_compute_counts_without_loading_product_graphs()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        var reads = new ReadCounter();
        await using var f = await Fixture.CreateAsync(database.Context(tenant, reads), tenant);
        f.Db.Categories.AddRange(new Category { Name = "Child A", ParentCategoryId = f.Product.CategoryId }, new Category { Name = "Child B", ParentCategoryId = f.Product.CategoryId });
        f.Db.Units.Add(new Unit { Name = "Carton", UnitPackage = 12, Description = "Twelve base units" });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var controller = Controller(f, Permissions.Categories.View);
        reads.Reads = 0;
        var categories = Data<LookupsController.CategoryOption>(await controller.CategoryDirectory(new() { Id = f.Product.CategoryId, PageSize = 1 }, default));
        var category = Assert.Single(categories.Items);
        Assert.Equal(1, category.ProductCount); Assert.Equal(2, category.SubCategoriesCount); Assert.Equal(2, reads.Reads);
        var units = Data<LookupsController.UnitDirectoryOption>(await Controller(f, Permissions.Units.View).UnitDirectory(new(), default));
        Assert.Equal(12, Assert.Single(units.Items).UnitPackage);
    }

    [Theory]
    [InlineData(nameof(LookupsController.Safes), Permissions.Expenses.Create)]
    [InlineData(nameof(LookupsController.Safes), Permissions.Customers.Edit)]
    [InlineData(nameof(LookupsController.Safes), Permissions.PurchaseInvoices.Create)]
    [InlineData(nameof(LookupsController.ExpenseCategories), Permissions.Expenses.Create)]
    [InlineData(nameof(LookupsController.Branches), Permissions.Warehouses.Create)]
    [InlineData(nameof(LookupsController.Branches), Permissions.PurchaseOrders.Create)]
    [InlineData(nameof(LookupsController.Suppliers), Permissions.PurchaseInvoices.Create)]
    [InlineData(nameof(LookupsController.Customers), Permissions.Pos.CreateSale)]
    [InlineData(nameof(LookupsController.Products), Permissions.SalesInvoices.Create)]
    [InlineData(nameof(LookupsController.Products), Permissions.StockTransfers.Create)]
    [InlineData(nameof(LookupsController.Products), Permissions.StockAdjustments.Create)]
    [InlineData(nameof(LookupsController.Warehouses), Permissions.StockTransfers.Create)]
    [InlineData(nameof(LookupsController.Categories), Permissions.Products.Create)]
    [InlineData(nameof(LookupsController.Units), Permissions.Products.Create)]
    public async Task Workflow_permission_grants_only_the_minimal_lookup(string action, string permission)
    {
        var attribute = typeof(LookupsController).GetMethod(action)!.GetCustomAttribute<HasAnyPermissionAttribute>()!;
        var allowed = FilterContext(permission);
        await attribute.OnAuthorizationAsync(allowed);
        Assert.Null(allowed.Result);
        var denied = FilterContext(Permissions.Dashboard.View);
        await attribute.OnAuthorizationAsync(denied);
        Assert.IsType<ObjectResult>(denied.Result);
        Assert.Equal(403, ((ObjectResult)denied.Result!).StatusCode);
        var full = FilterContext(permission);
        await typeof(ProductsController).GetMethod("GetById")!.GetCustomAttribute<HasPermissionAttribute>()!.OnAuthorizationAsync(full);
        Assert.Equal(403, ((ObjectResult)full.Result!).StatusCode);
    }

    [SqlServerFact]
    public async Task Lookups_are_bounded_searchable_tenant_isolated_and_omit_financial_fields()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        var reads = new ReadCounter();
        await using var f = await Fixture.CreateAsync(database.Context(tenant, reads), tenant);
        f.Db.Suppliers.AddRange(Enumerable.Range(1, 5001).Select(i => new Supplier { Name = $"Supplier {i:D4}", CurrentBalance = 123, IsActive = true }));
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var otherTenant = Guid.NewGuid();
        await using (var other = database.Context(otherTenant))
        {
            other.Add(new Tenant { Id = otherTenant, Name = "Other tenant", PhoneNumber = "0912345678" });
            other.Add(new Supplier { Name = "Foreign supplier", IsActive = true });
            await other.SaveChangesAsync();
        }
        var controller = Controller(f, Permissions.PurchaseInvoices.Create);
        var first = Data<LookupsController.Option>(await controller.Suppliers(new() { PageSize = int.MaxValue }, default));
        Assert.Equal(100, first.Items.Count); Assert.Equal(5002, first.TotalCount); Assert.Equal(100, first.PageSize);
        var last = Data<LookupsController.Option>(await controller.Suppliers(new() { Search = "0501" }, default));
        Assert.Equal("Supplier 0501", Assert.Single(last.Items).Name);
        var next = Data<LookupsController.Option>(await controller.Suppliers(new() { PageNumber = 2, PageSize = 100 }, default));
        Assert.Empty(first.Items.Select(i => i.Id).Intersect(next.Items.Select(i => i.Id)));
        var foreign = Data<LookupsController.Option>(await controller.Suppliers(new() { Search = "Foreign" }, default));
        Assert.Empty(foreign.Items);
        var safe = Data<LookupsController.Option>(await controller.Safes(new() { BranchId = f.Branch.Id }, default));
        Assert.Equal(f.Safe.Id, Assert.Single(safe.Items).Id);
        var json = JsonSerializer.Serialize(safe.Items);
        Assert.DoesNotContain("Balance", json); Assert.DoesNotContain("BankAccount", json);
        var foreignBranch = Data<LookupsController.Option>(await controller.Safes(new() { BranchId = Guid.NewGuid() }, default));
        Assert.Empty(foreignBranch.Items);
        var times = new List<double>();
        reads.Reads = 0;
        for (var iteration = 0; iteration < 25; iteration++)
        {
            var start = Stopwatch.GetTimestamp();
            await controller.Suppliers(new() { PageSize = 100, PageNumber = iteration % 5 + 1 }, default);
            times.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds);
        }
        Assert.Equal(50, reads.Reads); // COUNT and bounded SELECT, regardless of entity count.
        var boundedBytes = JsonSerializer.SerializeToUtf8Bytes(first.Items).Length;
        var legacyBytes = JsonSerializer.SerializeToUtf8Bytes(await f.Db.Suppliers.AsNoTracking().ToListAsync()).Length;
        times.Sort();
        output.WriteLine(JsonSerializer.Serialize(new { SyntheticRows = 5002, PageSize = 100, SqlReadsPerPage = 2,
            BoundedBytes = boundedBytes, LegacyAllBytes = legacyBytes, LocalMedianMs = times[12], LocalP95Ms = times[23] }));
    }

    [SqlServerTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Return_creation_reads_only_eligible_tenant_scoped_source_documents(bool sales)
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        Guid sourceId;
        if (sales) { var invoice = f.MakeSale(3, 10, 30); f.Db.Add(invoice); await f.Db.SaveChangesAsync(); sourceId = invoice.Id; }
        else { var invoice = f.MakePurchase(); f.Db.Add(invoice); await f.Db.SaveChangesAsync(); sourceId = invoice.Id; }
        f.Db.ChangeTracker.Clear();
        var permission = sales ? Permissions.SalesReturns.Create : Permissions.PurchaseReturns.Create;
        var method = typeof(LookupsController).GetMethod(sales ? nameof(LookupsController.SalesReturnInvoices) : nameof(LookupsController.PurchaseReturnInvoices))!;
        var filter = method.GetCustomAttribute<HasPermissionAttribute>()!;
        var allowed = FilterContext(permission); await filter.OnAuthorizationAsync(allowed); Assert.Null(allowed.Result);
        var denied = FilterContext(Permissions.Products.View); await filter.OnAuthorizationAsync(denied); Assert.Equal(403, ((ObjectResult)denied.Result!).StatusCode);
        var controller = Controller(f, permission);
        var header = Data<LookupsController.ReturnSource>(sales ? await controller.SalesReturnInvoices(new(), default) : await controller.PurchaseReturnInvoices(new(), default));
        Assert.Empty(Assert.Single(header.Items).Items);
        var selected = Data<LookupsController.ReturnSource>(sales ? await controller.SalesReturnInvoices(new() { Id = sourceId }, default) : await controller.PurchaseReturnInvoices(new() { Id = sourceId }, default));
        var source = Assert.Single(selected.Items);
        Assert.Equal(f.Product.Id, Assert.Single(source.Items).ProductId);
        Assert.DoesNotContain("PaidAmount", JsonSerializer.Serialize(source));
        await using var other = database.Context(Guid.NewGuid());
        var foreignController = new LookupsController(other) { ControllerContext = controller.ControllerContext };
        var foreign = Data<LookupsController.ReturnSource>(sales ? await foreignController.SalesReturnInvoices(new() { Id = sourceId }, default) : await foreignController.PurchaseReturnInvoices(new() { Id = sourceId }, default));
        Assert.Empty(foreign.Items);
    }

    private sealed class ReadCounter : DbCommandInterceptor
    {
        public int Reads { get; set; }
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        { Reads++; return ValueTask.FromResult(result); }
    }

    [SqlServerTheory]
    [InlineData(Permissions.SalesInvoices.Create, false, true)]
    [InlineData(Permissions.PurchaseInvoices.Create, true, true)]
    [InlineData(Permissions.StockTransfers.Create, false, false)]
    [InlineData(Permissions.StockAdjustments.Create, false, false)]
    [InlineData(Permissions.Products.View, true, true)]
    public async Task Product_metadata_enforces_price_visibility_and_loads_barcode_and_unit_contracts(string permission, bool cost, bool sale)
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        f.Db.ProductUnits.Add(new ProductUnit { ProductId = f.Product.Id, Unit = new Unit { Name = "Box" }, ConversionFactor = 3, IsDefault = true });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = Data<LookupsController.ProductOption>(await Controller(f, permission).Products(new() { Barcode = f.Barcode.BarCode }, default));
        var product = Assert.Single(result.Items);
        Assert.Equal(cost ? 10 : 0, product.CostPrice); Assert.Equal(sale ? 100 : 0, product.SalePrice);
        Assert.Equal(3, Assert.Single(product.Units).ConversionFactor);
        Assert.Equal(f.Barcode.Id, Assert.Single(product.BarCodes).Id);
        Assert.Empty(Data<LookupsController.ProductOption>(await Controller(f, permission).Products(new() { Id = Guid.NewGuid() }, default)).Items);
    }

    [SqlServerFact]
    public async Task Allocated_package_prices_post_and_reverse_exactly_ten_without_losing_a_base_unit()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var request = f.Purchase(1, 3.3334m);
        request.Items.Add(new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 2, UnitPrice = 3.3333m });
        var result = await f.Purchases.CreateAsync(request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(10, result.Data!.TotalAmount);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(13, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(10, await f.Db.Suppliers.Select(s => s.CurrentBalance).SingleAsync());
        var returned = await f.PurchaseReturns.CreateAsync(f.PurchaseReturn(result.Data.Id, 3));
        Assert.True(returned.IsSuccess, returned.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(10, returned.Data!.TotalAmount);
        Assert.Equal(10, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(0, await f.Db.Suppliers.Select(s => s.CurrentBalance).SingleAsync());
    }

    private static AuthorizationFilterContext FilterContext(string permission)
    {
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(Permissions.ClaimType, permission)], "test")) };
        return new(new ActionContext(http, new RouteData(), new ActionDescriptor()), []);
    }
    private static LookupsController Controller(Fixture f, string permission) => new(f.Db)
    {
        ControllerContext = new ControllerContext { HttpContext = FilterContext(permission).HttpContext }
    };
    private static PagedResult<T> Data<T>(IActionResult result) => Assert.IsType<ApiResponse<PagedResult<T>>>(Assert.IsAssignableFrom<ObjectResult>(result).Value).Data!;
}
