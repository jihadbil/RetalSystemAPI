using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Interceptors;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Models.DTOs.MappingProfiles;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

public sealed class ModelIntegrityTests
{
    [Theory]
    [InlineData(1.75)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(2147483648)]
    public void Stock_rejects_invalid_base_unit_counts(decimal quantity)
    {
        Assert.False(StockQuantity.IsValid(quantity));
        Assert.Throws<ArgumentOutOfRangeException>(() => StockQuantity.ToInt32(quantity));
    }

    [Theory]
    [InlineData(0.5, 1000, 500)]
    [InlineData(2, 12, 24)]
    public void Package_conversion_preserves_whole_base_units(decimal packages, int factor, int expected)
        => Assert.Equal(expected, StockQuantity.ToInt32(packages * factor));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task All_save_overloads_apply_user_audit_and_tenant_ownership(bool asynchronous)
    {
        var tenantId = Guid.NewGuid();
        await using var db = CreateContext(tenantId);
        var category = new Category { Name = "category" };
        db.Add(category);
        if (asynchronous) await db.SaveChangesAsync(true); else db.SaveChanges(true);
        Assert.Equal(tenantId, category.TenantId);
        Assert.Equal("test-user", category.CreatedByUserId);
        Assert.NotEqual(default, category.CreatedAt);

        category.Name = "updated";
        if (asynchronous) await db.SaveChangesAsync(); else db.SaveChanges();
        Assert.Equal("test-user", category.UpdatedByUserId);
        Assert.NotNull(category.UpdatedAt);

        db.ChangeTracker.Clear();
        db.Add(new Category { Name = "other tenant", TenantId = Guid.NewGuid() });
        if (asynchronous)
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
        else
            Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Foreign_tenant_updates_and_deletes_are_rejected(bool delete)
    {
        await using var db = CreateContext(Guid.NewGuid());
        var foreign = new Category { Id = Guid.NewGuid(), Name = "foreign", TenantId = Guid.NewGuid(), RowVersion = [1] };
        db.Attach(foreign);
        db.Entry(foreign).State = delete ? EntityState.Deleted : EntityState.Modified;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Adding_and_switching_defaults_keeps_one_default_and_rejects_foreign_product_barcode()
    {
        var tenantId = Guid.NewGuid();
        await using var db = CreateContext(tenantId);
        var p1 = new Product { Name = "p1", Category = new Category { Name = "category" } };
        var p2 = new Product { Name = "p2", Category = p1.Category };
        var barcode = new ProductBarCode { BarCode = "P2", Title = "p2", Product = p2 };
        var u1 = new Unit { Name = "gram" }; var u2 = new Unit { Name = "kilogram" };
        db.AddRange(p1, p2, barcode, u1, u2);
        await db.SaveChangesAsync();
        await using var uow = new UnitOfWork(db);
        var mapper = CreateMapper();
        var units = new RetalSystemAPI.Services.Catalog.Implementations.ProductUnitService(uow, mapper);
        var first = await units.AddUnitToProductAsync(p1.Id, new CreateProductUnitDto { UnitId = u1.Id, ConversionFactor = 1, IsDefault = true });
        var second = await units.AddUnitToProductAsync(p1.Id, new CreateProductUnitDto { UnitId = u2.Id, ConversionFactor = 1000, IsDefault = true });
        Assert.True(first.IsSuccess); Assert.True(second.IsSuccess);
        Assert.Single(await db.ProductUnits.Where(u => u.IsDefault).ToListAsync());
        Assert.True((await units.SetDefaultUnitAsync(first.Data!.Id)).IsSuccess);
        Assert.Equal(first.Data.Id, (await db.ProductUnits.SingleAsync(u => u.IsDefault)).Id);

        var images = new RetalSystemAPI.Services.Catalog.Implementations.ProductImageService(uow, mapper, null!);
        var rejected = await images.AddImageAsync(p1.Id, new CreateProductImageDto { ImageUrl = "/p1.png", BarcodeId = barcode.Id });
        Assert.False(rejected.IsSuccess);
        var image1 = await images.AddImageAsync(p1.Id, new CreateProductImageDto { ImageUrl = "/1.png", IsDefault = true });
        var image2 = await images.AddImageAsync(p1.Id, new CreateProductImageDto { ImageUrl = "/2.png", IsDefault = true });
        Assert.True(image1.IsSuccess); Assert.True(image2.IsSuccess);
        Assert.Single(await db.ProductImages.Where(i => i.IsDefault).ToListAsync());
        Assert.True((await images.SetDefaultImageAsync(image1.Data!.Id)).IsSuccess);
        Assert.Equal(image1.Data.Id, (await db.ProductImages.SingleAsync(i => i.IsDefault)).Id);
    }

    [Fact]
    public void All_tenant_relationships_include_tenant_and_snapshot_matches()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=unused;Database=ModelMetadataOnly;Integrated Security=True").Options);
        var fks = db.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys())
            .Where(f => f.PrincipalEntityType.ClrType != typeof(Tenant)
                && typeof(TenantBaseEntity).IsAssignableFrom(f.PrincipalEntityType.ClrType));
        Assert.All(fks, fk => Assert.Contains(fk.Properties, p => p.Name == nameof(TenantBaseEntity.TenantId)));
        var imageBarcode = db.Model.FindEntityType(typeof(ProductImage))!.GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(ProductBarCode));
        Assert.Contains(imageBarcode.Properties, p => p.Name == "ProductId");
        Assert.False(db.Database.HasPendingModelChanges());
    }

    internal static IMapper CreateMapper() => new MapperConfiguration(c =>
    {
        c.AddProfile<CatalogMappingProfile>(); c.AddProfile<PurchaseMappingProfile>();
    }, NullLoggerFactory.Instance).CreateMapper();

    internal static AppDbContext CreateContext(Guid tenantId) => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .AddInterceptors(new AuditInterceptor(new TestUser(), new TestTenant(tenantId)), new TestVersions())
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options, new TestTenant(tenantId));

    internal sealed class TestTenant(Guid id) : ICurrentTenantService { public Guid TenantId => id; }
    internal sealed class TestUser : ICurrentUserService { public string? UserId => "test-user"; }
    private sealed class TestVersions : SaveChangesInterceptor
    {
        private static void Set(DbContext? db)
        {
            if (db is not null)
                foreach (var e in db.ChangeTracker.Entries<BaseEntity>()) e.Entity.RowVersion ??= [1];
        }
        public override InterceptionResult<int> SavingChanges(DbContextEventData d, InterceptionResult<int> r)
        { Set(d.Context); return r; }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData d, InterceptionResult<int> r, CancellationToken ct = default)
        { Set(d.Context); return ValueTask.FromResult(r); }
    }
}
