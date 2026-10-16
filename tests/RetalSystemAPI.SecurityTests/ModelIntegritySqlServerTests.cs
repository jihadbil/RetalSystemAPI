using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Interceptors;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Accounting.Implementations;
using RetalSystemAPI.Services.Catalog.Implementations;
using RetalSystemAPI.Services.Purchase.Implementations;
using Xunit;

namespace RetalSystemAPI.SecurityTests;
// Set RETAL_TEST_SQLSERVER to a server-only connection string. Each test owns a fresh database.
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RETAL_TEST_SQLSERVER")))
            Skip = "RETAL_TEST_SQLSERVER is not configured.";
    }
}

public sealed class ModelIntegritySqlServerTests
{
    [SqlServerFact]
    public async Task Migrations_enforce_tenant_and_barcode_ownership_and_soft_delete_uniqueness()
    {
        await using var database = await TestDatabase.CreateAsync();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        await using var db = database.Context(a);
        var categoryA = new Category
        {
            Name = "A"
        };
        var productA = new Product
        {
            Name = "A",
            Category = categoryA
        };
        db.AddRange(new Tenant { Id = a, Name = "A", PhoneNumber = "0912345678" }, productA);
        await db.SaveChangesAsync();
        Guid categoryBId;
        await using (var other = database.Context(b))
        {
            var categoryB = new Category
            {
                Name = "B"
            };
            other.AddRange(new Tenant { Id = b, Name = "B", PhoneNumber = "0912345678" }, categoryB);
            await other.SaveChangesAsync();
            categoryBId = categoryB.Id;
        }

        var foreignTenant = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Products SET CategoryId = {categoryBId} WHERE Id = {productA.Id}"));
        Assert.Equal(547, foreignTenant.Number);
        // Even a detached object with a known row version cannot disguise a foreign row as its own.
        await using (var other = database.Context(b))
        {
            var forged = new Product
            {
                Id = productA.Id,
                TenantId = b,
                Name = "forged",
                CategoryId = categoryBId,
                RowVersion = productA.RowVersion
            };
            other.Attach(forged);
            other.Entry(forged).Property(p => p.Name).IsModified = true;
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => other.SaveChangesAsync());
        }

        var product2 = new Product
        {
            Name = "A2",
            CategoryId = categoryA.Id
        };
        var barcode = new ProductBarCode
        {
            BarCode = "A2",
            Title = "A2",
            Product = product2
        };
        var image = new ProductImage
        {
            ProductId = productA.Id,
            ImageUrl = "/a.png"
        };
        db.AddRange(product2, barcode, image);
        await db.SaveChangesAsync();
        var foreignProduct = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE ProductImages SET BarcodeId = {barcode.Id} WHERE Id = {image.Id}"));
        Assert.Equal(547, foreignProduct.Number);
        var unit = new Unit
        {
            Name = "gram"
        };
        db.Add(unit);
        await db.SaveChangesAsync();
        unit.IsDeleted = true;
        await db.SaveChangesAsync();
        db.Add(new Unit { Name = "gram" });
        await db.SaveChangesAsync();
        Assert.Single(await db.Units.Where(u => u.Name == "gram").ToListAsync());
        var u1 = new Unit
        {
            Name = "piece"
        };
        var u2 = new Unit
        {
            Name = "dozen"
        };
        db.AddRange(u1, u2);
        await db.SaveChangesAsync();
        await using var uow = new UnitOfWork(db);
        var units = new ProductUnitService(uow, ModelIntegrityTests.CreateMapper());
        var first = await units.AddUnitToProductAsync(productA.Id, new CreateProductUnitDto { UnitId = u1.Id, ConversionFactor = 1, IsDefault = true });
        var second = await units.AddUnitToProductAsync(productA.Id, new CreateProductUnitDto { UnitId = u2.Id, ConversionFactor = 12, IsDefault = true });
        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.True((await units.SetDefaultUnitAsync(first.Data!.Id)).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.Equal(first.Data.Id, (await db.ProductUnits.SingleAsync(u => u.IsDefault)).Id);
        var duplicateDefault = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE ProductUnits SET IsDefault = 1 WHERE Id = {second.Data!.Id}"));
        Assert.Contains(duplicateDefault.Number, new[] { 2601, 2627 });
    }

    [SqlServerFact]
    public async Task Purchase_packages_use_base_units_and_preserve_four_decimal_money_after_reload()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var db = database.Context(tenantId);
        var branch = new Branch
        {
            Name = "branch"
        };
        var warehouse = new Warehouse
        {
            Name = "storage",
            Type = WarehouseType.Storge,
            Branch = branch
        };
        var product = new Product
        {
            Name = "rice",
            Category = new Category
            {
                Name = "food"
            }
        };
        var barcode = new ProductBarCode
        {
            BarCode = "RICE",
            Title = "rice",
            Product = product
        };
        var supplier = new Supplier
        {
            Name = "supplier",
            IsActive = true
        };
        db.AddRange(new Tenant { Id = tenantId, Name = "tenant", PhoneNumber = "0912345678" }, warehouse, product, barcode, supplier);
        await db.SaveChangesAsync();
        await using var uow = new UnitOfWork(db);
        var service = new PurchaseInvoiceService(uow, ModelIntegrityTests.CreateMapper(), new FinancialPostingService(uow, new PosCashService(db, new ModelIntegrityTests.TestUser())));
        var dto = new CreatePurchaseInvoiceDto
        {
            InvoiceNumber = "HALF-KILO",
            SupplierId = supplier.Id,
            BranchId = branch.Id,
            WarehouseId = warehouse.Id,
            Items = [new()
            {
                ProductId = product.Id,
                ProductBarCodeId = barcode.Id,
                Quantity = 500,
                UnitPrice = 0.0125m,
                Breakdowns = [new()
                {
                    ProductBarCodeId = barcode.Id,
                    PackageQuantity = 0.5m,
                    UnitsPerPackage = 1000,
                    UnitPrice = 0.0125m
                }

                ]
            }

            ]
        };
        Assert.True((await service.CreateAsync(dto)).IsSuccess);
        var precise = new CreatePurchaseInvoiceDto
        {
            InvoiceNumber = "PRECISE",
            SupplierId = supplier.Id,
            BranchId = branch.Id,
            WarehouseId = warehouse.Id,
            Items = [new()
            {
                ProductId = product.Id,
                ProductBarCodeId = barcode.Id,
                Quantity = 1,
                UnitPrice = 10.125m
            }

            ]
        };
        Assert.True((await service.CreateAsync(precise)).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.Equal(501, (await db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(10.125m, (await db.PurchaseInvoices.SingleAsync(i => i.InvoiceNumber == "PRECISE")).TotalAmount);
        Assert.Equal(16.375m, (await db.Suppliers.SingleAsync()).CurrentBalance);
        Assert.Equal(16.375m, await db.SupplierTransactions.SumAsync(t => t.CreditAmount - t.DebitAmount));
        precise.InvoiceNumber = "FRACTION";
        precise.Items[0].Quantity = 1.75m;
        Assert.False((await service.CreateAsync(precise)).IsSuccess);
        dto.InvoiceNumber = "MISMATCH";
        dto.Items[0].Quantity = 600;
        Assert.False((await service.CreateAsync(dto)).IsSuccess);
        Assert.Equal(2, await db.PurchaseInvoices.CountAsync());
        Assert.Equal(501, (await db.StorgeStocks.SingleAsync()).Quantity);
        var itemId = await db.PurchaseInvoiceItems.Select(i => i.Id).FirstAsync();
        var fractional = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE PurchaseInvoiceItems SET Quantity = 1.75 WHERE Id = {itemId}"));
        Assert.Equal(547, fractional.Number);
        // Changing the quantity of an existing row must adjust the receipt, not just its header totals.
        var invoiceId = await db.PurchaseInvoices.Where(i => i.InvoiceNumber == "PRECISE").Select(i => i.Id).SingleAsync();
        var update = new UpdatePurchaseInvoiceDto
        {
            InvoiceNumber = "PRECISE",
            SupplierId = supplier.Id,
            BranchId = branch.Id,
            WarehouseId = warehouse.Id,
            Status = InvoiceStatus.Pending,
            PaymentMethod = PaymentMethod.Cash,
            Items = [new()
            {
                ProductId = product.Id,
                ProductBarCodeId = barcode.Id,
                Quantity = 2,
                UnitPrice = 10.125m
            }

            ]
        };
        Assert.True((await service.UpdateAsync(invoiceId, update)).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.Equal(502, (await db.StorgeStocks.SingleAsync()).Quantity);
        Assert.Equal(2m, (await db.PurchaseInvoiceItems.SingleAsync(i => i.PurchaseInvoiceId == invoiceId)).Quantity);
        Assert.Equal(26.5m, (await db.Suppliers.SingleAsync()).CurrentBalance);
    }

    [SqlServerFact]
    public async Task Migration_refuses_legacy_fractional_quantities_without_rounding_or_partial_schema_changes()
    {
        await using var database = await TestDatabase.CreateAsync("20260923102137_AddHybridPosCashSessions");
        var tenantId = Guid.NewGuid();
        await using var db = database.Context(tenantId);
        var branch = new Branch
        {
            Name = "branch"
        };
        var product = new Product
        {
            Name = "product",
            Category = new Category
            {
                Name = "category"
            }
        };
        var barcode = new ProductBarCode
        {
            BarCode = "LEGACY",
            Title = "legacy",
            Product = product
        };
        var order = new RetalSystemAPI.Models.Purchase.PurchaseOrder
        {
            OrderNumber = "LEGACY",
            Branch = branch,
            OrderDate = DateTime.UtcNow,
            Items = [new()
            {
                ProductBarCode = barcode,
                Quantity = 1.75m,
                UnitPrice = 10,
                LineTotal = 17.5m
            }

            ]
        };
        db.AddRange(new Tenant { Id = tenantId, Name = "legacy", PhoneNumber = "0912345678" }, order);
        await db.SaveChangesAsync();
        var exception = await Assert.ThrowsAsync<SqlException>(() => db.GetService<IMigrator>().MigrateAsync());
        Assert.Equal(51000, exception.Number);
        Assert.Contains("smallest-unit", exception.Message);
        Assert.Equal(1.75m, await db.PurchaseOrderItems.Select(i => i.Quantity).SingleAsync());
        Assert.DoesNotContain("20261016104351_EnforceModelIntegrity", await db.Database.GetAppliedMigrationsAsync());
    }

    internal sealed class TestDatabase(string connectionString) : IAsyncDisposable
    {
        public AppDbContext Context(Guid tenantId = default, params Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[] interceptors) => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).AddInterceptors(new AuditInterceptor(new ModelIntegrityTests.TestUser(), new ModelIntegrityTests.TestTenant(tenantId))).AddInterceptors(interceptors).Options, new ModelIntegrityTests.TestTenant(tenantId));
        public static async Task<TestDatabase> CreateAsync(string? targetMigration = null)
        {
            var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("RETAL_TEST_SQLSERVER"))
            {
                InitialCatalog = "RetalModelTests_" + Guid.NewGuid().ToString("N")
            };
            var database = new TestDatabase(connection.ConnectionString);
            try
            {
                await using var db = database.Context();
                await db.GetService<IMigrator>().MigrateAsync(targetMigration);
                return database;
            }
            catch
            {
                await database.DisposeAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            var name = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
            if (!name.StartsWith("RetalModelTests_", StringComparison.Ordinal) || name.Length != "RetalModelTests_".Length + 32)
                throw new InvalidOperationException("Refusing to delete a database not owned by these tests.");
            await using var db = Context();
            await db.Database.EnsureDeletedAsync();
        }
    }
}
