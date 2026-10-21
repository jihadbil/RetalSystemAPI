using ClosedXML.Excel;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Controllers.Warehouses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using RetalSystemAPI.Services.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Catalog.Implementations;
using RetalSystemAPI.Services.Common.Interfaces;
using RetalSystemAPI.Services.Sales.Implementations;
using RetalSystemAPI.Services.Warehouses.Implementations;
using Xunit;
using Fixture = RetalSystemAPI.SecurityTests.ServiceLayerRegressionTests.Fixture;
namespace RetalSystemAPI.SecurityTests;

public sealed class FullAuditRegressionTests
{
    [SqlServerFact]
    public async Task Referenced_warehouse_cannot_be_deleted_or_hide_a_posted_invoice()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var sale = f.MakeSale(1, 100, 100);
        f.Db.Add(sale); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        Assert.NotNull(await f.Uow.SalesInvoices.GetDetailsAsync(sale.Id));
        var deleted = await new WarehouseService(f.Uow, f.Mapper).DeleteAsync(f.Show.Id);
        Assert.False(deleted.IsSuccess); f.Db.ChangeTracker.Clear();
        Assert.True(await f.Db.SalesInvoices.AnyAsync(i => i.Id == sale.Id));
        Assert.NotNull(await f.Uow.SalesInvoices.GetDetailsAsync(sale.Id));
    }
    [SqlServerFact]
    public async Task Storage_return_without_a_barcode_is_rejected_before_posting()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid(); await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var p = new Product { Name = "No barcode", CategoryId = f.Product.CategoryId, SalePrice = 100 };
        f.Db.Add(p); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var service = new SalesReturnService(f.Uow, f.Mapper, new AllPermissions(), f.Posting);
        var result = await service.CreateAsync(new CreateSalesReturnDto
        {
            ReturnNumber = "NO-BARCODE",
            BranchId = f.Branch.Id,
            WarehouseId = f.Storage.Id,
            CustomerId = f.Customer.Id,
            Items = [new() { ProductId = p.Id, Quantity = 1 }]
        });
        Assert.False(result.IsSuccess); f.Db.ChangeTracker.Clear();
        Assert.Equal(0m, await f.Db.Customers.Select(c => c.CurrentBalance).SingleAsync());
        Assert.False(await f.Db.StorgeStocks.AnyAsync(s => s.ProductBarcode.ProductId == p.Id));
        Assert.Empty(await f.Db.SalesReturns.ToListAsync());
        Assert.Empty(await f.Db.CustomerTransactions.ToListAsync());
    }
    [SqlServerFact]
    public async Task Pos_retry_with_the_same_request_returns_the_original_sale()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid(); await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var service = new SalesInvoiceService(f.Uow, f.Mapper, f.Posting, f.Pos, new AllPermissions());
        CreateSalesInvoiceDto Payload() => new()
        {
            InvoiceNumber = "POS-same-client-request",
            BranchId = f.Branch.Id,
            WarehouseId = f.Storage.Id,
            CustomerId = f.Customer.Id,
            PaymentMethod = PaymentMethod.Credit,
            PaidAmount = 0,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1, UnitPrice = 100 }]
        };
        var a = await service.CreatePosAsync(Payload()); Assert.True(a.IsSuccess, a.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        var b = await service.CreatePosAsync(Payload()); Assert.True(b.IsSuccess, b.ErrorMessage);
        Assert.Equal(a.Data!.Id, b.Data!.Id);
        Assert.Equal(1, await f.Db.SalesInvoices.CountAsync());
        Assert.Equal(9, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(100m, await f.Db.Customers.Select(c => c.CurrentBalance).SingleAsync());
    }
    [SqlServerFact]
    public async Task Duplicate_product_names_do_not_break_validation_of_an_unrelated_workbook()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid(); await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        f.Db.Add(new Product { Name = f.Product.Name, CategoryId = f.Product.CategoryId, SalePrice = 20 });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var service = new ProductExcelService(f.Uow, new ConfigurationBuilder().Build(), NullLogger<ProductExcelService>.Instance);
        using var book = Workbook("Unrelated", "NEW-BARCODE", 5);
        var result = await service.ValidateExcelAsync(book);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, await f.Db.Products.CountAsync(p => p.Name == f.Product.Name));
    }
    [SqlServerFact]
    public async Task Excel_explicit_zero_clears_existing_stock()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid(); await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var service = new ProductExcelService(f.Uow, new ConfigurationBuilder().Build(), NullLogger<ProductExcelService>.Instance);
        using var book = Workbook(f.Product.Name, f.Barcode.BarCode, 0);
        var result = await service.ImportProductsFromExcelAsync(book, new ProductExcelImportOptionsDto
        {
            DefaultStorageWarehouseId = f.Storage.Id,
            DefaultShowroomWarehouseId = f.Show.Id
        });
        Assert.True(result.IsSuccess, result.ErrorMessage); f.Db.ChangeTracker.Clear();
        Assert.Equal(0, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
    }
    [SqlServerFact]
    public async Task Return_racing_invoice_cancellation_rolls_back_the_losing_return()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid(); await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var stock = await f.Db.ShowroomStocks.SingleAsync(); stock.Quantity = 1;
        var refund = new Safe { Name = "Second safe", BranchId = f.Branch.Id, CurrentBalance = 1000, IsActive = true };
        f.Db.Add(refund); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var sale = await f.Sales.CreateAsync(new()
        {
            InvoiceNumber = "RACE-SALE",
            BranchId = f.Branch.Id,
            WarehouseId = f.Show.Id,
            PaidAmount = 100,
            PaymentSafeId = f.Safe.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1, UnitPrice = 100 }]
        });
        Assert.True(sale.IsSuccess, sale.ErrorMessage); f.Db.ChangeTracker.Clear();
        var barrier = new StockBarrier();
        await using var returningDb = database.Context(tenant, barrier);
        await using var returningUow = new UnitOfWork(returningDb);
        var pos = new RetalSystemAPI.Services.Accounting.Implementations.PosCashService(returningDb, new ModelIntegrityTests.TestUser());
        var posting = new RetalSystemAPI.Services.Accounting.Implementations.FinancialPostingService(returningUow, pos);
        var service = new SalesReturnService(returningUow, f.Mapper, new AllPermissions(), posting);
        var request = f.SalesReturn(sale.Data!.Id, 1); request.RefundSafeId = refund.Id;
        var pending = service.CreateAsync(request);
        await barrier.Ready.Task.WaitAsync(TimeSpan.FromSeconds(30));
        var cancellation = await f.Sales.UpdateStatusAsync(sale.Data.Id, InvoiceStatus.Cancelled);
        barrier.Release.TrySetResult();
        var returned = await pending;
        Assert.True(cancellation.IsSuccess, cancellation.ErrorMessage); Assert.False(returned.IsSuccess);
        Assert.Equal(RetalSystemAPI.Services.Common.Models.ErrorCodes.ConcurrencyError, returned.ErrorCode);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(InvoiceStatus.Cancelled, await f.Db.SalesInvoices.Select(x => x.Status).SingleAsync());
        Assert.Equal(0, await f.Db.SalesReturns.CountAsync());
        Assert.Equal(1, await f.Db.ShowroomStocks.Select(x => x.Quantity).SingleAsync());
        Assert.Equal(1000m, await f.Db.Safes.Where(x => x.Id == refund.Id).Select(x => x.CurrentBalance).SingleAsync());
    }
    [SqlServerFact]
    public async Task Transfer_update_cannot_post_stock_through_the_create_only_action()
    {
        var action = typeof(StockTransfersController).GetMethod("Update")!;
        var attributes = action.GetCustomAttributes(typeof(HasPermissionAttribute), true);
        Assert.Single(attributes);
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid(); await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var service = new StockTransferService(f.Uow, f.Mapper);
        var created = await service.CreateAsync(new()
        {
            TransferNumber = "PUT-APPROVE",
            FromWarehouseId = f.Storage.Id,
            ToWarehouseId = f.Show.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 3 }]
        });
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var result = await service.UpdateAsync(created.Data!.Id, new() { Status = StockTransferStatus.Completed });
        Assert.False(result.IsSuccess); f.Db.ChangeTracker.Clear();
        Assert.Equal(10, await f.Db.StorgeStocks.Select(x => x.Quantity).SingleAsync());
        Assert.Equal(0, await f.Db.ShowroomStocks.Select(x => x.Quantity).SingleAsync());
    }
    [SqlServerFact]
    public async Task Warehouse_lookup_includes_the_branch_id_required_by_pos()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid(); await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var result = await new WarehouseService(f.Uow, f.Mapper).GetAllAsync(f.Branch.Id);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotEmpty(result.Data!);
        var json = System.Text.Json.JsonSerializer.Serialize(result.Data, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.All(doc.RootElement.EnumerateArray(), w => Assert.Equal(f.Branch.Id, w.GetProperty("branchId").GetGuid()));
    }
    [SqlServerFact]
    public async Task Transfer_overflow_is_rejected_without_changing_either_warehouse()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid(); await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        (await f.Db.ShowroomStocks.SingleAsync()).Quantity = int.MaxValue;
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var service = new StockTransferService(f.Uow, f.Mapper);
        var created = await service.CreateAsync(new()
        {
            TransferNumber = "OVERFLOW",
            FromWarehouseId = f.Storage.Id,
            ToWarehouseId = f.Show.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1 }]
        });
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var completed = await service.UpdateStatusAsync(created.Data!.Id, StockTransferStatus.Completed);
        Assert.False(completed.IsSuccess); f.Db.ChangeTracker.Clear();
        Assert.Equal(10, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(StockTransferStatus.Draft, await f.Db.StockTransfers.Select(t => t.Status).SingleAsync());
        Assert.Equal(int.MaxValue, await f.Db.ShowroomStocks.Select(s => s.Quantity).SingleAsync());
    }
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    public async Task Immediate_stock_adjustment_requires_create_and_approval(bool create, bool approve, bool allowed)
    {
        var claims = new List<Claim>();
        if (create) claims.Add(new(Permissions.ClaimType, Permissions.StockAdjustments.Create));
        if (approve) claims.Add(new(Permissions.ClaimType, Permissions.StockAdjustments.Approve));
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) };
        var context = new AuthorizationFilterContext(new ActionContext(http, new RouteData(), new ActionDescriptor()), []);
        foreach (var filter in typeof(StockAdjustmentsController).GetMethod("Create")!
            .GetCustomAttributes(typeof(HasPermissionAttribute), true).Cast<HasPermissionAttribute>())
            await filter.OnAuthorizationAsync(context);
        if (allowed) Assert.Null(context.Result);
        else Assert.Equal(403, Assert.IsType<ObjectResult>(context.Result).StatusCode);
    }

    [SqlServerFact]
    public async Task Pos_replay_survives_a_price_change_and_rejects_a_changed_payload()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var service = new SalesInvoiceService(f.Uow, f.Mapper, f.Posting, f.Pos, new AllPermissions());
        var request = PosRequest(f, "POS-replay-price");
        var first = await service.CreatePosAsync(request);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        (await f.Db.Products.SingleAsync()).SalePrice = 110;
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var replay = await service.CreatePosAsync(request);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.Equal(first.Data!.Id, replay.Data!.Id);
        request.Items[0].Quantity = 2;
        var changed = await service.CreatePosAsync(request);
        Assert.False(changed.IsSuccess);
        Assert.Equal(ErrorCodes.SalesInvoiceNumberExists, changed.ErrorCode);
        Assert.Equal(1, await f.Db.SalesInvoices.CountAsync());
        Assert.Equal(9, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(100m, await f.Db.Customers.Select(c => c.CurrentBalance).SingleAsync());
    }

    [SqlServerFact]
    public async Task Concurrent_pos_retries_return_one_invoice_and_post_once()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var barrier = new StockBarrier("StorgeStocks");
        await using var otherDb = database.Context(tenant, barrier);
        await using var otherUow = new UnitOfWork(otherDb);
        var pos = new RetalSystemAPI.Services.Accounting.Implementations.PosCashService(otherDb, new ModelIntegrityTests.TestUser());
        var posting = new RetalSystemAPI.Services.Accounting.Implementations.FinancialPostingService(otherUow, pos);
        var firstService = new SalesInvoiceService(otherUow, f.Mapper, posting, pos, new AllPermissions());
        var pending = firstService.CreatePosAsync(PosRequest(f, "POS-concurrent"));
        await barrier.Ready.Task.WaitAsync(TimeSpan.FromSeconds(30));
        ServiceResult<RetalSystemAPI.Models.DTOs.Sales.SalesInvoiceResponseDto> winner;
        try
        {
            winner = await new SalesInvoiceService(f.Uow, f.Mapper, f.Posting, f.Pos, new AllPermissions())
                .CreatePosAsync(PosRequest(f, "POS-concurrent"));
        }
        finally { barrier.Release.TrySetResult(); }
        var replay = await pending.WaitAsync(TimeSpan.FromSeconds(30));
        Assert.True(winner.IsSuccess, winner.ErrorMessage);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.Equal(winner.Data!.Id, replay.Data!.Id);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(1, await f.Db.SalesInvoices.CountAsync());
        Assert.Equal(9, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(100m, await f.Db.Customers.Select(c => c.CurrentBalance).SingleAsync());
    }

    [SqlServerFact]
    public async Task Cancellation_racing_a_return_rolls_back_the_losing_cancellation()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        (await f.Db.ShowroomStocks.SingleAsync()).Quantity = 1;
        var refund = new Safe { Name = "Refund", BranchId = f.Branch.Id, CurrentBalance = 1000, IsActive = true };
        f.Db.Add(refund);
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var sale = await f.Sales.CreateAsync(new()
        {
            InvoiceNumber = "CANCEL-RACE",
            BranchId = f.Branch.Id,
            WarehouseId = f.Show.Id,
            PaidAmount = 100,
            PaymentSafeId = f.Safe.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1, UnitPrice = 100 }]
        });
        Assert.True(sale.IsSuccess, sale.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        var barrier = new StockBarrier();
        await using var otherDb = database.Context(tenant, barrier);
        await using var otherUow = new UnitOfWork(otherDb);
        var pos = new RetalSystemAPI.Services.Accounting.Implementations.PosCashService(otherDb, new ModelIntegrityTests.TestUser());
        var posting = new RetalSystemAPI.Services.Accounting.Implementations.FinancialPostingService(otherUow, pos);
        var cancelling = new SalesInvoiceService(otherUow, f.Mapper, posting, pos)
            .UpdateStatusAsync(sale.Data!.Id, InvoiceStatus.Cancelled);
        await barrier.Ready.Task.WaitAsync(TimeSpan.FromSeconds(30));
        ServiceResult<SalesReturnResponseDto> returned;
        try
        {
            var request = f.SalesReturn(sale.Data.Id, 1);
            request.RefundSafeId = refund.Id;
            returned = await f.Returns.CreateAsync(request);
        }
        finally { barrier.Release.TrySetResult(); }
        var cancellation = await cancelling.WaitAsync(TimeSpan.FromSeconds(30));
        Assert.True(returned.IsSuccess, returned.ErrorMessage);
        Assert.False(cancellation.IsSuccess);
        Assert.Equal(ErrorCodes.ConcurrencyError, cancellation.ErrorCode);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(InvoiceStatus.Paid, await f.Db.SalesInvoices.Select(i => i.Status).SingleAsync());
        Assert.Equal(1, await f.Db.SalesReturns.CountAsync());
        Assert.Equal(1, await f.Db.ShowroomStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(1100m, await f.Db.Safes.Where(s => s.Id == f.Safe.Id).Select(s => s.CurrentBalance).SingleAsync());
        Assert.Equal(900m, await f.Db.Safes.Where(s => s.Id == refund.Id).Select(s => s.CurrentBalance).SingleAsync());
    }

    [SqlServerFact]
    public async Task Product_and_opening_stock_are_rolled_back_together_when_the_second_save_fails()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        var fault = new FailSecondSave();
        await using var f = await Fixture.CreateAsync(database.Context(tenant, fault), tenant);
        fault.Enabled = true;
        await Assert.ThrowsAsync<InjectedSaveException>(() => new ProductService(f.Uow, f.Mapper).CreateAsync(new()
        {
            Name = "Atomic product",
            CategoryId = f.Product.CategoryId,
            SalePrice = 100,
            StorageWarehouseId = f.Storage.Id,
            ShowroomWarehouseId = f.Show.Id,
            InitialShowroomQuantity = 2,
            BarCodes = [new() { BarCode = "ATOMIC-BC", Title = "Atomic", InitialQuantity = 5 }]
        }));
        await using var verify = database.Context(tenant);
        Assert.False(await verify.Products.AnyAsync(p => p.Name == "Atomic product"));
        Assert.False(await verify.ProductBarCodes.AnyAsync(b => b.BarCode == "ATOMIC-BC"));
        Assert.Equal(1, await verify.StorgeStocks.CountAsync());
    }

    [SqlServerFact]
    public async Task Warehouse_and_opening_stock_are_rolled_back_together_when_the_second_save_fails()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        var fault = new FailSecondSave();
        await using var f = await Fixture.CreateAsync(database.Context(tenant, fault), tenant);
        fault.Enabled = true;
        await Assert.ThrowsAsync<InjectedSaveException>(() => new WarehouseService(f.Uow, f.Mapper).CreateAsync(new()
        {
            Name = "Atomic warehouse",
            BranchId = f.Branch.Id,
            Type = WarehouseType.Storge,
            IsActive = true
        }));
        await using var verify = database.Context(tenant);
        Assert.False(await verify.Warehouses.AnyAsync(w => w.Name == "Atomic warehouse"));
        Assert.Equal(1, await verify.StorgeStocks.CountAsync());
    }

    [SqlServerFact]
    public async Task Excel_blank_quantities_and_minimums_preserve_existing_values()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        (await f.Db.StorgeStocks.SingleAsync()).MinStockLevel = 7;
        (await f.Db.ShowroomStocks.SingleAsync()).Quantity = 4;
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        using var input = Workbook(f.Product.Name, f.Barcode.BarCode, 0);
        using var workbook = new XLWorkbook(input);
        workbook.Worksheet(1).Cell(2, 9).Clear();
        workbook.Worksheet(1).Cell(2, 10).Clear();
        using var file = new MemoryStream();
        workbook.SaveAs(file); file.Position = 0;
        var result = await Excel(f).ImportProductsFromExcelAsync(file, new()
        {
            DefaultStorageWarehouseId = f.Storage.Id,
            DefaultShowroomWarehouseId = f.Show.Id
        });
        Assert.True(result.IsSuccess, result.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(10, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(7, await f.Db.StorgeStocks.Select(s => s.MinStockLevel).SingleAsync());
        Assert.Equal(4, await f.Db.ShowroomStocks.Select(s => s.Quantity).SingleAsync());
    }

    [SqlServerFact]
    public async Task Excel_conflict_rolls_back_instead_of_overwriting_a_concurrent_minimum_change()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        var interceptor = new BeforeSave(async () =>
        {
            await using var concurrent = database.Context(tenant);
            (await concurrent.StorgeStocks.SingleAsync()).MinStockLevel = 7;
            await concurrent.SaveChangesAsync();
        });
        await using var f = await Fixture.CreateAsync(database.Context(tenant, interceptor), tenant);
        interceptor.Enabled = true;
        using var file = Workbook(f.Product.Name, f.Barcode.BarCode, 20);
        var result = await Excel(f).ImportProductsFromExcelAsync(file, new()
        {
            DefaultStorageWarehouseId = f.Storage.Id,
            DefaultShowroomWarehouseId = f.Show.Id
        });
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ConcurrencyError, result.ErrorCode);
        await using var verify = database.Context(tenant);
        Assert.Equal(10, await verify.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(7, await verify.StorgeStocks.Select(s => s.MinStockLevel).SingleAsync());
    }

    [SqlServerFact]
    public async Task Duplicate_product_names_require_an_id_or_existing_barcode_when_importing()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var other = new Product { Name = f.Product.Name, CategoryId = f.Product.CategoryId, SalePrice = 20 };
        f.Db.Add(other); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        using var ambiguous = Workbook(f.Product.Name, "UNASSIGNED", 0);
        var validation = await Excel(f).ValidateExcelAsync(ambiguous);
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        Assert.Equal(1, validation.Data!.InvalidRows);
        using var identified = Workbook(f.Product.Name, f.Barcode.BarCode, 0);
        var imported = await Excel(f).ImportProductsFromExcelAsync(identified, new()
        {
            DefaultStorageWarehouseId = f.Storage.Id,
            DefaultShowroomWarehouseId = f.Show.Id
        });
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        Assert.Empty(imported.Data!.FailedRows);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(20m, await f.Db.Products.Where(p => p.Id == other.Id).Select(p => p.SalePrice).SingleAsync());
        Assert.Equal(2, await f.Db.Products.CountAsync());
    }

    [SqlServerFact]
    public async Task Paged_product_search_filters_before_pagination()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        f.Db.Add(new Product { Name = "Needle", CategoryId = f.Product.CategoryId });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = await new ProductService(f.Uow, f.Mapper).GetPagedAsync(1, 1, null, "  Needle  ");
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Data!.TotalCount);
        Assert.Equal("Needle", result.Data.Items.Single().Name);
    }

    private static CreateSalesInvoiceDto PosRequest(Fixture f, string number) => new()
    {
        InvoiceNumber = number,
        BranchId = f.Branch.Id,
        WarehouseId = f.Storage.Id,
        CustomerId = f.Customer.Id,
        PaymentMethod = PaymentMethod.Credit,
        Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1, UnitPrice = 100 }]
    };

    [SqlServerFact]
    public async Task Storage_return_overflow_rolls_back_the_document_and_customer_credit()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        (await f.Db.StorgeStocks.SingleAsync()).Quantity = int.MaxValue;
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var service = new SalesReturnService(f.Uow, f.Mapper, new AllPermissions(), f.Posting);
        var result = await service.CreateAsync(new()
        {
            ReturnNumber = "RETURN-OVERFLOW",
            BranchId = f.Branch.Id,
            WarehouseId = f.Storage.Id,
            CustomerId = f.Customer.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1 }]
        });
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ValidationError, result.ErrorCode);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(int.MaxValue, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(0m, await f.Db.Customers.Select(c => c.CurrentBalance).SingleAsync());
        Assert.Empty(await f.Db.SalesReturns.ToListAsync());
        Assert.Empty(await f.Db.CustomerTransactions.ToListAsync());
    }

    [SqlServerFact]
    public async Task Invoice_cancellation_overflow_preserves_the_posted_invoice_and_safe_balance()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var sale = PosRequest(f, "NORMAL-OVERFLOW");
        sale.PaymentMethod = PaymentMethod.Cash; sale.CustomerId = null;
        sale.PaidAmount = 100; sale.PaymentSafeId = f.Safe.Id;
        var created = await f.Sales.CreateAsync(sale);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        (await f.Db.StorgeStocks.SingleAsync()).Quantity = int.MaxValue;
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = await f.Sales.UpdateStatusAsync(created.Data!.Id, InvoiceStatus.Cancelled);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ValidationError, result.ErrorCode);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(InvoiceStatus.Paid, await f.Db.SalesInvoices.Select(i => i.Status).SingleAsync());
        Assert.Equal(int.MaxValue, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Equal(1100m, await f.Db.Safes.Select(s => s.CurrentBalance).SingleAsync());
        Assert.Equal(1, await f.Db.SafeTransactions.CountAsync());
    }

    private static ProductExcelService Excel(Fixture f) =>
        new(f.Uow, new ConfigurationBuilder().Build(), NullLogger<ProductExcelService>.Instance);

    [SqlServerFact]
    public async Task A_paid_purchase_keeps_its_zero_balance_supplier_available()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var purchase = f.MakePurchase();
        purchase.Status = InvoiceStatus.Paid; purchase.PaidAmount = purchase.TotalAmount; purchase.RemainingAmount = 0;
        purchase.PaymentMethod = PaymentMethod.Cash; purchase.PaymentSafeId = f.Safe.Id;
        f.Db.Add(purchase); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var deleted = await new RetalSystemAPI.Services.Suppliers.Implementations.SupplierService(f.Uow, f.Mapper, f.Pos)
            .DeleteAsync(f.Supplier.Id);
        Assert.False(deleted.IsSuccess);
        f.Db.ChangeTracker.Clear();
        Assert.NotNull(await f.Uow.PurchaseInvoices.GetDetailsAsync(purchase.Id));
        Assert.NotNull(await f.Uow.Suppliers.GetDetailsAsync(f.Supplier.Id));
    }

    [SqlServerFact]
    public async Task Archived_sales_history_keeps_its_zero_balance_customer_available()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var sale = f.MakeSale(1, 100, 100); sale.CustomerId = f.Customer.Id; sale.IsDeleted = true;
        f.Db.Add(sale); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var deleted = await new RetalSystemAPI.Services.Customers.Implementations.CustomerService(f.Uow, f.Mapper, f.Pos)
            .DeleteAsync(f.Customer.Id);
        Assert.False(deleted.IsSuccess);
        Assert.NotNull(await f.Uow.Customers.GetDetailsAsync(f.Customer.Id));
    }

    [SqlServerFact]
    public async Task A_branch_with_no_users_cannot_be_deleted_when_it_has_warehouses_or_documents()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var sale = f.MakeSale(1, 100, 100);
        f.Db.Add(sale); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var deleted = await new RetalSystemAPI.Services.Branch.Implementations.BranchService(f.Uow, f.Mapper)
            .DeleteAsync(f.Branch.Id);
        Assert.False(deleted.IsSuccess);
        f.Db.ChangeTracker.Clear();
        Assert.NotNull(await f.Uow.SalesInvoices.GetDetailsAsync(sale.Id));
        Assert.NotNull(await f.Uow.Warehouses.GetDetailsAsync(f.Storage.Id));
    }

    [Fact]
    public async Task Unused_zero_balance_parties_and_empty_branches_can_still_be_deleted()
    {
        await using var f = await Fixture.CreateAsync();
        Assert.True((await new RetalSystemAPI.Services.Suppliers.Implementations.SupplierService(f.Uow, f.Mapper, f.Pos)
            .DeleteAsync(f.Supplier.Id)).IsSuccess);
        Assert.True((await new RetalSystemAPI.Services.Customers.Implementations.CustomerService(f.Uow, f.Mapper, f.Pos)
            .DeleteAsync(f.Customer.Id)).IsSuccess);
        var branch = new RetalSystemAPI.Models.Branchs.Branch { Name = "Empty branch", IsActive = true };
        f.Db.Add(branch); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        Assert.True((await new RetalSystemAPI.Services.Branch.Implementations.BranchService(f.Uow, f.Mapper)
            .DeleteAsync(branch.Id)).IsSuccess);
    }

    private sealed class InjectedSaveException : Exception { }

    [SqlServerFact]
    public async Task Party_balance_and_activity_filters_apply_before_counting_and_paging()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        f.Db.AddRange(
            new RetalSystemAPI.Models.Customers.Customer { Name = "Active debtor", IsActive = true, CurrentBalance = 100 },
            new RetalSystemAPI.Models.Customers.Customer { Name = "Inactive debtor", IsActive = false, CurrentBalance = 50 },
            new RetalSystemAPI.Models.Suppliers.Supplier { Name = "Active creditor", IsActive = true, CurrentBalance = 100 },
            new RetalSystemAPI.Models.Suppliers.Supplier { Name = "Inactive creditor", IsActive = false, CurrentBalance = 50 });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var customers = new RetalSystemAPI.Services.Customers.Implementations.CustomerService(f.Uow, f.Mapper, f.Pos);
        var first = await customers.GetPagedAsync(1, 1, null, null, null, true);
        var second = await customers.GetPagedAsync(2, 1, null, null, null, true);
        Assert.True(first.IsSuccess); Assert.True(second.IsSuccess);
        Assert.Equal(2, first.Data!.TotalCount);
        Assert.Equal(2, first.Data.TotalPages);
        Assert.Equal("Active debtor", first.Data.Items.Single().Name);
        Assert.Equal("Inactive debtor", second.Data!.Items.Single().Name);
        var suppliers = new RetalSystemAPI.Services.Suppliers.Implementations.SupplierService(f.Uow, f.Mapper, f.Pos);
        var debts = await suppliers.GetPagedAsync(1, 1, null, null, true);
        var active = await suppliers.GetPagedAsync(1, 1, null, true, false);
        Assert.True(debts.IsSuccess); Assert.True(active.IsSuccess);
        Assert.Equal(2, debts.Data!.TotalCount);
        Assert.Equal("Active creditor", debts.Data.Items.Single().Name);
        Assert.Equal(2, active.Data!.TotalCount);
        Assert.All(active.Data.Items, supplier => Assert.True(supplier.IsActive));
    }

    [Fact]
    public async Task Creating_an_inactive_user_returns_inactive_and_prevents_login()
    {
        await using var f = await Fixture.CreateAsync();
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddLogging();
        services.AddSingleton(f.Db);
        services.AddIdentityCore<RetalSystemAPI.Models.ApplicationUser>()
            .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
            .AddEntityFrameworkStores<RetalSystemAPI.DataAccess.Context.AppDbContext>();
        await using var provider = services.BuildServiceProvider();
        var users = provider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<RetalSystemAPI.Models.ApplicationUser>>();
        var roles = provider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        Assert.True((await roles.CreateAsync(new("Cashier"))).Succeeded);
        var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var service = new RetalSystemAPI.Services.Users.Implementations.UserService(users, roles, f.Db,
            new ModelIntegrityTests.TestTenant(f.TenantId), http);
        var result = await service.CreateUserAsync(new()
        {
            UserName = "inactive-cashier",
            Email = "inactive@example.test",
            Password = "Inactive123!",
            BranchId = f.Branch.Id,
            Role = "Cashier",
            IsActive = false
        });
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Data!.IsActive);
        var created = await users.FindByIdAsync(result.Data.Id);
        Assert.NotNull(created);
        Assert.True(await users.IsLockedOutAsync(created));
    }
    [SqlServerFact]
    public async Task Safe_dropdown_filters_match_the_selected_branch_type_and_activity()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var otherBranch = new RetalSystemAPI.Models.Branchs.Branch { Name = "Other branch" };
        var selected = new Safe { Name = "Selected bank", BranchId = f.Branch.Id, Type = SafeType.Bank, IsActive = true };
        f.Db.AddRange(otherBranch, selected,
            new Safe { Name = "Inactive bank", BranchId = f.Branch.Id, Type = SafeType.Bank, IsActive = false },
            new Safe { Name = "Other branch bank", Branch = otherBranch, Type = SafeType.Bank, IsActive = true });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var service = new RetalSystemAPI.Services.Accounting.Implementations.SafeService(f.Uow, f.Mapper, f.Pos);
        var controller = new RetalSystemAPI.Controllers.Accounting.SafesController(service);
        var response = Assert.IsType<OkObjectResult>(await controller.GetAll(f.Branch.Id, SafeType.Bank, true));
        var result = Assert.IsType<RetalSystemAPI.Responses.ApiResponse<IReadOnlyList<RetalSystemAPI.Models.DTOs.Accounting.Safes.SafeSummaryDto>>>(response.Value);
        Assert.True(result.Success);
        Assert.Equal(selected.Id, Assert.Single(result.Data!).Id);
        Assert.Equal(4, (await service.GetAllAsync()).Data!.Count);
    }

    private sealed class FailSecondSave : SaveChangesInterceptor
    {
        public bool Enabled { get; set; }
        private int _saves;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Enabled && ++_saves == 2) throw new InjectedSaveException();
            return ValueTask.FromResult(result);
        }
    }

    private sealed class BeforeSave(Func<Task> action) : SaveChangesInterceptor
    {
        public bool Enabled { get; set; }
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Enabled) { Enabled = false; await action(); }
            return result;
        }
    }

    private sealed class StockBarrier(string table = "ShowroomStocks") : DbCommandInterceptor
    {
        public TaskCompletionSource Ready { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken ct = default)
        {
            if (command.CommandText.Contains($"FROM [{table}]") && !Ready.Task.IsCompleted)
            { Ready.TrySetResult(); await Release.Task.WaitAsync(TimeSpan.FromSeconds(45), ct); }
            return result;
        }
    }
    private static MemoryStream Workbook(string name, string barcode, int qty)
    {
        using var workbook = new XLWorkbook(); var w = workbook.AddWorksheet("Items");
        string[] headers = ["code", "name", "barcode", "category", "unit", "factor", "cost", "price", "showroom qty", "storage qty", "min stock", "description"];
        for (int i = 0; i < headers.Length; i++) w.Cell(1, i + 1).Value = headers[i];
        w.Cell(2, 2).Value = name; w.Cell(2, 3).Value = barcode; w.Cell(2, 4).Value = "Category"; w.Cell(2, 5).Value = "حبة";
        w.Cell(2, 6).Value = 1; w.Cell(2, 7).Value = 10; w.Cell(2, 8).Value = 100; w.Cell(2, 9).Value = 0; w.Cell(2, 10).Value = qty;
        var stream = new MemoryStream(); workbook.SaveAs(stream); stream.Position = 0; return stream;
    }
    private sealed class AllPermissions : ICurrentPermissionService { public bool HasPermission(string permission) => true; }
}
