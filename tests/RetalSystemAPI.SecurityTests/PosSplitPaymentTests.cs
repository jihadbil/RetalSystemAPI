using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.DTOs.Accounting.Pos;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Accounting.Implementations;
using RetalSystemAPI.Services.Common.Interfaces;
using RetalSystemAPI.Services.Sales.Implementations;
using Xunit;
using Fixture = RetalSystemAPI.SecurityTests.ServiceLayerRegressionTests.Fixture;

namespace RetalSystemAPI.SecurityTests;

public class PosSplitPaymentTests
{
    private sealed class Allowed : ICurrentPermissionService { public bool HasPermission(string permission) => true; }
    private const string DeviceToken = "split-payment-test-device";
    private static PosPaymentService Payments(Fixture f) => new(f.Db, f.Uow, f.Pos, f.Posting, new ModelIntegrityTests.TestUser(), new Allowed());
    private static SalesInvoiceService Sales(Fixture f) => new(f.Uow, f.Mapper, f.Posting, f.Pos, new Allowed(), Payments(f));
    private static SalesReturnService Returns(Fixture f) => new(f.Uow, f.Mapper, new Allowed(), f.Posting, Payments(f));

    private static async Task<(Guid Session, Guid Bank, Guid CashBox)> PrepareAsync(Fixture f)
    {
        var drawer = await f.Db.Safes.SingleAsync(x => x.Id == f.Safe.Id);
        drawer.Type = SafeType.CashDrawer; drawer.CurrentBalance = drawer.OpeningBalance = 100;
        var terminal = new PosTerminal { BranchId = f.Branch.Id, DefaultCashDrawerId = drawer.Id, Code = "SPLIT", Name = "Split POS", IsActive = true };
        var session = new CashSession { BranchId = f.Branch.Id, PosTerminal = terminal, CashDrawerId = drawer.Id,
            CashierUserId = "test-user", OpenedAt = DateTime.UtcNow, OpeningFloat = 100, ExpectedClosingBalance = 100, Status = CashSessionStatus.Open };
        var bank = new Safe { BranchId = f.Branch.Id, Name = "Bank", Type = SafeType.Bank, IsActive = true, OpeningBalance = 500, CurrentBalance = 500 };
        var cashBox = new Safe { BranchId = f.Branch.Id, Name = "Cash box", Type = SafeType.CashBox, IsActive = true };
        f.Db.AddRange(session, bank, cashBox, new PosDeviceCredential { PosTerminal = terminal,
            TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(DeviceToken))), ExpiresAt = DateTime.UtcNow.AddDays(1) });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        return (session.Id, bank.Id, cashBox.Id);
    }

    private static CreateSalesInvoiceDto Request(Fixture f, Guid session, decimal cash = 40, decimal card = 60) => new()
    {
        InvoiceNumber = "POS-SPLIT", BranchId = f.Branch.Id, WarehouseId = f.Storage.Id, CustomerId = f.Customer.Id,
        CashSessionId = session,
        Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1, UnitPrice = 100 }],
        Payments = new[]
        {
            new PosPaymentDto { ClientMovementId = Guid.Parse("00000000-0000-4000-8000-000000000001"), PaymentMethod = PaymentMethod.Cash, Amount = cash, TenderedAmount = cash + 10 },
            new PosPaymentDto { ClientMovementId = Guid.Parse("00000000-0000-4000-8000-000000000002"), PaymentMethod = PaymentMethod.CreditCard, Amount = card }
        }.Where(x => x.Amount > 0).ToList()
    };
    private static SettleCashSessionPaymentsDto Accounts(Guid bank, Guid cashBox) => new()
    {
        Accounts = [new() { PaymentMethod = PaymentMethod.Cash, SafeId = cashBox }, new() { PaymentMethod = PaymentMethod.CreditCard, SafeId = bank }]
    };

    [Fact]
    public async Task Split_sale_records_one_invoice_two_typed_movements_and_only_actual_cash_and_replays_exactly()
    {
        await using var f = await Fixture.CreateAsync(); var setup = await PrepareAsync(f);
        var first = await Sales(f).CreatePosAsync(Request(f, setup.Session));
        Assert.True(first.IsSuccess, first.ErrorMessage); Assert.Equal(2, first.Data!.Payments.Count);
        Assert.Equal(100, first.Data.PaidAmount); Assert.Equal(PaymentMethod.Multiple, first.Data.PaymentMethod);
        f.Db.ChangeTracker.Clear();
        var retryRequest = Request(f, setup.Session); retryRequest.Payments!.Reverse();
        var retry = await Sales(f).CreatePosAsync(retryRequest);
        Assert.True(retry.IsSuccess, retry.ErrorMessage); Assert.Equal(first.Data.Id, retry.Data!.Id);
        Assert.False((await Sales(f).CreatePosAsync(Request(f, setup.Session, 50, 50))).IsSuccess);
        Assert.Equal(1, await f.Db.SalesInvoices.CountAsync()); Assert.Equal(2, await f.Db.CashSessionMovements.CountAsync());
        Assert.Equal(9, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(140, (await f.Db.Safes.SingleAsync(x => x.Id == f.Safe.Id)).CurrentBalance);
        Assert.Equal(500, (await f.Db.Safes.SingleAsync(x => x.Id == setup.Bank)).CurrentBalance);
        Assert.Equal(40, (await f.Db.SafeTransactions.SingleAsync()).Amount);
        Assert.Equal(0, (await f.Db.Customers.SingleAsync()).CurrentBalance);
        Assert.DoesNotContain(await f.Db.CustomerTransactions.ToListAsync(), x => x.PaymentMethod == PaymentMethod.Multiple);
        var audit = await new FinancialReconciliationService(f.Db).AuditAsync();
        Assert.DoesNotContain(audit.Data!.Issues, x => x.Code == "SAFE_DOCUMENT_LEDGER_MISMATCH");
    }

    [Fact]
    public async Task Closing_keeps_its_existing_cash_count_and_does_not_settle_until_explicit_action()
    {
        await using var f = await Fixture.CreateAsync(); var setup = await PrepareAsync(f);
        Assert.True((await Sales(f).CreatePosAsync(Request(f, setup.Session))).IsSuccess);
        Assert.False((await Payments(f).SettleAsync(setup.Session, Accounts(setup.Bank, setup.CashBox), default)).IsSuccess);
        var close = await f.Pos.CloseSessionAsync(DeviceToken, new() { CountedClosingBalance = 140 });
        Assert.True(close.IsSuccess, close.ErrorMessage); Assert.Equal(CashSessionStatus.Closed, close.Data!.Status);
        Assert.Equal(140, close.Data.ExpectedClosingBalance);
        Assert.Equal(0, await f.Db.CashSessionPaymentSettlements.CountAsync());
        Assert.Equal(500, (await f.Db.Safes.SingleAsync(x => x.Id == setup.Bank)).CurrentBalance);
        var accounts = Accounts(setup.Bank, setup.CashBox);
        var settled = await Payments(f).SettleAsync(setup.Session, accounts, default);
        Assert.True(settled.IsSuccess, settled.ErrorMessage); Assert.True(settled.Data!.IsSettled);
        f.Db.ChangeTracker.Clear(); accounts.Accounts.Reverse();
        Assert.True((await Payments(f).SettleAsync(setup.Session, accounts, default)).IsSuccess);
        Assert.Equal(1, await f.Db.CashSessionPaymentSettlements.CountAsync());
        Assert.Equal(100, (await f.Db.Safes.SingleAsync(x => x.Id == f.Safe.Id)).CurrentBalance);
        Assert.Equal(560, (await f.Db.Safes.SingleAsync(x => x.Id == setup.Bank)).CurrentBalance);
        Assert.Equal(40, (await f.Db.Safes.SingleAsync(x => x.Id == setup.CashBox)).CurrentBalance);
        Assert.Equal(4, await f.Db.SafeTransactions.CountAsync());
        Assert.True((await Sales(f).CreatePosAsync(Request(f, setup.Session))).IsSuccess);
    }

    [Fact]
    public async Task Difference_requires_approval_but_approval_still_does_not_trigger_optional_settlement()
    {
        await using var f = await Fixture.CreateAsync(); var setup = await PrepareAsync(f);
        Assert.True((await Sales(f).CreatePosAsync(Request(f, setup.Session))).IsSuccess);
        var close = await f.Pos.CloseSessionAsync(DeviceToken, new() { CountedClosingBalance = 135 });
        Assert.True(close.IsSuccess); Assert.Equal(CashSessionStatus.PendingApproval, close.Data!.Status);
        Assert.False((await Payments(f).SettleAsync(setup.Session, Accounts(setup.Bank, setup.CashBox), default)).IsSuccess);
        Assert.True((await f.Pos.ApproveDifferenceAsync(setup.Session, new() { Notes = "Cash shortage counted" })).IsSuccess);
        Assert.Equal(0, await f.Db.CashSessionPaymentSettlements.CountAsync());
        Assert.Equal(500, (await f.Db.Safes.SingleAsync(x => x.Id == setup.Bank)).CurrentBalance);
    }

    [Fact]
    public async Task Cancellation_reverses_each_original_method_and_restores_cash_stock_and_customer_once()
    {
        await using var f = await Fixture.CreateAsync(); var setup = await PrepareAsync(f);
        var sale = await Sales(f).CreatePosAsync(Request(f, setup.Session)); Assert.True(sale.IsSuccess, sale.ErrorMessage);
        var cancelled = await Sales(f).UpdateStatusAsync(sale.Data!.Id, InvoiceStatus.Cancelled);
        Assert.True(cancelled.IsSuccess, cancelled.ErrorMessage);
        Assert.False((await Sales(f).UpdateStatusAsync(sale.Data.Id, InvoiceStatus.Cancelled)).IsSuccess);
        Assert.Equal(100, (await f.Db.Safes.SingleAsync(x => x.Id == f.Safe.Id)).CurrentBalance);
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(0, (await f.Db.Customers.SingleAsync()).CurrentBalance);
        Assert.Equal(4, await f.Db.CashSessionMovements.CountAsync());
        Assert.All(await f.Db.CashSessionMovements.Where(x => x.Direction == TransactionDirection.Inbound).ToListAsync(), x => Assert.Equal(x.Amount, x.ReversedAmount));
    }

    [Fact]
    public async Task Return_and_undo_use_original_methods_without_refunding_the_full_amount_from_cash()
    {
        await using var f = await Fixture.CreateAsync(); var setup = await PrepareAsync(f);
        var sale = await Sales(f).CreatePosAsync(Request(f, setup.Session)); Assert.True(sale.IsSuccess, sale.ErrorMessage);
        var returned = await Returns(f).CreateAsync(f.SalesReturn(sale.Data!.Id, 1));
        Assert.True(returned.IsSuccess, returned.ErrorMessage);
        Assert.Equal(100, (await f.Db.Safes.SingleAsync(x => x.Id == f.Safe.Id)).CurrentBalance);
        Assert.Equal(4, await f.Db.CashSessionMovements.CountAsync());
        var undone = await Returns(f).DeleteAsync(returned.Data!.Id); Assert.True(undone.IsSuccess, undone.ErrorMessage);
        Assert.Equal(140, (await f.Db.Safes.SingleAsync(x => x.Id == f.Safe.Id)).CurrentBalance);
        Assert.Equal(6, await f.Db.CashSessionMovements.CountAsync());
        Assert.All(await f.Db.CashSessionMovements.Where(x => x.DocumentType == FinancialDocumentType.SalesInvoice).ToListAsync(), x => Assert.Equal(0, x.ReversedAmount));
    }

    [Theory]
    [InlineData(PaymentMethod.Credit)]
    [InlineData(PaymentMethod.Multiple)]
    public void Aggregate_or_credit_methods_cannot_be_a_receipt(PaymentMethod method) =>
        Assert.NotNull(DtoValidation.GetError(new PosPaymentDto { ClientMovementId = Guid.NewGuid(), PaymentMethod = method, Amount = 1 }));

    [Fact]
    public async Task Invalid_or_duplicated_distribution_does_not_change_stock_or_balances()
    {
        await using var f = await Fixture.CreateAsync(); var setup = await PrepareAsync(f);
        var request = Request(f, setup.Session, 60, 60);
        Assert.False((await Sales(f).CreatePosAsync(request)).IsSuccess);
        request = Request(f, setup.Session); request.Payments![1].ClientMovementId = request.Payments[0].ClientMovementId;
        Assert.False((await Sales(f).CreatePosAsync(request)).IsSuccess);
        Assert.Equal(0, await f.Db.SalesInvoices.CountAsync()); Assert.Equal(0, await f.Db.CashSessionMovements.CountAsync());
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(100, (await f.Db.Safes.SingleAsync(x => x.Id == f.Safe.Id)).CurrentBalance);
    }

    [Fact]
    public async Task Card_only_session_movement_does_not_inflate_cash_count_and_settles_without_a_cash_transfer()
    {
        await using var f = await Fixture.CreateAsync(); var setup = await PrepareAsync(f);
        var sale = await Sales(f).CreatePosAsync(Request(f, setup.Session, 0, 100)); Assert.True(sale.IsSuccess, sale.ErrorMessage);
        Assert.Empty(await f.Db.SafeTransactions.ToListAsync());
        var close = await f.Pos.CloseSessionAsync(DeviceToken, new() { CountedClosingBalance = 100 }); Assert.True(close.IsSuccess, close.ErrorMessage);
        var result = await Payments(f).SettleAsync(setup.Session, new() { Accounts = [new() { PaymentMethod = PaymentMethod.CreditCard, SafeId = setup.Bank }] }, default);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(100, (await f.Db.Safes.SingleAsync(x => x.Id == f.Safe.Id)).CurrentBalance);
        Assert.Equal(600, (await f.Db.Safes.SingleAsync(x => x.Id == setup.Bank)).CurrentBalance);
        Assert.Single(await f.Db.SafeTransactions.ToListAsync());
    }

    [SqlServerFact]
    public async Task Sql_server_failed_settlement_rolls_back_every_balance_and_can_be_retried()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        var failure = new FailSettlementSave();
        await using var f = await Fixture.CreateAsync(database.Context(tenantId, failure), tenantId);
        var setup = await PrepareAsync(f);
        Assert.True((await Sales(f).CreatePosAsync(Request(f, setup.Session))).IsSuccess);
        Assert.True((await f.Pos.CloseSessionAsync(DeviceToken, new() { CountedClosingBalance = 140 })).IsSuccess);
        failure.Armed = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Payments(f).SettleAsync(setup.Session, Accounts(setup.Bank, setup.CashBox), default));
        Assert.Equal(0, await f.Db.CashSessionPaymentSettlements.CountAsync());
        Assert.Equal(0, await f.Db.CashSessionMovements.CountAsync(x => x.SettlementId != null));
        Assert.Equal(140, (await f.Db.Safes.SingleAsync(x => x.Id == f.Safe.Id)).CurrentBalance);
        Assert.Equal(500, (await f.Db.Safes.SingleAsync(x => x.Id == setup.Bank)).CurrentBalance);
        var retryDb = database.Context(tenantId);
        await using var retryUow = new UnitOfWork(retryDb);
        var retryPos = new PosCashService(retryDb, new ModelIntegrityTests.TestUser());
        var retryPayments = new PosPaymentService(retryDb, retryUow, retryPos, new FinancialPostingService(retryUow, retryPos),
            new ModelIntegrityTests.TestUser(), new Allowed());
        Assert.True((await retryPayments.SettleAsync(setup.Session, Accounts(setup.Bank, setup.CashBox), default)).IsSuccess);
    }

    private sealed class FailSettlementSave : SaveChangesInterceptor
    {
        public bool Armed;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken ct = default)
        {
            if (Armed && data.Context!.ChangeTracker.Entries<CashSessionPaymentSettlement>().Any(x => x.State == EntityState.Added))
            {
                Armed = false;
                throw new InvalidOperationException("Injected settlement failure");
            }
            return ValueTask.FromResult(result);
        }
    }

    [SqlServerFact]
    public async Task Sql_server_card_sale_cannot_add_movements_after_a_concurrent_close()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenantId), tenantId);
        var setup = await PrepareAsync(f);
        var proxy = DispatchProxy.Create<IUnitOfWork, EngineeringAuditV2RegressionTests.InterleavingUnitOfWork>();
        var interleaving = (EngineeringAuditV2RegressionTests.InterleavingUnitOfWork)(object)proxy;
        interleaving.Inner = f.Uow;
        interleaving.BeforeBegin = async () =>
        {
            await using var other = database.Context(tenantId);
            var result = await new PosCashService(other, new ModelIntegrityTests.TestUser()).CloseSessionAsync(DeviceToken, new() { CountedClosingBalance = 100 });
            Assert.True(result.IsSuccess, result.ErrorMessage);
        };
        var sales = new SalesInvoiceService(proxy, f.Mapper, f.Posting, f.Pos, new Allowed(), Payments(f));
        var sale = await sales.CreatePosAsync(Request(f, setup.Session, 0, 100));
        Assert.False(sale.IsSuccess);
        Assert.Equal(0, await f.Db.CashSessionMovements.CountAsync());
        Assert.Equal(0, await f.Db.SalesInvoices.CountAsync());
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(CashSessionStatus.Closed, (await f.Db.CashSessions.SingleAsync()).Status);
    }

    [SqlServerFact]
    public async Task Sql_server_split_sale_and_optional_settlement_obey_real_constraints()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenantId), tenantId);
        var setup = await PrepareAsync(f);
        var sale = await Sales(f).CreatePosAsync(Request(f, setup.Session)); Assert.True(sale.IsSuccess, sale.ErrorMessage);
        var close = await f.Pos.CloseSessionAsync(DeviceToken, new() { CountedClosingBalance = 140 }); Assert.True(close.IsSuccess, close.ErrorMessage);
        var result = await Payments(f).SettleAsync(setup.Session, Accounts(setup.Bank, setup.CashBox), default); Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, await f.Db.CashSessionMovements.CountAsync(x => x.SettlementId != null));
    }
}
