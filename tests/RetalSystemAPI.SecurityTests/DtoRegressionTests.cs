using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using AutoMapper;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Branch;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.DTOs.MappingProfiles;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Catalog.Implementations;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Purchase.Implementations;
using RetalSystemAPI.Services.Sales.Implementations;
using RetalSystemAPI.Services.Tenant.Implementations;
using RetalSystemAPI.Services.Warehouses.Implementations;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

public sealed class DtoRegressionTests
{
    private static MapperConfiguration Configuration() => new(c => c.AddMaps(typeof(CatalogMappingProfile).Assembly), NullLoggerFactory.Instance);

    [Fact]
    public void All_mapping_profiles_are_valid() => Configuration().AssertConfigurationIsValid();

    [Fact]
    public async Task Date_only_purchase_order_update_preserves_lines_total_and_warehouse()
    {
        await using var f = await Fixture.CreateAsync();
        var request = JsonSerializer.Deserialize<UpdatePurchaseOrderDto>("{\"ExpectedDate\":\"2026-11-01\"}")!;
        var result = await new PurchaseOrderService(f.Uow, f.Mapper, new PurchaseInvoiceService(f.Uow, f.Mapper, null!)).UpdateAsync(f.Order.Id, request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        var stored = await f.Db.PurchaseOrders.Include(o => o.Items).SingleAsync();
        Assert.Single(stored.Items);
        Assert.Equal(10m, stored.TotalAmount);
        Assert.Equal(f.Warehouse.Id, stored.WarehouseId);
        Assert.Equal(new DateTime(2026, 11, 1), stored.ExpectedDate);
    }

    [Fact]
    public async Task Empty_purchase_order_replacement_is_rejected_without_deleting_lines()
    {
        await using var f = await Fixture.CreateAsync();
        var result = await new PurchaseOrderService(f.Uow, f.Mapper, new PurchaseInvoiceService(f.Uow, f.Mapper, null!)).UpdateAsync(f.Order.Id, new() { Items = [] });
        Assert.False(result.IsSuccess);
        Assert.Single(await f.Db.PurchaseOrderItems.ToListAsync());
    }

    [Theory]
    [InlineData(1.9)]
    [InlineData(2147483648)]
    [InlineData(-1)]
    public async Task Invalid_stock_counts_fail_validation_and_do_not_change_stock(decimal quantity)
    {
        await using var f = await Fixture.CreateAsync();
        var request = new SetShowroomStockDto { WarehouseId = f.Warehouse.Id, ProductId = f.Product.Id, Quantity = quantity };
        Assert.False(MvcValid(request));
        var result = await new StockService(f.Uow, f.Mapper).SetShowroomStockAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ValidationError, result.ErrorCode);
        Assert.Equal(10, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Zero_stock_is_valid_and_preserved_exactly()
    {
        await using var f = await Fixture.CreateAsync();
        var result = await new StockService(f.Uow, f.Mapper).SetShowroomStockAsync(new() { WarehouseId = f.Warehouse.Id, ProductId = f.Product.Id, Quantity = 0 });
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0m, result.Data!.Quantity);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Negative_or_duplicate_initial_stock_is_rejected_before_product_is_saved(bool duplicate)
    {
        await using var f = await Fixture.CreateAsync();
        var initial = new CreateShowroomStockQuantityDto { WarehouseId = f.Warehouse.Id, Quantity = duplicate ? 1 : -7 };
        var request = new CreateProductDto { Name = "New", CategoryId = f.Product.CategoryId, ShowroomInitialQuantities = duplicate ? [initial, initial] : [initial] };
        var result = await new ProductService(f.Uow, f.Mapper).CreateAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Single(await f.Db.Products.ToListAsync());
        Assert.Equal(10, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Adjustment_reads_system_quantity_from_server_and_rejects_duplicate_lines()
    {
        await using var f = await Fixture.CreateAsync();
        var service = new StockAdjustmentService(f.Uow, f.Mapper);
        var result = await service.CreateAsync(new()
        {
            AdjustmentNumber = "A", WarehouseId = f.Warehouse.Id,
            Items = [new() { ProductId = f.Product.Id, ActualQuantity = 7, SystemQuantity = 900 }]
        });
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var line = Assert.Single(result.Data!.Items);
        Assert.Equal(10, line.SystemQuantity);
        Assert.Equal(-3, line.DifferenceQuantity);
        Assert.Equal(7, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
        var duplicate = await service.CreateAsync(new()
        {
            AdjustmentNumber = "B", WarehouseId = f.Warehouse.Id,
            Items = [new() { ProductId = f.Product.Id, ActualQuantity = 1 }, new() { ProductId = f.Product.Id, ActualQuantity = 2 }]
        });
        Assert.False(duplicate.IsSuccess);
        Assert.Equal(7, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Draft_update_uses_server_cost_and_never_changes_stock_or_ledger(bool replaceItems)
    {
        await using var f = await Fixture.CreateAsync();
        var request = new UpdateSalesInvoiceDto { TaxAmount = 1, Items = replaceItems ? [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 2, UnitPrice = 10, UnitCost = 999 }] : null };
        var result = await f.Sales.UpdateAsync(f.Invoice.Id, request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(InvoiceStatus.Draft, result.Data!.Status);
        Assert.Equal(replaceItems ? 21m : 11m, result.Data.TotalAmount);
        Assert.Equal(result.Data.TotalAmount, result.Data.RemainingAmount);
        Assert.Equal(2, result.Data.Items[0].UnitCost);
        Assert.Equal(10, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
        Assert.Empty(await f.Db.SafeTransactions.ToListAsync());
        Assert.Empty(await f.Db.CustomerTransactions.ToListAsync());
    }

    [Theory]
    [InlineData(InvoiceStatus.Paid, 0)]
    [InlineData(InvoiceStatus.Draft, 1)]
    public async Task Draft_update_cannot_post_or_collect_payment(InvoiceStatus status, decimal paid)
    {
        await using var f = await Fixture.CreateAsync();
        var result = await f.Sales.UpdateAsync(f.Invoice.Id, new() { Status = status, PaidAmount = paid });
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceStatus.Draft, (await f.Db.SalesInvoices.SingleAsync()).Status);
        Assert.Equal(10, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Removing_a_draft_does_not_add_unposted_stock(bool cancel)
    {
        await using var f = await Fixture.CreateAsync();
        if (cancel) Assert.True((await f.Sales.UpdateStatusAsync(f.Invoice.Id, InvoiceStatus.Cancelled)).IsSuccess);
        else Assert.True((await f.Sales.DeleteAsync(f.Invoice.Id)).IsSuccess);
        Assert.Equal(10, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Null_line_is_rejected_by_mvc_and_direct_service_call()
    {
        await using var f = await Fixture.CreateAsync();
        var request = new CreateSalesInvoiceDto { InvoiceNumber = "null", BranchId = f.Branch.Id, WarehouseId = f.Warehouse.Id, Items = [null!] };
        Assert.False(MvcValid(request));
        var result = await f.Sales.CreateAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ValidationError, result.ErrorCode);
        Assert.Single(await f.Db.SalesInvoices.ToListAsync());
        Assert.False(MvcValid(new UpdatePurchaseOrderDto { Items = [null!] }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public async Task Warehouse_type_must_be_defined(int type)
    {
        await using var f = await Fixture.CreateAsync();
        var request = new CreateWarehouseDto { Name = "bad", BranchId = f.Branch.Id, Type = (WarehouseType)type };
        Assert.False(MvcValid(request));
        Assert.False((await new WarehouseService(f.Uow, f.Mapper).CreateAsync(request)).IsSuccess);
        Assert.Single(await f.Db.Warehouses.ToListAsync());
    }

    [Fact]
    public void Money_text_identifiers_and_tenant_settings_reject_invalid_values()
    {
        Assert.False(MvcValid(new CreateProductDto { Name = new string('x', 201), CategoryId = Guid.NewGuid() }));
        Assert.False(MvcValid(new CreateProductDto { Name = "T", Description = new string('x', 2001), CategoryId = Guid.NewGuid() }));
        Assert.False(MvcValid(new CreateBranchDto { Name = "T", Address = new string('x', 501) }));
        Assert.False(MvcValid(new SetShowroomStockDto { Quantity = 0 }));
        Assert.False(MvcValid(new CreateProductDto { Name = "T", CategoryId = Guid.NewGuid(), SalePrice = 0.00001m }));
        Assert.False(MvcValid(new CreateProductDto { Name = "T", CategoryId = Guid.NewGuid(), SalePrice = 100000000000000m }));
        Assert.False(MvcValid(new UpdateSalesInvoiceDto { TaxAmount = -1 }));
        Assert.False(MvcValid(new RetalSystemAPI.Models.DTOs.Accounting.Pos.OpenCashSessionDto { OpeningFloat = decimal.MaxValue }));
        Assert.False(MvcValid(new CreatePurchaseInvoiceDto { InvoiceNumber = "T", Items = [] }));
        Assert.False(MvcValid(new CreateTenantDto { Name = "T", PhoneNumber = "0912345678", DefaultTaxRate = -1 }));
        Assert.False(MvcValid(new CreateTenantDto { Name = "T", PhoneNumber = "0912345678", DefaultTaxRate = 101 }));
        Assert.False(MvcValid(new CreateTenantDto { Name = "T", PhoneNumber = "0912345678", Timezone = "invalid/zone" }));
        Assert.True(MvcValid(new CreateTenantDto { Name = "T", PhoneNumber = "0912345678", DefaultTaxRate = 15 }));
        // Tenant membership comes from the session; contextual IDs may be omitted from these payloads.
        Assert.Null(DtoValidation.GetError(new RetalSystemAPI.Models.DTOs.Auth.RegisterDto
            { UserName = "user", Email = "user@example.com", Password = "Password1!", BranchId = Guid.NewGuid() }));
        Assert.Null(DtoValidation.GetError(new RetalSystemAPI.Models.DTOs.Users.CreateUserDto
            { UserName = "user", Email = "user@example.com", Password = "Password1!", BranchId = Guid.NewGuid() }));
        var browserAmount = new CreateProductDto { Name = "T", CategoryId = Guid.NewGuid(), SalePrice = 0.30000000000000004m };
        Assert.True(MvcValid(browserAmount));
        Assert.Null(DtoValidation.GetError(browserAmount));
        Assert.Equal(0.3m, browserAmount.SalePrice);
    }

    [Fact]
    public void Mapped_request_strings_fit_database_columns()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=unused;Database=Metadata;Integrated Security=true").Options);
        foreach (var map in Configuration().Internal().GetAllTypeMaps().Where(m => m.SourceType.Namespace?.Contains(".DTOs.") == true))
        {
            var entity = db.Model.FindEntityType(map.DestinationType);
            if (entity is null) continue;
            foreach (var property in map.SourceType.GetProperties().Where(p => p.PropertyType == typeof(string)))
            {
                var max = entity.FindProperty(property.Name)?.GetMaxLength();
                if (max is null || property.GetCustomAttribute<RegularExpressionAttribute>() is not null) continue;
                var bound = property.GetCustomAttribute<MaxLengthAttribute>();
                Assert.True(bound is not null && bound.Length <= max, $"{map.SourceType.Name}.{property.Name} must fit {max} characters");
            }
        }
    }

    [Fact]
    public async Task Tenant_list_and_details_report_the_same_counts()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.Add(new Tenant { Id = f.TenantId, Name = "Tenant", PhoneNumber = "0912345678" });
        await f.Db.SaveChangesAsync();
        var service = new TenantService(f.Uow, f.Db, f.Mapper);
        var list = (await service.GetAllAsync()).Data!;
        var page = (await service.GetPagedAsync(1, 20)).Data!;
        var details = (await service.GetByIdAsync(f.TenantId)).Data!;
        Assert.Equal(1, details.BranchesCount);
        Assert.Equal(details.BranchesCount, Assert.Single(list).BranchesCount);
        Assert.Equal(details.WarehousesCount, Assert.Single(page.Items).WarehousesCount);
    }

    [SqlServerFact]
    public async Task Sql_server_preserves_order_lines_and_saves_authoritative_adjustment_and_draft_cost()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenantId), tenantId);
        var purchase = new PurchaseInvoice
        {
            TenantId = tenantId, InvoiceNumber = "SQL-PURCHASE", InvoiceDate = DateTime.UtcNow, Branch = f.Branch, Warehouse = f.Warehouse,
            Supplier = new RetalSystemAPI.Models.Suppliers.Supplier { TenantId = tenantId, Name = "Supplier" },
            Status = InvoiceStatus.Draft, PaymentMethod = PaymentMethod.Cash, SubTotal = 10, TotalAmount = 10, RemainingAmount = 10,
            Items = [new() { TenantId = tenantId, Product = f.Product, ProductBarCode = f.Barcode, Quantity = 1, UnitPrice = 10, LineTotal = 10 }]
        };
        f.Db.Add(purchase);
        await f.Db.SaveChangesAsync();
        var purchaseResponse = await new PurchaseInvoiceService(f.Uow, f.Mapper, null!).GetByIdAsync(purchase.Id);
        Assert.True(purchaseResponse.IsSuccess, purchaseResponse.ErrorMessage);
        Assert.Equal(f.Barcode.BarCode, purchaseResponse.Data!.Items[0].ProductCode);
        var orderResult = await new PurchaseOrderService(f.Uow, f.Mapper, new PurchaseInvoiceService(f.Uow, f.Mapper, null!)).UpdateAsync(f.Order.Id, new() { ExpectedDate = new DateTime(2026, 11, 1) });
        Assert.True(orderResult.IsSuccess, orderResult.ErrorMessage);
        var adjustment = await new StockAdjustmentService(f.Uow, f.Mapper).CreateAsync(new()
        {
            AdjustmentNumber = "SQL-ADJ", WarehouseId = f.Warehouse.Id,
            Items = [new() { ProductId = f.Product.Id, SystemQuantity = 900, ActualQuantity = 7 }]
        });
        Assert.True(adjustment.IsSuccess, adjustment.ErrorMessage);
        var draft = await f.Sales.UpdateAsync(f.Invoice.Id, new()
        {
            TaxAmount = 0.1234m,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 2, UnitPrice = 10, UnitCost = 999 }]
        });
        Assert.True(draft.IsSuccess, draft.ErrorMessage);
        f.Db.ChangeTracker.Clear();
        Assert.Single((await f.Db.PurchaseOrders.Include(o => o.Items).SingleAsync()).Items);
        Assert.Equal(-3, (await f.Db.StockAdjustmentItems.SingleAsync()).DifferenceQuantity);
        Assert.Equal(7, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
        Assert.Equal(2m, (await f.Db.SalesInvoiceItems.SingleAsync()).UnitCost);
        Assert.Equal(InvoiceStatus.Draft, (await f.Db.SalesInvoices.SingleAsync()).Status);
        Assert.Equal(20.1234m, (await f.Db.SalesInvoices.SingleAsync()).TotalAmount);
    }

    private static bool MvcValid(object request)
    {
        using var provider = new ServiceCollection().AddLogging().AddControllers().Services.BuildServiceProvider();
        var action = new ActionContext { HttpContext = new DefaultHttpContext { RequestServices = provider } };
        provider.GetRequiredService<IObjectModelValidator>().Validate(action, null, "", request);
        return action.ModelState.IsValid;
    }

    private sealed class Fixture : IAsyncDisposable
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public AppDbContext Db { get; }
        public UnitOfWork Uow { get; }
        public IMapper Mapper { get; } = Configuration().CreateMapper();
        public Branch Branch { get; } = new() { Id = Guid.NewGuid(), Name = "Branch" };
        public Warehouse Warehouse { get; }
        public Product Product { get; }
        public ProductBarCode Barcode { get; }
        public PurchaseOrder Order { get; }
        public SalesInvoice Invoice { get; }
        public SalesInvoiceService Sales => new(Uow, Mapper, null!, null!);

        private Fixture(AppDbContext? context = null, Guid? tenantId = null)
        {
            if (tenantId.HasValue) TenantId = tenantId.Value;
            Db = context ?? ModelIntegrityTests.CreateContext(TenantId);
            Uow = new UnitOfWork(Db);
            Warehouse = new() { Id = Guid.NewGuid(), Name = "Showroom", Type = WarehouseType.Show, Branch = Branch };
            Product = new() { Id = Guid.NewGuid(), Name = "Product", AveragePrice = 2, CostPrice = 1, Category = new Category { Id = Guid.NewGuid(), Name = "Category" } };
            Barcode = new() { Id = Guid.NewGuid(), Product = Product, BarCode = "B", Title = "B" };
            Order = new() { Id = Guid.NewGuid(), Branch = Branch, Warehouse = Warehouse, OrderNumber = "PO", TotalAmount = 10, Items = [new() { ProductBarCode = Barcode, Quantity = 1, UnitPrice = 10, LineTotal = 10 }] };
            Invoice = new() { Id = Guid.NewGuid(), Branch = Branch, Warehouse = Warehouse, InvoiceNumber = "DRAFT", Status = InvoiceStatus.Draft, PaymentMethod = PaymentMethod.Cash, SubTotal = 10, TotalAmount = 10, RemainingAmount = 10, Items = [new() { Product = Product, ProductBarCode = Barcode, Quantity = 1, UnitPrice = 10, UnitCost = 2, LineTotal = 10 }] };
        }

        public static async Task<Fixture> CreateAsync(AppDbContext? context = null, Guid? tenantId = null)
        {
            var f = new Fixture(context, tenantId);
            if (context is not null) f.Db.Add(new Tenant { Id = f.TenantId, Name = "Tenant", PhoneNumber = "0912345678" });
            f.Db.AddRange(f.Order, f.Invoice, new ShowroomStock { Warehouse = f.Warehouse, Product = f.Product, Quantity = 10 });
            await f.Db.SaveChangesAsync();
            return f;
        }

        public async ValueTask DisposeAsync() { await Uow.DisposeAsync(); await Db.DisposeAsync(); }
    }
}
