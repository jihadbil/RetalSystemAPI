using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Services.Accounting.Implementations;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

public sealed class FinancialPostingTests
{
    [Fact]
    public async Task Posting_updates_all_balances_and_creates_auditable_entries()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        await using var context = CreateContext(tenantId);
        await using var unitOfWork = new UnitOfWork(context);
        var service = new FinancialPostingService(unitOfWork);

        context.Add(new Branch { Id = branchId, TenantId = tenantId, Name = "Branch", IsActive = true });

        var safe = new Safe
        {
            Id = Guid.NewGuid(), TenantId = tenantId, BranchId = branchId,
            Name = "Main", CurrentBalance = 1_000, IsActive = true
        };
        var customer = new Customer
        {
            Id = Guid.NewGuid(), TenantId = tenantId, Name = "Customer", CurrentBalance = 100
        };
        var supplier = new Supplier
        {
            Id = Guid.NewGuid(), TenantId = tenantId, Name = "Supplier", CurrentBalance = 200
        };
        context.AddRange(safe, customer, supplier);
        SetRowVersions(context);
        await context.SaveChangesAsync();

        var documentId = Guid.NewGuid();
        await service.PostCustomerBalanceAsync(customer, 75, CustomerTransactionType.SalesInvoice,
            branchId, FinancialDocumentType.SalesInvoice, documentId, DateTime.UtcNow,
            PaymentMethod.Credit, "sale");
        await service.PostSupplierBalanceAsync(supplier, -40, SupplierTransactionType.PurchaseReturn,
            branchId, FinancialDocumentType.PurchaseReturn, documentId, DateTime.UtcNow,
            PaymentMethod.Credit, "return");
        await service.PostSafeBalanceAsync(safe, 50, FinancialDocumentType.SalesInvoice,
            documentId, DateTime.UtcNow, "receipt");
        SetRowVersions(context);
        await unitOfWork.SaveChangesAsync();

        Assert.Equal(175, customer.CurrentBalance);
        Assert.Equal(160, supplier.CurrentBalance);
        Assert.Equal(1_050, safe.CurrentBalance);

        var customerEntry = await context.CustomerTransactions.SingleAsync();
        Assert.Equal(75, customerEntry.DebitAmount);
        Assert.Equal(documentId, customerEntry.DocumentId);

        var supplierEntry = await context.SupplierTransactions.SingleAsync();
        Assert.Equal(40, supplierEntry.DebitAmount);
        Assert.Equal(documentId, supplierEntry.DocumentId);

        var safeEntry = await context.SafeTransactions.SingleAsync();
        Assert.Equal(TransactionDirection.Inbound, safeEntry.Direction);
        Assert.Equal(50, safeEntry.Amount);
        Assert.Equal(documentId, safeEntry.DocumentId);
    }

    [Fact]
    public async Task ResolveSafe_auto_selects_only_unique_active_safe_and_checks_outbound_balance()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        await using var context = CreateContext(tenantId);
        await using var unitOfWork = new UnitOfWork(context);
        var service = new FinancialPostingService(unitOfWork);
        context.Add(new Branch { Id = branchId, TenantId = tenantId, Name = "Branch", IsActive = true });
        var safe = new Safe
        {
            Id = Guid.NewGuid(), TenantId = tenantId, BranchId = branchId,
            Name = "Only safe", CurrentBalance = 25, IsActive = true
        };
        context.Add(safe);
        SetRowVersions(context);
        await context.SaveChangesAsync();

        var selected = await service.ResolveSafeAsync(branchId, null, 20, outbound: true);
        Assert.True(selected.IsSuccess, selected.ErrorMessage);
        Assert.Equal(safe.Id, selected.Data!.Id);

        var insufficient = await service.ResolveSafeAsync(branchId, null, 30, outbound: true);
        Assert.False(insufficient.IsSuccess);
        Assert.Equal("INSUFFICIENT_SAFE_BALANCE", insufficient.ErrorCode);

        context.Add(new Safe
        {
            Id = Guid.NewGuid(), TenantId = tenantId, BranchId = branchId,
            Name = "Second", CurrentBalance = 100, IsActive = true
        });
        SetRowVersions(context);
        await context.SaveChangesAsync();

        var ambiguous = await service.ResolveSafeAsync(branchId, null, 10, outbound: false);
        Assert.False(ambiguous.IsSuccess);
    }

    private static AppDbContext CreateContext(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, new TestTenantService(tenantId));
    }

    private static void SetRowVersions(AppDbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>()
                     .Where(e => e.State == EntityState.Added && e.Entity.RowVersion is null))
            entry.Entity.RowVersion = [1];
    }

    private sealed class TestTenantService(Guid tenantId) : ICurrentTenantService
    {
        public Guid TenantId { get; } = tenantId;
    }
}
