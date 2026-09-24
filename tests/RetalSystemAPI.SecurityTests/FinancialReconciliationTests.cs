using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Services.Accounting.Implementations;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

public sealed class FinancialReconciliationTests
{
    [Fact]
    public async Task Audit_returns_no_issues_when_document_ledgers_and_balances_match()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var safeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        await using var context = CreateContext(tenantId);

        context.Add(new Branch { Id = branchId, TenantId = tenantId, Name = "Main", IsActive = true });
        context.Add(new Safe
        {
            Id = safeId, TenantId = tenantId, BranchId = branchId, Name = "Cash",
            OpeningBalance = 100, CurrentBalance = 140, IsActive = true
        });
        context.Add(new Customer
        {
            Id = customerId, TenantId = tenantId, Name = "Customer",
            OpeningBalance = 10, CurrentBalance = 70
        });
        context.Add(new SalesInvoice
        {
            Id = invoiceId, TenantId = tenantId, InvoiceNumber = "S-1", BranchId = branchId,
            WarehouseId = Guid.NewGuid(), CustomerId = customerId,
            Status = InvoiceStatus.PartiallyPaid, PaymentMethod = PaymentMethod.Cash,
            SubTotal = 100, TotalAmount = 100, PaidAmount = 40, RemainingAmount = 60,
            PaymentSafeId = safeId
        });
        context.AddRange(
            new CustomerTransaction
            {
                TenantId = tenantId, CustomerId = customerId, BranchId = branchId,
                TransactionType = CustomerTransactionType.SalesInvoice,
                DebitAmount = 100, BalanceAfter = 110, TransactionDate = DateTime.UtcNow,
                DocumentType = FinancialDocumentType.SalesInvoice, DocumentId = invoiceId
            },
            new CustomerTransaction
            {
                TenantId = tenantId, CustomerId = customerId, BranchId = branchId,
                TransactionType = CustomerTransactionType.Payment,
                CreditAmount = 40, BalanceAfter = 70, TransactionDate = DateTime.UtcNow,
                DocumentType = FinancialDocumentType.SalesInvoice, DocumentId = invoiceId,
                PaymentMethod = PaymentMethod.Cash
            },
            new SafeTransaction
            {
                TenantId = tenantId, SafeId = safeId, TransactionNumber = "OPEN-1",
                Direction = TransactionDirection.Inbound, Amount = 100, BalanceAfter = 100,
                TransactionDate = DateTime.UtcNow, DocumentType = FinancialDocumentType.OpeningBalance
            },
            new SafeTransaction
            {
                TenantId = tenantId, SafeId = safeId, TransactionNumber = "SALE-1",
                Direction = TransactionDirection.Inbound, Amount = 40, BalanceAfter = 140,
                TransactionDate = DateTime.UtcNow, DocumentType = FinancialDocumentType.SalesInvoice,
                DocumentId = invoiceId
            });

        SetRowVersions(context);
        await context.SaveChangesAsync();

        var result = await new FinancialReconciliationService(context).AuditAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.TotalIssues);
        Assert.Empty(result.Data.Issues);
    }

    [Fact]
    public async Task Audit_flags_paid_legacy_document_without_safe_and_missing_ledger()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        await using var context = CreateContext(tenantId);

        context.Add(new Branch { Id = branchId, TenantId = tenantId, Name = "Legacy", IsActive = true });
        context.Add(new SalesInvoice
        {
            Id = invoiceId, TenantId = tenantId, InvoiceNumber = "LEGACY-1", BranchId = branchId,
            WarehouseId = Guid.NewGuid(), Status = InvoiceStatus.Paid, PaymentMethod = PaymentMethod.Cash,
            SubTotal = 50, TotalAmount = 50, PaidAmount = 50, RemainingAmount = 0
        });

        SetRowVersions(context);
        await context.SaveChangesAsync();

        var result = await new FinancialReconciliationService(context).AuditAsync();

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Data!.Issues, x => x.Code == "BRANCH_WITHOUT_ACTIVE_SAFE" && x.EntityId == branchId);
        Assert.Contains(result.Data.Issues, x => x.Code == "PAID_DOCUMENT_WITHOUT_SAFE" && x.EntityId == invoiceId);
        Assert.Contains(result.Data.Issues, x => x.Code == "SAFE_DOCUMENT_LEDGER_MISMATCH" && x.EntityId == invoiceId);
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
