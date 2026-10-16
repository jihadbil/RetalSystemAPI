using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Catalog.Implementations;
using Xunit;
using TestDatabase = RetalSystemAPI.SecurityTests.ModelIntegritySqlServerTests.TestDatabase;

namespace RetalSystemAPI.SecurityTests;
public sealed class DataAccessSqlServerRegressionTests
{
    [SqlServerFact]
    public async Task Stale_detached_update_and_delete_cannot_replace_a_newer_tracked_version()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var db = database.Context(tenantId);
        var item = new Category
        {
            Name = "old"
        };
        db.AddRange(Tenant(tenantId), item);
        await db.SaveChangesAsync();
        var stale = await db.Categories.AsNoTracking().SingleAsync();
        item.Name = "latest";
        await db.SaveChangesAsync();
        Assert.False(item.RowVersion.SequenceEqual(stale.RowVersion));
        var repo = new CategoryRepository(db);
        repo.Update(stale);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        Assert.Equal("latest", (await repo.GetByIdAsync(item.Id))!.Name);
        var barcode = new ProductBarCode
        {
            BarCode = "version",
            Title = "old",
            Product = new Product
            {
                Name = "product",
                CategoryId = item.Id
            }
        };
        db.Add(barcode);
        await db.SaveChangesAsync();
        var staleBarcode = await db.ProductBarCodes.AsNoTracking().SingleAsync();
        barcode.Title = "latest";
        await db.SaveChangesAsync();
        new ProductRepository(db).HardDeleteBarCode(staleBarcode);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
        await using var verify = database.Context(tenantId);
        Assert.Equal("latest", (await verify.Categories.SingleAsync()).Name);
    }

    [SqlServerFact]
    public async Task Specialized_updates_copy_defaulted_columns_and_preserve_versions()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var db = database.Context(tenantId);
        var branch = new Branch
        {
            Name = "branch"
        };
        var barcode = new ProductBarCode
        {
            BarCode = "stock",
            Title = "stock",
            Product = new Product
            {
                Name = "product",
                Category = new Category
                {
                    Name = "category"
                }
            }
        };
        var stock = new StorgeStock
        {
            Warehouse = new Warehouse
            {
                Name = "warehouse",
                Type = WarehouseType.Storge,
                Branch = branch
            },
            ProductBarcode = barcode,
            Quantity = 4
        };
        var safe = new Safe
        {
            Name = "safe",
            Branch = branch,
            CurrentBalance = 10
        };
        db.AddRange(Tenant(tenantId), stock, safe);
        await db.SaveChangesAsync();
        var incomingStock = await db.StorgeStocks.AsNoTracking().SingleAsync();
        var incomingSafe = await db.Safes.AsNoTracking().SingleAsync();
        incomingStock.Quantity = 9;
        incomingSafe.CurrentBalance = 25;
        incomingSafe.IsActive = false;
        new StockRepository(db).UpdateStorgeStock(incomingStock);
        new AccountingRepository(db).UpdateSafe(incomingSafe);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(9, (await db.StorgeStocks.SingleAsync()).Quantity);
        var actual = await db.Safes.SingleAsync();
        Assert.Equal(25m, actual.CurrentBalance);
        Assert.False(actual.IsActive);
    }

    [SqlServerFact]
    public async Task Rollback_undoes_flushed_changes_and_requires_a_fresh_context()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var db = database.Context(tenantId);
        var category = new Category
        {
            Name = "original"
        };
        db.AddRange(Tenant(tenantId), category);
        await db.SaveChangesAsync();
        await using var uow = new UnitOfWork(db);
        await uow.BeginTransactionAsync();
        category.Name = "cancelled";
        await uow.SaveChangesAsync();
        await uow.Categories.AddAsync(new Category { Name = "pending" });
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await uow.RollbackTransactionAsync(cancelled.Token);
        Assert.Empty(db.ChangeTracker.Entries());
        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.SaveChangesAsync());
        await using var verify = database.Context(tenantId);
        Assert.Equal("original", (await verify.Categories.SingleAsync()).Name);
        verify.Add(new Category { Name = "new scope" });
        await verify.SaveChangesAsync();
        Assert.Equal(2, await verify.Categories.CountAsync());
    }

    [SqlServerFact]
    public async Task Failed_commit_rolls_back_and_cannot_leak_pending_inserts()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var db = database.Context(tenantId);
        db.Add(Tenant(tenantId));
        await db.SaveChangesAsync();
        await using var uow = new UnitOfWork(db);
        await uow.BeginTransactionAsync();
        db.AddRange(new Unit { Name = "duplicate" }, new Unit { Name = "duplicate" });
        await Assert.ThrowsAsync<DbUpdateException>(() => uow.CommitTransactionAsync());
        Assert.Empty(db.ChangeTracker.Entries());
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        await using var verify = database.Context(tenantId);
        Assert.False(await verify.Units.AnyAsync());
    }

    [SqlServerFact]
    public async Task Deleted_names_can_be_reused_but_active_duplicates_remain_rejected()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tenantId = Guid.NewGuid();
        await using var db = database.Context(tenantId);
        var branch = new Branch
        {
            Name = "branch"
        };
        var safe = new Safe
        {
            Name = "cash",
            Branch = branch
        };
        var category = new ExpenseCategory
        {
            Name = "rent"
        };
        db.AddRange(Tenant(tenantId), safe, category);
        await db.SaveChangesAsync();
        var repo = new AccountingRepository(db);
        repo.SoftDeleteSafe(safe);
        repo.SoftDeleteExpenseCategory(category);
        await db.SaveChangesAsync();
        Assert.Null(await repo.GetSafeByIdAsync(safe.Id));
        Assert.Null(await repo.GetExpenseCategoryByIdAsync(category.Id));
        db.AddRange(new Safe { Name = "cash", BranchId = branch.Id }, new ExpenseCategory { Name = "rent" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.Add(new ExpenseCategory { Name = "rent" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        db.Add(new Safe { Name = "cash", BranchId = branch.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        var downgrade = await Assert.ThrowsAsync<SqlException>(() => db.GetService<IMigrator>().MigrateAsync("20261016104351_EnforceModelIntegrity"));
        Assert.Equal(51000, downgrade.Number);
        Assert.Contains("20261016121121_FixDataAccessConsistency", await db.Database.GetAppliedMigrationsAsync());
    }

    [SqlServerFact]
    public async Task Migration_repairs_only_zero_orphans_and_product_delete_cleans_new_balances()
    {
        await using var database = await TestDatabase.CreateAsync("20261016104351_EnforceModelIntegrity");
        var tenantId = Guid.NewGuid();
        await using var db = database.Context(tenantId);
        var product = new Product
        {
            Name = "legacy",
            Category = new Category
            {
                Name = "category"
            }
        };
        var barcode = new ProductBarCode
        {
            Product = product,
            BarCode = "legacy",
            Title = "legacy"
        };
        var warehouse = new Warehouse
        {
            Name = "warehouse",
            Type = WarehouseType.Storge,
            Branch = new Branch
            {
                Name = "branch"
            }
        };
        var zero = new StorgeStock
        {
            Warehouse = warehouse,
            ProductBarcode = barcode,
            Quantity = 0
        };
        var nonzeroBarcode = new ProductBarCode
        {
            Product = product,
            BarCode = "nonzero",
            Title = "nonzero"
        };
        var nonzero = new StorgeStock
        {
            Warehouse = warehouse,
            ProductBarcode = nonzeroBarcode,
            Quantity = 3
        };
        db.AddRange(Tenant(tenantId), zero, nonzero);
        await db.SaveChangesAsync();
        product.IsDeleted = true;
        barcode.IsDeleted = true;
        await db.SaveChangesAsync();
        await db.GetService<IMigrator>().MigrateAsync();
        db.ChangeTracker.Clear();
        Assert.True((await db.StorgeStocks.IgnoreQueryFilters().SingleAsync(x => x.Id == zero.Id)).IsDeleted);
        Assert.False((await db.StorgeStocks.IgnoreQueryFilters().SingleAsync(x => x.Id == nonzero.Id)).IsDeleted);
        var liveProduct = new Product
        {
            Name = "live",
            CategoryId = product.CategoryId
        };
        var liveStock = new StorgeStock
        {
            WarehouseId = warehouse.Id,
            ProductBarcode = new ProductBarCode
            {
                Product = liveProduct,
                BarCode = "live",
                Title = "live"
            }
        };
        db.Add(liveStock);
        await db.SaveChangesAsync();
        await using var uow = new UnitOfWork(db);
        Assert.True((await new ProductService(uow, ModelIntegrityTests.CreateMapper()).DeleteAsync(liveProduct.Id)).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.True((await db.StorgeStocks.IgnoreQueryFilters().SingleAsync(x => x.Id == liveStock.Id)).IsDeleted);
    }

    private static Tenant Tenant(Guid id) => new()
    {
        Id = id,
        Name = "tenant",
        PhoneNumber = "0912345678"
    };
}
