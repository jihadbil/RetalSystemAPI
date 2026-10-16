using System.Data.Common;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Catalog.Implementations;
using Xunit;
using TestDatabase = RetalSystemAPI.SecurityTests.ModelIntegritySqlServerTests.TestDatabase;

namespace RetalSystemAPI.SecurityTests;
public sealed class RepositoryRefactorSqlServerTests
{
    [SqlServerFact]
    public async Task Specialized_searches_and_details_translate_and_preserve_relationships()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        var capture = new Commands();
        await using var db = database.Context(tenantId, capture);
        var branch = new Branch
        {
            Name = "Branch"
        };
        var storage = new Warehouse
        {
            Name = "Storage",
            Type = WarehouseType.Storge,
            Branch = branch
        };
        var showroom = new Warehouse
        {
            Name = "Showroom",
            Type = WarehouseType.Show,
            Branch = branch
        };
        var unit = new Unit
        {
            Name = "piece"
        };
        var product = new Product
        {
            Name = "Needle",
            Category = new Category
            {
                Name = "Category"
            },
            ProductUnits = [new ProductUnit
            {
                Unit = unit,
                IsDefault = true,
                ConversionFactor = 1
            }

            ],
            ProductImages = [new ProductImage
            {
                ImageUrl = "/image.png",
                IsDefault = true
            }

            ]
        };
        var barcode = new ProductBarCode
        {
            Product = product,
            BarCode = "CODE",
            Title = "barcode"
        };
        var supplier = new Supplier
        {
            Name = "Supplier"
        };
        var customer = new Customer
        {
            Name = "Customer"
        };
        var order = new PurchaseOrder
        {
            OrderNumber = "PO",
            Branch = branch,
            Warehouse = storage,
            Supplier = supplier,
            Items = [new PurchaseOrderItem
            {
                ProductBarCode = barcode,
                Quantity = 1,
                UnitPrice = 1,
                LineTotal = 1
            }

            ]
        };
        var purchase = new PurchaseInvoice
        {
            InvoiceNumber = "PI",
            Branch = branch,
            Warehouse = storage,
            Supplier = supplier,
            Items = [new PurchaseInvoiceItem
            {
                Product = product,
                ProductBarCode = barcode,
                Quantity = 1,
                UnitPrice = 1,
                LineTotal = 1
            }

            ]
        };
        var sale = new SalesInvoice
        {
            InvoiceNumber = "SI",
            Branch = branch,
            Warehouse = showroom,
            Customer = customer,
            Items = [new SalesInvoiceItem
            {
                Product = product,
                ProductBarCode = barcode,
                Quantity = 1,
                UnitPrice = 1,
                LineTotal = 1
            }

            ]
        };
        var purchaseReturn = new PurchaseReturn
        {
            ReturnNumber = "PR",
            PurchaseInvoice = purchase,
            Supplier = supplier,
            Branch = branch,
            Warehouse = storage
        };
        var salesReturn = new SalesReturn
        {
            ReturnNumber = "SR",
            OriginalInvoice = sale,
            Customer = customer,
            Branch = branch,
            Warehouse = showroom
        };
        var transfer = new StockTransfer
        {
            TransferNumber = "TR",
            FromWarehouse = storage,
            ToWarehouse = showroom
        };
        var adjustment = new StockAdjustment
        {
            AdjustmentNumber = "AD",
            Warehouse = storage,
            Reason = (StockAdjustmentReason)1
        };
        var safe = new Safe
        {
            Name = "cash",
            Branch = branch
        };
        var expenseCategory = new ExpenseCategory
        {
            Name = "rent"
        };
        var expense = new Expense
        {
            ExpenseNumber = "EX",
            ExpenseCategory = expenseCategory,
            Branch = branch,
            Amount = 1,
            Description = "rent",
            PaymentMethod = PaymentMethod.Cash
        };
        db.AddRange(Tenant(tenantId), order, purchase, sale, purchaseReturn, salesReturn, transfer, adjustment, safe, expense, new StorgeStock { Warehouse = storage, ProductBarcode = barcode, Quantity = 3 }, new ShowroomStock { Warehouse = showroom, Product = product, Quantity = 4 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        capture.Text.Clear();
        await using var uow = new UnitOfWork(db);
        Assert.Single((await uow.Branches.SearchAsync(new BranchQuery())).Items);
        Assert.Single((await uow.Categories.SearchAsync(new CategoryQuery())).Items);
        Assert.Single(await uow.Units.ListLookupAsync());
        var products = await uow.Products.SearchAsync(new ProductQuery { Search = "CODE", CategoryId = product.CategoryId, PageSize = 1 });
        Assert.Equal(1, products.TotalCount);
        Assert.Single(products.Items.Single().ProductBarCodes);
        Assert.Single(products.Items.Single().ProductImages);
        Assert.Single((await uow.Products.GetDetailsAsync(product.Id))!.ProductUnits);
        Assert.Single((await uow.Customers.SearchAsync(new CustomerQuery { Search = "Customer", IsActive = true })).Items);
        Assert.Single((await uow.Suppliers.SearchAsync(new SupplierQuery { Search = "Supplier" })).Items);
        Assert.Single((await uow.Warehouses.SearchAsync(new WarehouseQuery { BranchId = branch.Id, Type = WarehouseType.Show })).Items);
        Assert.Single((await uow.Tenants.SearchAsync(new TenantQuery())).Items);
        Assert.Single((await uow.PurchaseOrders.SearchAsync(new PurchaseOrderQuery { BranchId = branch.Id })).Items);
        Assert.Single((await uow.PurchaseInvoices.SearchAsync(new PurchaseInvoiceQuery { SupplierId = supplier.Id })).Items);
        Assert.Single((await uow.PurchaseReturns.SearchAsync(new PurchaseReturnQuery { Search = "PR" })).Items);
        Assert.Single((await uow.SalesInvoices.SearchAsync(new SalesInvoiceQuery { CustomerId = customer.Id })).Items);
        Assert.Single((await uow.SalesReturns.SearchAsync(new SalesReturnQuery { Search = "SR" })).Items);
        Assert.Single((await uow.StockTransfers.SearchAsync(new StockTransferQuery { FromWarehouseId = storage.Id })).Items);
        Assert.Single((await uow.StockAdjustments.SearchAsync(new StockAdjustmentQuery { WarehouseId = storage.Id })).Items);
        Assert.Single((await uow.Stock.SearchStorageAsync(new StorageStockQuery { WarehouseId = storage.Id, SearchTerm = "CODE", ExactBarcode = true })).Items);
        Assert.Single((await uow.Stock.SearchShowroomAsync(new ShowroomStockQuery { WarehouseId = showroom.Id, SearchTerm = "CODE", ExactBarcode = true })).Items);
        Assert.Single((await uow.Accounting.SearchSafesAsync(new SafeQuery { SearchTerm = "cash" })).Items);
        Assert.Single((await uow.Accounting.SearchExpensesAsync(new ExpenseQuery { CategoryId = expenseCategory.Id })).Items);
        Assert.Single((await uow.Accounting.SearchExpenseCategoriesAsync(new ExpenseCategoryQuery { SearchTerm = "rent" })).Items);
        Assert.Empty((await uow.Accounting.SearchSafeTransactionsAsync(new SafeTransactionQuery())).Items);
        var details = await uow.PurchaseInvoices.GetReadResultAsync(purchase.Id);
        Assert.Equal("Needle", details!.Items.Single().ProductName);
        Assert.Equal("Supplier", details.SupplierName);
        Assert.True(await uow.SalesInvoices.HasProductReferencesAsync(product.Id));
        Assert.True(await uow.PurchaseInvoices.HasBarcodeReferencesAsync([barcode.Id]));
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.DoesNotContain(capture.Text, sql => sql.Contains("ORDER BY (SELECT 1)"));
        var tracked = await uow.SalesInvoices.GetDetailsForUpdateAsync(sale.Id);
        Assert.Equal(EntityState.Unchanged, db.Entry(tracked!).State);
        // The same specialized search cannot expose another tenant's graph.
        await using var other = database.Context(Guid.NewGuid());
        Assert.Empty((await new ProductRepository(other).SearchAsync(new ProductQuery { Search = "CODE" })).Items);
    }

    [SqlServerFact]
    public async Task Import_candidate_loading_uses_three_queries_for_1005_keys()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        var capture = new Commands();
        await using var db = database.Context(tenantId, capture);
        var category = new Category
        {
            Name = "category"
        };
        db.Add(Tenant(tenantId));
        var products = Enumerable.Range(0, 1005).Select(i => new Product { Name = $"P{i}", Category = category }).ToArray();
        db.AddRange(products);
        db.Add(new Product { Name = "unrelated", Category = category });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        capture.Text.Clear();
        var result = await new ProductRepository(db).GetImportCandidatesForUpdateAsync(new ImportKeys([], products.Select(p => p.Name).ToArray()));
        Assert.Equal(1005, result.Count);
        Assert.Equal(3, capture.Text.Count);
        Assert.Equal(1005, db.ChangeTracker.Entries<Product>().Count());
    }

    [SqlServerFact]
    public Task Single_sheet_import_updates_by_barcode_and_exports_without_loading_unrelated_entities() => VerifyImportAsync(singleSheet: true);
    [SqlServerFact]
    public Task Multi_sheet_import_links_new_product_codes_and_keeps_global_barcode_uniqueness() => VerifyImportAsync(singleSheet: false);
    private static async Task VerifyImportAsync(bool singleSheet)
    {
        await using var database = await TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var db = database.Context(tenantId);
        var branch = new Branch
        {
            Name = "branch"
        };
        var storage = new Warehouse
        {
            Name = "storage",
            Type = WarehouseType.Storge,
            Branch = branch
        };
        var showroom = new Warehouse
        {
            Name = "showroom",
            Type = WarehouseType.Show,
            Branch = branch
        };
        var category = new Category
        {
            Name = "food"
        };
        var product = new Product
        {
            Name = "old",
            Category = category
        };
        var unrelated = new Product
        {
            Name = "unrelated",
            Category = category
        };
        db.AddRange(Tenant(tenantId), storage, showroom, new Unit { Name = "حبة" }, new ProductBarCode { Product = product, BarCode = "EXIST", Title = "old" }, new ProductBarCode { Product = unrelated, BarCode = "200000000500", Title = "reserved" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        using var workbook = new XLWorkbook();
        if (singleSheet)
        {
            var sheet = workbook.AddWorksheet("Products");
            var headers = new[]
            {
                "كود الصنف",
                "اسم الصنف",
                "الباركود",
                "التصنيف",
                "الوحدة",
                "معامل التحويل",
                "سعر التكلفة",
                "سعر البيع",
                "كمية الصالة",
                "كمية المخزن",
                "حد الطلب",
                "الوصف"
            };
            for (var i = 0; i < headers.Length; i++)
                sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(2, 2).Value = "renamed";
            sheet.Cell(2, 3).Value = "EXIST";
            sheet.Cell(3, 2).Value = "new";
            for (var row = 2; row <= 3; row++)
            {
                sheet.Cell(row, 4).Value = "food";
                sheet.Cell(row, 5).Value = "حبة";
                sheet.Cell(row, 6).Value = 1;
                sheet.Cell(row, 7).Value = 2;
                sheet.Cell(row, 8).Value = 3;
                sheet.Cell(row, 9).Value = 5;
                sheet.Cell(row, 10).Value = 7;
            }
        }
        else
        {
            var sheet = workbook.AddWorksheet("الأصناف");
            sheet.Cell(1, 1).Value = "معرف الصنف";
            sheet.Cell(1, 2).Value = "اسم الصنف";
            sheet.Cell(2, 1).Value = product.Id.ToString();
            sheet.Cell(2, 2).Value = "renamed";
            sheet.Cell(2, 5).Value = category.Id.ToString();
            sheet.Cell(3, 1).Value = "RAW-NEW";
            sheet.Cell(3, 2).Value = "new";
            sheet.Cell(3, 5).Value = category.Id.ToString();
            var barcodes = workbook.AddWorksheet("الباركودات");
            barcodes.Cell(1, 1).Value = "معرف الباركود";
            barcodes.Cell(2, 2).Value = "RAW-NEW";
            barcodes.Cell(2, 3).Value = "NEW-CODE";
            var categories = workbook.AddWorksheet("التصنيفات");
            categories.Cell(1, 1).Value = "معرف التصنيف";
            categories.Cell(2, 1).Value = category.Id.ToString();
            categories.Cell(2, 2).Value = "food";
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        await using var uow = new UnitOfWork(db);
        var logger = new ImportLogger();
        var service = new ProductExcelService(uow, new ConfigurationBuilder().Build(), logger);
        var result = await service.ImportProductsFromExcelAsync(stream, new ProductExcelImportOptionsDto { DefaultShowroomWarehouseId = showroom.Id, DefaultStorageWarehouseId = storage.Id });
        Assert.True(result.IsSuccess, logger.Error?.ToString() ?? result.ErrorMessage);
        Assert.True(result.Data!.IsSuccess, string.Join(";", result.Data.Errors));
        Assert.DoesNotContain(db.ChangeTracker.Entries<Product>(), e => e.Entity.Id == unrelated.Id);
        Assert.Equal("renamed", (await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id)).Name);
        var added = await db.Products.AsNoTracking().SingleAsync(p => p.Name == "new");
        var barcodeValue = singleSheet ? "200000000501" : "NEW-CODE";
        Assert.Equal(added.Id, (await db.ProductBarCodes.AsNoTracking().SingleAsync(b => b.BarCode == barcodeValue)).ProductId);
        if (singleSheet)
            Assert.Equal(7, (await db.StorgeStocks.AsNoTracking().SingleAsync(s => s.ProductBarcode.ProductId == added.Id)).Quantity);
        var exported = await service.ExportProductsToExcelAsync(singleSheet);
        using var exportedStream = new MemoryStream(exported);
        using var reloaded = new XLWorkbook(exportedStream);
        Assert.True(reloaded.Worksheets.Count >= 1);
        Assert.Contains(reloaded.Worksheets.First().CellsUsed(), c => c.GetString() == "new");
    }

    private static Tenant Tenant(Guid id) => new()
    {
        Id = id,
        Name = "tenant",
        PhoneNumber = "0912345678"
    };
    private sealed class Commands : DbCommandInterceptor
    {
        public List<string> Text { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Text.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }

    private sealed class ImportLogger : ILogger<ProductExcelService>
    {
        public Exception? Error { get; private set; }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Error = exception;
    }
}
