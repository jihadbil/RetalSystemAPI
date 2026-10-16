using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Warehouses;
using Xunit;

namespace RetalSystemAPI.SecurityTests;
// Regression coverage for the data-access audit findings.
public sealed class DataAccessRegressionTests
{
    [Fact]
    public async Task Stock_paging_keeps_equal_keys_in_stable_pages()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
        var warehouse = new Warehouse
        {
            Name = "showroom",
            Branch = new RetalSystemAPI.Models.Branchs.Branch
            {
                Name = "branch"
            }
        };
        var stocks = Enumerable.Range(0, 5).Select(i => new ShowroomStock { Warehouse = warehouse, Product = new Product { Name = "same", Category = new Category { Name = "category" } } }).ToArray();
        db.AddRange(stocks);
        await db.SaveChangesAsync();
        var repo = new StockRepository(db);
        var first = await repo.SearchShowroomAsync(new ShowroomStockQuery { WarehouseId = warehouse.Id, PageNumber = 1, PageSize = 2 });
        var second = await repo.SearchShowroomAsync(new ShowroomStockQuery { WarehouseId = warehouse.Id, PageNumber = 2, PageSize = 2 });
        Assert.Equal(5, first.TotalCount);
        Assert.Equal(5, second.TotalCount);
        Assert.Empty(first.Items.Select(x => x.Id).Intersect(second.Items.Select(x => x.Id)));
        Assert.Equal(stocks.OrderBy(x => x.Id).Skip(2).Take(2).Select(x => x.Id), second.Items.Select(x => x.Id));
    }

    [Fact]
    public async Task FindById_hides_tracked_soft_deleted_record()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
        var item = new Category
        {
            Name = "category"
        };
        db.Add(item);
        await db.SaveChangesAsync();
        var repo = new CategoryRepository(db);
        repo.SoftDelete(item);
        Assert.Null(await repo.GetByIdAsync(item.Id));
        await db.SaveChangesAsync();
        Assert.Null(await repo.GetByIdAsync(item.Id));
        Assert.Null(await repo.GetByIdAsync(item.Id));
    }

    [Fact]
    public async Task FindById_hides_tracked_record_from_another_tenant()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
        var foreign = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "foreign",
            RowVersion = [1]
        };
        db.Attach(foreign);
        var repo = new CategoryRepository(db);
        Assert.Null(await repo.GetByIdAsync(foreign.Id));
        Assert.Null(await repo.GetByIdAsync(foreign.Id));
    }

    [Fact]
    public async Task Update_rejects_stale_version_when_another_instance_is_tracked()
    {
        var tenantId = Guid.NewGuid();
        await using var db = ModelIntegrityTests.CreateContext(tenantId);
        var current = new Category
        {
            Name = "latest",
            RowVersion = [2]
        };
        db.Add(current);
        await db.SaveChangesAsync();
        var stale = new Category
        {
            Id = current.Id,
            TenantId = tenantId,
            Name = "stale overwrites latest",
            RowVersion = [1]
        };
        var repo = new CategoryRepository(db);
        repo.Update(stale);
        Assert.Equal(new byte[] { 1 }, db.Entry(current).Property(x => x.RowVersion).OriginalValue);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
        Assert.Equal("latest", (await repo.GetByIdAsync(current.Id))!.Name);
    }

    [Fact]
    public async Task Update_preserves_pending_insert_and_saves_final_values()
    {
        var tenantId = Guid.NewGuid();
        await using var db = ModelIntegrityTests.CreateContext(tenantId);
        var repo = new CategoryRepository(db);
        var item = new Category
        {
            TenantId = tenantId,
            Name = "new",
            RowVersion = [1]
        };
        await repo.AddAsync(item);
        repo.Update(item);
        Assert.Equal(EntityState.Added, db.Entry(item).State);
        await db.SaveChangesAsync();
        Assert.NotNull(await repo.GetByIdAsync(item.Id));
    }

    [Fact]
    public async Task Soft_delete_preserves_pending_insert_and_created_audit()
    {
        var tenantId = Guid.NewGuid();
        await using var db = ModelIntegrityTests.CreateContext(tenantId);
        var repo = new CategoryRepository(db);
        var item = new Category
        {
            TenantId = tenantId,
            Name = "new"
        };
        await repo.AddAsync(item);
        repo.SoftDelete(item);
        Assert.Equal(EntityState.Added, db.Entry(item).State);
        await db.SaveChangesAsync();
        Assert.False(await db.Categories.AnyAsync());
        var saved = await db.Categories.IgnoreQueryFilters().SingleAsync();
        Assert.True(saved.IsDeleted);
        Assert.Equal("test-user", saved.CreatedByUserId);
    }

    [Fact]
    public async Task Detached_update_cannot_change_tracked_tenant_key_or_created_audit()
    {
        var tenantId = Guid.NewGuid();
        await using var db = ModelIntegrityTests.CreateContext(tenantId);
        var item = new Category
        {
            Name = "original"
        };
        db.Add(item);
        await db.SaveChangesAsync();
        var originalCreatedAt = item.CreatedAt;
        var incoming = new Category
        {
            Id = item.Id,
            TenantId = tenantId,
            Name = "updated",
            RowVersion = item.RowVersion,
            CreatedAt = DateTime.MinValue,
            CreatedByUserId = "forged"
        };
        var repo = new CategoryRepository(db);
        repo.Update(incoming);
        await db.SaveChangesAsync();
        Assert.Equal(originalCreatedAt, item.CreatedAt);
        Assert.Equal("test-user", item.CreatedByUserId);
        incoming.TenantId = Guid.NewGuid();
        Assert.Throws<UnauthorizedAccessException>(() => repo.Update(incoming));
        Assert.Equal(tenantId, item.TenantId);
    }

    [Fact]
    public async Task Product_delete_removes_zero_storage_stock_and_keeps_counts_consistent()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
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
            Product = product,
            BarCode = "123",
            Title = "barcode"
        };
        var warehouse = new Warehouse
        {
            Name = "storage",
            Type = RetalSystemAPI.Models.Enums.WarehouseType.Storge,
            Branch = new RetalSystemAPI.Models.Branchs.Branch
            {
                Name = "branch"
            }
        };
        db.Add(new StorgeStock { ProductBarcode = barcode, Warehouse = warehouse, Quantity = 0 });
        await db.SaveChangesAsync();
        await using var uow = new UnitOfWork(db);
        var service = new RetalSystemAPI.Services.Catalog.Implementations.ProductService(uow, ModelIntegrityTests.CreateMapper());
        Assert.True((await service.DeleteAsync(product.Id)).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.False(await db.Products.AnyAsync());
        Assert.False(await db.ProductBarCodes.AnyAsync());
        Assert.False(await db.StorgeStocks.AnyAsync());
        var page = await uow.Stock.SearchStorageAsync(new StorageStockQuery { WarehouseId = warehouse.Id });
        Assert.Equal(0, page.TotalCount);
        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task Stock_page_count_includes_all_matches()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
        var warehouse = new Warehouse
        {
            Name = "showroom",
            Branch = new RetalSystemAPI.Models.Branchs.Branch
            {
                Name = "branch"
            }
        };
        for (var i = 0; i < 5; i++)
            db.Add(new ShowroomStock { Warehouse = warehouse, Quantity = i, Product = new Product { Name = $"P{i}", Category = new Category { Name = "category" } } });
        await db.SaveChangesAsync();
        var page = await new StockRepository(db).SearchShowroomAsync(new ShowroomStockQuery { WarehouseId = warehouse.Id, PageNumber = 2, PageSize = 2 });
        Assert.Equal(5, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task Category_repository_applies_paging_once()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
        for (var i = 0; i < 5; i++)
            db.Add(new Category { Name = $"C{i}", SortOrder = i });
        await db.SaveChangesAsync();
        var result = await new CategoryRepository(db).SearchAsync(new CategoryQuery { PageNumber = 2, PageSize = 2 });
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(new[] { "C2", "C3" }, result.Items.Select(x => x.Name));
        var first = await new CategoryRepository(db).SearchAsync(new CategoryQuery { PageNumber = 1, PageSize = 2 });
        Assert.Equal(new[] { "C0", "C1" }, first.Items.Select(x => x.Name));
    }

    [Fact]
    public async Task Expense_category_name_check_matches_filtered_unique_index()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
        var item = new ExpenseCategory
        {
            Name = "rent"
        };
        db.Add(item);
        await db.SaveChangesAsync();
        var repo = new AccountingRepository(db);
        repo.SoftDeleteExpenseCategory(item);
        await db.SaveChangesAsync();
        Assert.False(await repo.ExpenseCategoryNameExistsAsync("rent"));
        using var sql = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=unused;Database=MetadataOnly;Integrated Security=True").Options);
        var index = sql.Model.FindEntityType(typeof(ExpenseCategory))!.GetIndexes().Single(x => x.IsUnique && x.Properties.Any(p => p.Name == "Name"));
        Assert.Equal("[IsDeleted] = 0", index.GetFilter());
    }

    [Fact]
    public async Task Rollback_discards_tracking_and_rejects_context_reuse()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
        await using var uow = new UnitOfWork(db);
        await uow.BeginTransactionAsync();
        var item = new Category
        {
            Name = "cancelled operation"
        };
        await uow.Categories.AddAsync(item);
        await uow.RollbackTransactionAsync();
        Assert.Empty(db.ChangeTracker.Entries());
        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.SaveChangesAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.BeginTransactionAsync());
    }
}
