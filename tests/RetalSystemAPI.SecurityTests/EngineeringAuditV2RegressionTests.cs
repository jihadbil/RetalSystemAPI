using System.Reflection;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.DTOs.Customers;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Services.Accounting.Implementations;
using RetalSystemAPI.Services.Catalog.Implementations;
using RetalSystemAPI.Services.Common.Interfaces;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Customers.Implementations;
using RetalSystemAPI.Services.FileUpload.Interfaces;
using RetalSystemAPI.Services.Sales.Implementations;
using RetalSystemAPI.Services.Suppliers.Implementations;
using RetalSystemAPI.Services.Warehouses.Implementations;
using Xunit;
using Fixture = RetalSystemAPI.SecurityTests.ServiceLayerRegressionTests.Fixture;

namespace RetalSystemAPI.SecurityTests;

// Engineering regressions only. InMemory cases do not establish SQL transaction/isolation behavior.
public class EngineeringAuditV2RegressionTests
{
    [Fact]
    public async Task Warehouse_summary_retains_branch_for_document_selectors()
    {
        await using var f = await Fixture.CreateAsync();
        Assert.Equal(f.Branch.Id, f.Mapper.Map<WarehouseSummaryDto>(f.Storage).BranchId);
    }

    [Fact]
    public async Task Product_and_opening_stock_are_persisted_in_one_save()
    {
        await using var f = await Fixture.CreateAsync();
        var saves = new CountSaves(failAt: 2);
        await using var db = Context(f, saves);
        await using var uow = new UnitOfWork(db);
        var result = await new ProductService(uow, f.Mapper).CreateAsync(new()
        {
            Name = "Atomic product", CategoryId = f.Product.CategoryId, CostPrice = 2, SalePrice = 4,
            StorageWarehouseId = f.Storage.Id,
            BarCodes = [new() { BarCode = "ATOMIC", Title = "Opening", InitialQuantity = 7 }]
        });
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, saves.Calls);
        Assert.Equal(7, await f.Db.StorgeStocks.Where(s => s.ProductBarcode.ProductId == result.Data!.Id).SumAsync(s => s.Quantity));
    }

    [Fact]
    public async Task Omitting_units_during_product_edit_preserves_all_conversion_factors()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.AddRange(new ProductUnit { ProductId = f.Product.Id, Unit = new() { Name = "Piece", UnitPackage = 1 }, ConversionFactor = 1, IsDefault = true },
            new ProductUnit { ProductId = f.Product.Id, Unit = new() { Name = "Pack", UnitPackage = 12 }, ConversionFactor = 12 });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = await new ProductService(f.Uow, f.Mapper).UpdateAsync(f.Product.Id, new()
        {
            Id = f.Product.Id, Name = "Renamed", CategoryId = f.Product.CategoryId, CostPrice = 10, SalePrice = 100
        });
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(new[] { 1, 12 }, await f.Db.ProductUnits.OrderBy(u => u.ConversionFactor).Select(u => u.ConversionFactor).ToArrayAsync());
    }

    [Fact]
    public async Task Pos_retry_reuses_invoice_stock_and_payment_and_rejects_a_different_request()
    {
        await using var f = await Fixture.CreateAsync();
        (await f.Db.Safes.SingleAsync()).Type = SafeType.Bank;
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var service = new SalesInvoiceService(f.Uow, f.Mapper, f.Posting, f.Pos, new Allowed());
        CreateSalesInvoiceDto Request(int quantity = 1) => new()
        {
            InvoiceNumber = "POS-STABLE", BranchId = f.Branch.Id, WarehouseId = f.Storage.Id,
            PaymentMethod = PaymentMethod.CreditCard, PaymentSafeId = f.Safe.Id, PaidAmount = 100,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = quantity, UnitPrice = 100 }]
        };
        var first = await service.CreatePosAsync(Request()); f.Db.ChangeTracker.Clear();
        var retry = await service.CreatePosAsync(Request()); f.Db.ChangeTracker.Clear();
        var different = await service.CreatePosAsync(Request(2)); f.Db.ChangeTracker.Clear();
        Assert.True(first.IsSuccess, first.ErrorMessage); Assert.True(retry.IsSuccess, retry.ErrorMessage);
        Assert.Equal(first.Data!.Id, retry.Data!.Id); Assert.False(different.IsSuccess);
        Assert.Equal(1, await f.Db.SalesInvoices.CountAsync());
        Assert.Equal(9, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(1100, (await f.Db.Safes.SingleAsync()).CurrentBalance);
        Assert.Equal(1, await f.Db.SafeTransactions.CountAsync());
    }

    [Fact]
    public async Task Storage_return_without_barcode_fails_before_financial_or_stock_posting()
    {
        await using var f = await Fixture.CreateAsync();
        var product = new Product { Name = "Without barcode", CategoryId = f.Product.CategoryId, SalePrice = 5 };
        f.Db.Add(product); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = await new SalesReturnService(f.Uow, f.Mapper, new Allowed(), f.Posting).CreateAsync(new()
        {
            ReturnNumber = "NO-BARCODE", BranchId = f.Branch.Id, WarehouseId = f.Storage.Id,
            Items = [new() { ProductId = product.Id, Quantity = 1 }]
        });
        Assert.False(result.IsSuccess); Assert.Equal(0, await f.Db.SalesReturns.CountAsync());
        Assert.Equal(0, await f.Db.SafeTransactions.CountAsync());
        Assert.Equal(1000, (await f.Db.Safes.SingleAsync()).CurrentBalance);
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Return_rechecks_original_invoice_after_interleaved_cancellation()
    {
        await using var f = await Fixture.CreateAsync();
        await AssertReturnAfterCancellationAsync(f);
    }

    [SqlServerFact]
    public async Task Sql_server_return_rechecks_original_invoice_after_interleaved_cancellation()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        await AssertReturnAfterCancellationAsync(f);
    }

    private static async Task AssertReturnAfterCancellationAsync(Fixture f)
    {
        var sale = new SalesInvoice
        {
            InvoiceNumber = "ZERO", BranchId = f.Branch.Id, WarehouseId = f.Show.Id, Status = InvoiceStatus.Paid,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1 }]
        };
        f.Db.Add(sale); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var proxy = DispatchProxy.Create<IUnitOfWork, InterleavingUnitOfWork>();
        var control = (InterleavingUnitOfWork)(object)proxy; control.Inner = f.Uow;
        control.BeforeBegin = async () =>
        {
            await using var other = Context(f); await using var uow = new UnitOfWork(other);
            var pos = new PosCashService(other, new ModelIntegrityTests.TestUser());
            var cancelled = await new SalesInvoiceService(uow, f.Mapper, new FinancialPostingService(uow, pos), pos)
                .UpdateStatusAsync(sale.Id, InvoiceStatus.Cancelled);
            Assert.True(cancelled.IsSuccess, cancelled.ErrorMessage);
        };
        var result = await new SalesReturnService(proxy, f.Mapper, new Allowed(), f.Posting).CreateAsync(new()
        {
            ReturnNumber = "INTERLEAVED", OriginalInvoiceId = sale.Id, BranchId = f.Branch.Id, WarehouseId = f.Show.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1 }]
        });
        f.Db.ChangeTracker.Clear();
        Assert.False(result.IsSuccess); Assert.Equal(ErrorCodes.ConcurrencyError, result.ErrorCode);
        Assert.Equal(0, await f.Db.SalesReturns.CountAsync());
        Assert.Equal(1, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Transfer_persists_notes_with_status_and_cancelled_transfer_cannot_move_stock()
    {
        await using var f = await Fixture.CreateAsync();
        var service = new StockTransferService(f.Uow, f.Mapper);
        var created = await service.CreateAsync(new()
        {
            TransferNumber = "TRANSFER", FromWarehouseId = f.Storage.Id, ToWarehouseId = f.Show.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1 }]
        });
        Assert.True(created.IsSuccess, created.ErrorMessage); f.Db.ChangeTracker.Clear();
        var changed = await service.UpdateAsync(created.Data!.Id, new() { Id = created.Data.Id, Status = StockTransferStatus.Confirmed, Notes = "Keep notes" });
        Assert.True(changed.IsSuccess, changed.ErrorMessage); f.Db.ChangeTracker.Clear();
        Assert.Equal("Keep notes", (await f.Db.StockTransfers.SingleAsync()).Notes);
        Assert.True((await service.UpdateStatusAsync(created.Data.Id, StockTransferStatus.Cancelled)).IsSuccess);
        f.Db.ChangeTracker.Clear();
        Assert.False((await service.UpdateStatusAsync(created.Data.Id, StockTransferStatus.Completed)).IsSuccess);
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(0, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Transfer_rejects_destination_overflow_before_mutating_source_even_if_later_saved()
    {
        await using var f = await Fixture.CreateAsync();
        (await f.Db.ShowroomStocks.SingleAsync()).Quantity = int.MaxValue;
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var service = new StockTransferService(f.Uow, f.Mapper);
        var created = await service.CreateAsync(new()
        {
            TransferNumber = "OVERFLOW", FromWarehouseId = f.Storage.Id, ToWarehouseId = f.Show.Id,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1 }]
        });
        var result = await service.UpdateStatusAsync(created.Data!.Id, StockTransferStatus.Completed);
        Assert.False(result.IsSuccess);
        await f.Uow.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        Assert.Equal(10, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(int.MaxValue, (await f.Db.ShowroomStocks.SingleAsync()).Quantity);
        Assert.Equal(StockTransferStatus.Draft, (await f.Db.StockTransfers.SingleAsync()).Status);
    }

    [Fact]
    public async Task Warehouse_with_historical_invoice_cannot_be_deleted_when_stock_is_zero()
    {
        await using var f = await Fixture.CreateAsync();
        var invoice = f.MakeSale(1, 0, 0); f.Db.Add(invoice); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = await new WarehouseService(f.Uow, f.Mapper).DeleteAsync(f.Show.Id);
        Assert.False(result.IsSuccess);
        Assert.NotNull((await f.Sales.GetByIdAsync(invoice.Id)).Data);
    }

    [Fact]
    public async Task Excel_ambiguous_names_are_row_errors_but_stable_barcode_can_update_the_correct_product()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.Add(new Product { Name = f.Product.Name, CategoryId = f.Product.CategoryId, SalePrice = 200 });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        using var ambiguous = Workbook(f, stableIdentity: false);
        var validation = await Excel(f).ValidateExcelAsync(ambiguous);
        Assert.True(validation.IsSuccess, validation.ErrorMessage); Assert.Equal(1, validation.Data!.InvalidRows);
        using var known = Workbook(f);
        var result = await Excel(f).ImportProductsFromExcelAsync(known);
        Assert.True(result.IsSuccess, result.ErrorMessage); Assert.Empty(result.Data!.FailedRows);
        Assert.Equal(2, await f.Db.Products.CountAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Excel_distinguishes_explicit_zero_from_a_blank_quantity_or_threshold(bool explicitZero)
    {
        await using var f = await Fixture.CreateAsync();
        (await f.Db.StorgeStocks.SingleAsync()).MinStockLevel = 5;
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        using var input = Workbook(f, zeroStock: explicitZero);
        var result = await Excel(f).ImportProductsFromExcelAsync(input);
        Assert.True(result.IsSuccess, result.ErrorMessage); Assert.Empty(result.Data!.FailedRows);
        f.Db.ChangeTracker.Clear();
        var stock = await f.Db.StorgeStocks.SingleAsync();
        Assert.Equal(explicitZero ? 0 : 10, stock.Quantity);
        Assert.Equal(explicitZero ? 0 : 5, stock.MinStockLevel);
    }

    [Fact]
    public async Task Excel_reordered_code_and_name_headers_preserve_both_meanings()
    {
        await using var f = await Fixture.CreateAsync();
        using var input = Workbook(f, reverseHeaders: true);
        var result = await Excel(f).ImportProductsFromExcelAsync(input);
        Assert.True(result.IsSuccess, result.ErrorMessage); Assert.Empty(result.Data!.FailedRows);
        Assert.Equal(f.Product.Name, (await f.Db.Products.SingleAsync()).Name);
    }

    [Fact]
    public async Task Excel_stale_row_version_cannot_overwrite_newer_stock()
    {
        await using var f = await Fixture.CreateAsync();
        (await f.Db.StorgeStocks.SingleAsync()).Quantity = 20;
        await using (var other = Context(f))
        {
            var stock = await other.StorgeStocks.SingleAsync(); stock.Quantity = 7; stock.RowVersion = [2];
            await other.SaveChangesAsync();
        }
        var save = typeof(ProductExcelService).GetMethod("SaveTrackedChangesAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => (Task)save.Invoke(Excel(f), [CancellationToken.None])!);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(7, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Customer_and_supplier_statements_include_end_day_and_recompute_backdated_balances()
    {
        await using var f = await Fixture.CreateAsync();
        var day = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc);
        var customer = await f.Db.Customers.SingleAsync(); customer.OpeningBalance = 50; customer.CurrentBalance = 60;
        var supplier = await f.Db.Suppliers.SingleAsync(); supplier.OpeningBalance = 50; supplier.CurrentBalance = 60;
        f.Db.AddRange(
            new CustomerTransaction { CustomerId = customer.Id, BranchId = f.Branch.Id, TransactionType = CustomerTransactionType.Adjustment, TransactionDate = day.AddDays(-1).AddHours(12), DebitAmount = 20, BalanceAfter = 60 },
            new CustomerTransaction { CustomerId = customer.Id, BranchId = f.Branch.Id, TransactionType = CustomerTransactionType.Payment, TransactionDate = day.AddHours(12), CreditAmount = 10, BalanceAfter = 40 },
            new SupplierTransaction { SupplierId = supplier.Id, BranchId = f.Branch.Id, TransactionType = SupplierTransactionType.Adjustment, TransactionDate = day.AddDays(-1).AddHours(12), CreditAmount = 20, BalanceAfter = 60 },
            new SupplierTransaction { SupplierId = supplier.Id, BranchId = f.Branch.Id, TransactionType = SupplierTransactionType.Payment, TransactionDate = day.AddHours(12), DebitAmount = 10, BalanceAfter = 40 });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var customers = new CustomerService(f.Uow, f.Mapper, f.Pos);
        var suppliers = new SupplierService(f.Uow, f.Mapper, f.Pos);
        var all = await customers.GetStatementAsync(new() { CustomerId = customer.Id });
        Assert.Equal(new decimal[] { 70, 60 }, all.Data!.Transactions.Select(t => t.BalanceAfter));
        var c = await customers.GetStatementAsync(new() { CustomerId = customer.Id, FromDate = day, ToDate = day });
        var s = await suppliers.GetStatementAsync(new() { SupplierId = supplier.Id, FromDate = day, ToDate = day });
        Assert.Equal(70, c.Data!.BalanceBeforePeriod); Assert.Equal(60, Assert.Single(c.Data.Transactions).BalanceAfter);
        Assert.Equal(70, s.Data!.BalanceBeforePeriod); Assert.Equal(60, Assert.Single(s.Data.Transactions).BalanceAfter);
    }

    [Fact]
    public async Task Debtor_filters_and_financial_totals_apply_before_pagination()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.AddRange(Enumerable.Range(1, 30).Select(i => new Customer { Name = $"Debtor {i:D2}", CurrentBalance = 10, IsActive = i % 2 == 0 }));
        f.Db.Add(new Customer { Name = "Advance", CurrentBalance = -50 });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var page = await new CustomerService(f.Uow, f.Mapper, f.Pos).GetPagedAsync(2, 10, hasDebt: true);
        Assert.True(page.IsSuccess, page.ErrorMessage); Assert.Equal(30, page.Data!.TotalCount);
        Assert.Equal(10, page.Data.Items.Count); Assert.Equal(300, page.Data.Summary.TotalDebt);
        Assert.Equal(30, page.Data.Summary.WithDebt);
        Assert.Contains(page.Data.Items, c => c.IsActive); Assert.Contains(page.Data.Items, c => !c.IsActive);
        var all = await new CustomerService(f.Uow, f.Mapper, f.Pos).GetPagedAsync(1, 10);
        Assert.Equal(50, all.Data!.Summary.TotalCredit);
    }

    [Fact]
    public async Task Product_page_search_and_lookup_ranking_limit_are_applied_by_repository()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.AddRange(Enumerable.Range(0, 110).Select(i => new Product { Name = $"Match {i:D3}", CategoryId = f.Product.CategoryId }));
        f.Db.Add(new Product { Name = "ZZ Match exact barcode", CategoryId = f.Product.CategoryId, ProductBarCodes = [new() { BarCode = "Match", Title = "Exact" }] });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var service = new ProductService(f.Uow, f.Mapper);
        var page = await service.GetPagedAsync(1, 10, search: "Match");
        Assert.Equal(111, page.Data!.TotalCount); Assert.Equal(10, page.Data.Items.Count);
        var lookup = await service.SearchAsync("Match");
        Assert.Equal(100, lookup.Data!.Count); Assert.Equal("ZZ Match exact barcode", lookup.Data[0].Name);
    }

    [Fact]
    public async Task Safes_filter_by_branch_type_and_active_state()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.Add(new Safe { Name = "Bank", BranchId = f.Branch.Id, Type = SafeType.Bank, IsActive = true });
        f.Db.Add(new Safe { Name = "Inactive bank", BranchId = f.Branch.Id, Type = SafeType.Bank, IsActive = false });
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var page = await new SafeService(f.Uow, f.Mapper, f.Pos).GetAllAsync(f.Branch.Id, SafeType.Bank, true);
        Assert.Equal("Bank", Assert.Single(page.Data!).Name);
    }

    [Fact]
    public async Task Master_data_and_manual_financial_fields_round_trip()
    {
        await using var f = await Fixture.CreateAsync();
        var customers = new CustomerService(f.Uow, f.Mapper, f.Pos);
        var created = await customers.CreateAsync(new() { Name = "Metadata", Code = "  META  ", TaxNumber = "TAX", CommercialRegister = "REG", Notes = "Notes", DiscountRate = 3.125m, IsActive = false });
        Assert.True(created.IsSuccess, created.ErrorMessage); f.Db.ChangeTracker.Clear();
        var customer = (await customers.GetByIdAsync(created.Data!.Id)).Data!;
        Assert.Equal("META", customer.Code); Assert.Equal("TAX", customer.TaxNumber); Assert.Equal("REG", customer.CommercialRegister);
        Assert.Equal(3.125m, customer.DiscountRate); Assert.Equal("Notes", customer.Notes); Assert.False(customer.IsActive);
        var suppliers = new SupplierService(f.Uow, f.Mapper, f.Pos);
        var supplier = await suppliers.CreateAsync(new() { Name = "Metadata supplier", CompanyName = "Company", Email = "mail@example.test", TaxNumber = "SUP-TAX", Notes = "Supplier notes" });
        Assert.True(supplier.IsSuccess, supplier.ErrorMessage); Assert.Equal("Company", supplier.Data!.CompanyName); Assert.Equal("mail@example.test", supplier.Data.Email);
        var date = new DateTime(2026, 1, 4, 12, 0, 0, DateTimeKind.Utc);
        var safe = new SafeService(f.Uow, f.Mapper, f.Pos);
        var transaction = await safe.AddTransactionAsync(new() { SafeId = f.Safe.Id, Amount = 1, Direction = TransactionDirection.Inbound, Description = "Manual", TransactionDate = date, ReferenceNumber = "REF-1" });
        Assert.True(transaction.IsSuccess, transaction.ErrorMessage); Assert.Equal(date, transaction.Data!.TransactionDate); Assert.Equal("REF-1", transaction.Data.ReferenceNumber);
    }

    [Fact]
    public async Task Image_delete_waits_for_database_and_failed_file_cleanup_remains_retryable()
    {
        await using var f = await Fixture.CreateAsync();
        var image = new ProductImage { ProductId = f.Product.Id, ImageUrl = "/uploads/test.png" };
        f.Db.Add(image); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var files = new FileCleanup();
        await using (var db = Context(f, new CountSaves(failAt: 1)))
        await using (var uow = new UnitOfWork(db))
            await Assert.ThrowsAsync<InvalidOperationException>(() => new ProductImageService(uow, f.Mapper, files).RemoveImageAsync(image.Id));
        Assert.Equal(0, files.Calls); Assert.Equal(1, await f.Db.ProductImages.CountAsync());
        files.Fail = true;
        var service = new ProductImageService(f.Uow, f.Mapper, files);
        Assert.False((await service.RemoveImageAsync(image.Id)).IsSuccess);
        f.Db.ChangeTracker.Clear(); Assert.Equal(0, await f.Db.ProductImages.CountAsync());
        Assert.True((await f.Db.ProductImages.IgnoreQueryFilters().SingleAsync()).IsDeleted);
        files.Fail = false;
        Assert.True((await service.RemoveImageAsync(image.Id)).IsSuccess); Assert.Equal(2, files.Calls);
    }

    [Fact]
    public async Task Invoice_print_contract_uses_the_actual_tenant_issuer()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.Add(new Tenant { Id = f.TenantId, Name = "Actual company", CommercialName = "Actual trade name", PhoneNumber = "0911111111", TaxNumber = "REAL-TAX" });
        var invoice = f.MakeSale(1, 0, 0); f.Db.Add(invoice); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = await f.Sales.GetByIdAsync(invoice.Id);
        Assert.Equal("Actual trade name", result.Data!.Issuer!.Name); Assert.Equal("REAL-TAX", result.Data.Issuer.TaxNumber);
    }

    [Fact]
    public void Sql_model_has_customer_code_uniqueness_stock_checks_and_matching_snapshot()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=unused.invalid;Database=Offline;Integrated Security=true").Options);
        var customer = db.GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>().Model.FindEntityType(typeof(Customer))!;
        var index = Assert.Single(customer.GetIndexes(), i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "Code" }));
        Assert.True(index.IsUnique); Assert.Contains("IsDeleted", index.GetFilter());
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.Contains(db.GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>().Model.FindEntityType(typeof(RetalSystemAPI.Models.Warehouses.StorgeStock))!.GetCheckConstraints(), c => c.Name == "CK_StorgeStocks_Quantity");
    }

    [Theory]
    [InlineData(2147483646, 1, true)]
    [InlineData(2147483646, 2, false)]
    public async Task Return_stock_credit_checks_the_integer_boundary(int initial, int quantity, bool success)
    {
        await using var f = await Fixture.CreateAsync();
        (await f.Db.StorgeStocks.SingleAsync()).Quantity = initial;
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = await new SalesReturnService(f.Uow, f.Mapper, new Allowed(), f.Posting).CreateAsync(new()
        {
            ReturnNumber = "BOUNDARY", BranchId = f.Branch.Id, WarehouseId = f.Storage.Id, CustomerId = f.Customer.Id,
            RefundedAmount = 0, PaymentMethod = PaymentMethod.Credit,
            Items = [new() { ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = quantity }]
        });
        Assert.Equal(success, result.IsSuccess); f.Db.ChangeTracker.Clear();
        Assert.Equal(success ? int.MaxValue : initial, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(success ? 1 : 0, await f.Db.SalesReturns.CountAsync());
        if (!success) Assert.Equal(0, (await f.Db.Customers.SingleAsync()).CurrentBalance);
    }

    [Fact]
    public async Task Cancellation_that_would_overflow_stock_preserves_invoice_and_inventory()
    {
        await using var f = await Fixture.CreateAsync();
        (await f.Db.StorgeStocks.SingleAsync()).Quantity = int.MaxValue;
        var invoice = f.MakeSale(2, 0, 0); invoice.WarehouseId = f.Storage.Id;
        f.Db.Add(invoice); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = await f.Sales.UpdateStatusAsync(invoice.Id, InvoiceStatus.Cancelled);
        Assert.False(result.IsSuccess); f.Db.ChangeTracker.Clear();
        Assert.Equal(int.MaxValue, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(InvoiceStatus.Paid, (await f.Db.SalesInvoices.SingleAsync()).Status);
    }

    [Fact]
    public async Task Supplier_and_branch_with_history_remain_visible_after_a_delete_request()
    {
        await using var f = await Fixture.CreateAsync();
        var invoice = f.MakePurchase(); f.Db.Add(invoice); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        Assert.False((await new SupplierService(f.Uow, f.Mapper, f.Pos).DeleteAsync(f.Supplier.Id)).IsSuccess);
        Assert.False((await new RetalSystemAPI.Services.Branch.Implementations.BranchService(f.Uow, f.Mapper).DeleteAsync(f.Branch.Id)).IsSuccess);
        Assert.NotNull((await f.Purchases.GetByIdAsync(invoice.Id)).Data);
    }

    [Fact]
    public async Task Pending_purchase_is_posted_but_a_paid_status_does_not_invent_a_payment()
    {
        await using var f = await Fixture.CreateAsync();
        var request = f.Purchase(2, 10); request.Status = InvoiceStatus.Pending;
        var created = await f.Purchases.CreateAsync(request);
        Assert.True(created.IsSuccess, created.ErrorMessage); f.Db.ChangeTracker.Clear();
        var updated = await f.Purchases.UpdateAsync(created.Data!.Id, new()
        {
            InvoiceNumber = created.Data.InvoiceNumber, InvoiceDate = created.Data.InvoiceDate, SupplierId = f.Supplier.Id,
            BranchId = f.Branch.Id, WarehouseId = f.Storage.Id, Status = InvoiceStatus.Paid,
            PaymentMethod = PaymentMethod.Credit, PaidAmount = 0
        });
        Assert.True(updated.IsSuccess, updated.ErrorMessage); Assert.Equal(InvoiceStatus.Pending, updated.Data!.Status);
        Assert.Equal(0, updated.Data.PaidAmount); Assert.Equal(20, updated.Data.RemainingAmount);
        Assert.Equal(12, (await f.Db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(1000, (await f.Db.Safes.SingleAsync()).CurrentBalance);
    }

    [Fact]
    public async Task Expense_aggregates_cover_every_filtered_page()
    {
        await using var f = await Fixture.CreateAsync();
        var category = new ExpenseCategory { Name = "Operating" };
        f.Db.AddRange(Enumerable.Range(1, 30).Select(i => new Expense
        {
            ExpenseNumber = $"E-{i:D2}", ExpenseCategory = category, BranchId = f.Branch.Id, Amount = 10,
            Description = "Operating", ExpenseDate = DateTime.UtcNow
        }));
        await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var service = new ExpenseService(f.Uow, f.Mapper, f.Pos);
        var first = await service.GetPagedAsync(new() { BranchId = f.Branch.Id, PageNumber = 1, PageSize = 10 });
        var third = await service.GetPagedAsync(new() { BranchId = f.Branch.Id, PageNumber = 3, PageSize = 10 });
        Assert.Equal(30, third.Data!.TotalCount); Assert.Equal(10, third.Data.Items.Count);
        Assert.Equal(300, third.Data.Summary); Assert.Equal(first.Data!.Summary, third.Data.Summary);
        Assert.Empty(first.Data.Items.Select(e => e.Id).Intersect(third.Data.Items.Select(e => e.Id)));
    }

    [Fact]
    public async Task Reconciliation_expects_no_posting_for_unposted_drafts()
    {
        await using var f = await Fixture.CreateAsync();
        var sales = f.MakeSale(1, 100, 100); sales.Status = InvoiceStatus.Draft; sales.PaidAmount = 0; sales.RemainingAmount = 100;
        var purchase = f.MakePurchase(); purchase.Status = InvoiceStatus.Draft; purchase.PaidAmount = 0; purchase.RemainingAmount = purchase.TotalAmount;
        f.Db.AddRange(sales, purchase); await f.Db.SaveChangesAsync(); f.Db.ChangeTracker.Clear();
        var result = await new FinancialReconciliationService(f.Db).AuditAsync();
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.DoesNotContain(result.Data!.Issues, i => i.EntityId == sales.Id || i.EntityId == purchase.Id);
    }

    [Fact]
    public async Task Purchase_validation_repository_calls_do_not_grow_per_line()
    {
        async Task<int> CountAsync(int count)
        {
            await using var f = await Fixture.CreateAsync();
            var repository = DispatchProxy.Create<IProductRepository, CountingProducts>();
            var counter = (CountingProducts)(object)repository; counter.Inner = f.Uow.Products;
            var proxy = DispatchProxy.Create<IUnitOfWork, InterleavingUnitOfWork>();
            var control = (InterleavingUnitOfWork)(object)proxy; control.Inner = f.Uow; control.ProductsOverride = repository;
            var request = f.Purchase(1, 10);
            request.Items = Enumerable.Range(0, count).Select(_ => new RetalSystemAPI.Models.DTOs.Purchase.CreatePurchaseInvoiceItemDto
            {
                ProductId = f.Product.Id, ProductBarCodeId = f.Barcode.Id, Quantity = 1, UnitPrice = 10
            }).ToList();
            var result = await new RetalSystemAPI.Services.Purchase.Implementations.PurchaseInvoiceService(proxy, f.Mapper, f.Posting).CreateAsync(request);
            Assert.True(result.IsSuccess, result.ErrorMessage); return counter.Calls;
        }
        Assert.Equal(await CountAsync(1), await CountAsync(25));
    }

    [SqlServerFact]
    public async Task Sql_server_concurrent_customer_codes_accept_one_request_and_return_a_conflict_for_the_other()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid(); await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); int arrivals = 0;
        async Task<ServiceResult<CustomerResponseDto>> CreateAsync()
        {
            await using var db = database.Context(tenant); await using var uow = new UnitOfWork(db);
            var proxy = DispatchProxy.Create<IUnitOfWork, InterleavingUnitOfWork>();
            var control = (InterleavingUnitOfWork)(object)proxy; control.Inner = uow;
            control.BeforeSave = async () => { if (Interlocked.Increment(ref arrivals) == 2) ready.SetResult(); await ready.Task.WaitAsync(TimeSpan.FromSeconds(20)); };
            return await new CustomerService(proxy, f.Mapper, new PosCashService(db, new ModelIntegrityTests.TestUser()))
                .CreateAsync(new() { Name = "Concurrent", Code = "SAME-CODE" });
        }
        var results = await Task.WhenAll(CreateAsync(), CreateAsync());
        Assert.Single(results, r => r.IsSuccess);
        Assert.Equal(ErrorCodes.CustomerCodeExists, Assert.Single(results, r => !r.IsSuccess).ErrorCode);
        Assert.Equal(1, await f.Db.Customers.CountAsync(c => c.Code == "SAME-CODE"));
    }

    private static ProductExcelService Excel(Fixture f) => new(f.Uow, new ConfigurationBuilder().Build(), NullLogger<ProductExcelService>.Instance);
    private static MemoryStream Workbook(Fixture f, bool stableIdentity = true, bool? zeroStock = null, bool reverseHeaders = false)
    {
        using var book = new XLWorkbook(); var ws = book.AddWorksheet("Products");
        string[] headers = ["كود الصنف", "اسم الصنف *", "الباركود *", "التصنيف", "الوحدة", "معامل التحويل", "سعر التكلفة", "سعر البيع", "كمية الصالة", "كمية المخزن", "حد الطلب", "الوصف"];
        for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
        ws.Cell(2, 1).Value = stableIdentity ? f.Product.Id.ToString() : ""; ws.Cell(2, 2).Value = f.Product.Name;
        ws.Cell(2, 3).Value = stableIdentity ? f.Barcode.BarCode : ""; ws.Cell(2, 4).Value = "Category";
        if (zeroStock == true) { ws.Cell(2, 9).Value = 0; ws.Cell(2, 10).Value = 0; ws.Cell(2, 11).Value = 0; }
        if (reverseHeaders)
        {
            ws.Cell(1, 1).Value = "اسم الصنف"; ws.Cell(1, 2).Value = "كود الصنف";
            ws.Cell(2, 1).Value = f.Product.Name; ws.Cell(2, 2).Value = f.Product.Id.ToString();
        }
        var stream = new MemoryStream(); book.SaveAs(stream); stream.Position = 0; return stream;
    }

    private static AppDbContext Context(Fixture f, params IInterceptor[] interceptors) => new(
        new DbContextOptionsBuilder<AppDbContext>((DbContextOptions<AppDbContext>)f.Db.GetService<IDbContextOptions>()).AddInterceptors(interceptors).Options,
        new ModelIntegrityTests.TestTenant(f.TenantId));
    private sealed class Allowed : ICurrentPermissionService { public bool HasPermission(string permission) => true; }
    private sealed class CountSaves(int failAt) : SaveChangesInterceptor
    {
        public int Calls { get; private set; }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken ct = default)
        {
            if (++Calls == failAt) throw new InvalidOperationException("Injected persistence failure");
            return ValueTask.FromResult(result);
        }
    }
    public class InterleavingUnitOfWork : DispatchProxy
    {
        public IUnitOfWork Inner = null!;
        public Func<Task>? BeforeBegin;
        public Func<Task>? BeforeSave;
        public IProductRepository? ProductsOverride;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "get_Products" && ProductsOverride is not null) return ProductsOverride;
            if (method.Name == nameof(IUnitOfWork.SaveChangesAsync) && BeforeSave is not null) return Save(method, args);
            if (method.Name == nameof(IUnitOfWork.BeginTransactionAsync) && BeforeBegin is not null) return Begin(method, args);
            try { return method.Invoke(Inner, args); } catch (TargetInvocationException ex) { System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException!).Throw(); throw; }
        }
        private async Task<int> Save(MethodInfo method, object?[]? args)
        {
            var action = BeforeSave!; BeforeSave = null; await action(); return await (Task<int>)method.Invoke(Inner, args)!;
        }
        private async Task Begin(MethodInfo method, object?[]? args)
        {
            var action = BeforeBegin!; BeforeBegin = null; await action(); await (Task)method.Invoke(Inner, args)!;
        }
    }
    public class CountingProducts : DispatchProxy
    {
        public IProductRepository Inner = null!; public int Calls;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            Calls++;
            try { return method!.Invoke(Inner, args); }
            catch (TargetInvocationException ex) { System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException!).Throw(); throw; }
        }
    }
    private sealed class FileCleanup : IFileUploadService
    {
        public int Calls; public bool Fail;
        public Task<ServiceResult> DeleteAsync(string url, CancellationToken ct = default) { Calls++; return Task.FromResult(Fail ? ServiceResult.Failure("Cleanup failed", ErrorCodes.ValidationError) : ServiceResult.Success()); }
        public Task<ServiceResult<string>> UploadAsync(IFormFile file, string subfolder, CancellationToken ct = default) => throw new NotSupportedException();
        public bool IsValidImageExtension(string name) => true;
        public bool IsWithinSizeLimit(long bytes, long max = 5 * 1024 * 1024) => true;
    }
}
