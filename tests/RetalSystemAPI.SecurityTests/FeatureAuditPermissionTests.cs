using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.Controllers.Sales;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Implementations;
using RetalSystemAPI.Services.Sales.Implementations;
using Xunit;
using Fixture = RetalSystemAPI.SecurityTests.ServiceLayerRegressionTests.Fixture;

namespace RetalSystemAPI.SecurityTests;

public sealed class FeatureAuditPermissionTests
{
    private static HttpContext Context(params string[] permissions) => new DefaultHttpContext
    {
        User = new ClaimsPrincipal(new ClaimsIdentity(permissions.Select(p => new Claim(Permissions.ClaimType, p)), "test"))
    };

    private static SalesInvoiceService Service(Fixture f, params string[] permissions) =>
        new(f.Uow, f.Mapper, f.Posting, f.Pos, new CurrentPermissionService(new HttpContextAccessor { HttpContext = Context(permissions) }));

    private static CreateSalesInvoiceDto Request(Fixture f) => new()
    {
        InvoiceNumber = "CLIENT-NUMBER", BranchId = f.Branch.Id, WarehouseId = f.Storage.Id,
        CustomerId = f.Customer.Id, PaymentMethod = PaymentMethod.Credit,
        Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1, UnitPrice = 100 }]
    };

    [Theory]
    [InlineData(false, 0, 0, 100)]
    [InlineData(true, 10, 0, 100)]
    [InlineData(true, 0, 10, 100)]
    [InlineData(true, 0, 0, 90)]
    public async Task Pos_rejects_missing_sale_permission_discounts_and_hidden_price_reductions(bool canSell, decimal headerDiscount, decimal lineDiscount, decimal price)
    {
        await using var f = await Fixture.CreateAsync();
        var request = Request(f);
        request.DiscountAmount = headerDiscount;
        request.Items[0].DiscountAmount = lineDiscount;
        request.Items[0].UnitPrice = price;
        var result = await Service(f, canSell ? [Permissions.Pos.CreateSale] : []).CreatePosAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Empty(await f.Db.SalesInvoices.ToListAsync());
        Assert.Equal(10, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
        Assert.Empty(await f.Db.CustomerTransactions.ToListAsync());
    }

    [Theory]
    [InlineData(SafeType.CashDrawer)]
    [InlineData(SafeType.CashBox)]
    public async Task Pos_card_payment_cannot_enter_a_cash_safe(SafeType type)
    {
        await using var f = await Fixture.CreateAsync();
        var safe = await f.Db.Safes.SingleAsync();
        safe.Type = type;
        await f.Db.SaveChangesAsync();
        var request = Request(f);
        request.PaymentMethod = PaymentMethod.CreditCard;
        request.PaidAmount = 100;
        request.PaymentSafeId = safe.Id;
        Assert.False((await Service(f, Permissions.Pos.CreateSale).CreatePosAsync(request)).IsSuccess);
        Assert.Empty(await f.Db.SalesInvoices.ToListAsync());
        Assert.Equal(1000, safe.CurrentBalance);
    }

    [Fact]
    public async Task Pos_cash_payment_requires_an_open_session_even_with_a_client_invoice_number()
    {
        await using var f = await Fixture.CreateAsync();
        var request = Request(f);
        request.PaymentMethod = PaymentMethod.Cash;
        request.PaymentSafeId = f.Safe.Id;
        request.PaidAmount = 100;
        Assert.False((await Service(f, Permissions.Pos.CreateSale).CreatePosAsync(request)).IsSuccess);
        Assert.Empty(await f.Db.SalesInvoices.ToListAsync());
    }

    [SqlServerFact]
    public async Task Pos_sale_and_authorized_discount_post_to_the_selected_bank_account()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var bank = new Safe { BranchId = f.Branch.Id, Name = "Card clearing", Type = SafeType.Bank, IsActive = true };
        f.Db.Add(bank);
        await f.Db.SaveChangesAsync();
        var request = Request(f);
        request.PaymentMethod = PaymentMethod.CreditCard;
        request.DiscountAmount = 10;
        request.PaidAmount = 90;
        request.PaymentSafeId = bank.Id;
        var result = await Service(f, Permissions.Pos.CreateSale, Permissions.Pos.ApplyDiscount).CreatePosAsync(request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.StartsWith("POS-", result.Data!.InvoiceNumber);
        Assert.Equal(bank.Id, result.Data.PaymentSafeId);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(90, await f.Db.Safes.Where(s => s.Id == bank.Id).Select(s => s.CurrentBalance).SingleAsync());
        Assert.Equal(1000, await f.Db.Safes.Where(s => s.Id == f.Safe.Id).Select(s => s.CurrentBalance).SingleAsync());
        Assert.Equal(9, await f.Db.StorgeStocks.Select(s => s.Quantity).SingleAsync());
    }

    [Fact]
    public async Task Cashier_cannot_bypass_pos_rules_using_the_generic_invoice_action()
    {
        var context = Context(Permissions.SalesInvoices.Create);
        ((ClaimsIdentity)context.User.Identity!).AddClaim(new Claim(ClaimTypes.Role, "Cashier"));
        var controller = new SalesInvoicesController(null!) { ControllerContext = new ControllerContext { HttpContext = context } };
        var result = await controller.Create(new CreateSalesInvoiceDto(), default);
        Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Theory]
    [InlineData("Permissions.SalesReturns.Create", true)]
    [InlineData("Permissions.SalesInvoices.View", true)]
    [InlineData("Permissions.Products.View", false)]
    public async Task Alternative_permissions_allow_only_the_requested_capabilities(string permission, bool allowed)
    {
        var action = new ActionContext(Context(permission), new RouteData(), new ActionDescriptor());
        var context = new AuthorizationFilterContext(action, []);
        await new HasAnyPermissionAttribute(Permissions.SalesInvoices.View, Permissions.SalesReturns.Create).OnAuthorizationAsync(context);
        if (allowed) Assert.Null(context.Result);
        else Assert.Equal(403, Assert.IsType<ObjectResult>(context.Result).StatusCode);
    }
}
