using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context;

internal static class ModelIntegrityConfiguration
{
    public static void Apply(ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes()
            .Where(e => e.ClrType != typeof(Tenant) && typeof(TenantBaseEntity).IsAssignableFrom(e.ClrType)).ToArray())
            builder.Entity(entity.ClrType).Property(nameof(TenantBaseEntity.TenantId)).IsConcurrencyToken();

        // A reference to tenant-owned data must carry the same tenant as its owner.
        var relationships = builder.Model.GetEntityTypes()
            .Where(e => e.FindProperty(nameof(TenantBaseEntity.TenantId)) != null)
            .SelectMany(e => e.GetForeignKeys())
            .Where(f => f.PrincipalEntityType.ClrType != typeof(Tenant)
                && typeof(TenantBaseEntity).IsAssignableFrom(f.PrincipalEntityType.ClrType))
            .ToArray();

        foreach (var foreignKey in relationships)
        {
            var foreignProperties = new[] { nameof(TenantBaseEntity.TenantId) }
                .Concat(foreignKey.Properties.Select(p => p.Name)).Distinct().ToArray();
            var principalProperties = new[] { nameof(TenantBaseEntity.TenantId) }
                .Concat(foreignKey.PrincipalKey.Properties.Select(p => p.Name)).Distinct().ToArray();

            // A barcode must also belong to the referenced product, when both are present.
            if (foreignKey.PrincipalEntityType.ClrType == typeof(ProductBarCode)
                && foreignKey.DeclaringEntityType.FindProperty(nameof(ProductBarCode.ProductId)) != null)
            {
                foreignProperties = [nameof(TenantBaseEntity.TenantId), nameof(ProductBarCode.ProductId), foreignKey.Properties.Single().Name];
                principalProperties = [nameof(TenantBaseEntity.TenantId), nameof(ProductBarCode.ProductId), nameof(BaseEntity.Id)];
            }

            builder.Entity(foreignKey.DeclaringEntityType.ClrType)
                .HasOne(foreignKey.PrincipalEntityType.ClrType, foreignKey.DependentToPrincipal?.Name)
                .WithMany(foreignKey.PrincipalToDependent?.Name)
                .HasForeignKey(foreignProperties)
                .HasPrincipalKey(principalProperties)
                .OnDelete(foreignKey.DeleteBehavior == DeleteBehavior.SetNull
                    ? DeleteBehavior.Restrict : foreignKey.DeleteBehavior);
        }

        builder.Entity<ProductUnit>().HasIndex(x => new { x.TenantId, x.ProductId })
            .IsUnique().HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0");
        builder.Entity<ProductImage>().HasIndex(x => new { x.TenantId, x.ProductId })
            .IsUnique().HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0");

        foreach (var type in new[] { typeof(PurchaseInvoiceItem), typeof(PurchaseInvoiceItemBreakdown), typeof(PurchaseOrderItem), typeof(PurchaseReturnItem) })
        {
            var entity = builder.Entity(type);
            entity.ToTable(table => table.HasCheckConstraint($"CK_{entity.Metadata.GetTableName()}_BaseQuantity",
                "[Quantity] > 0 AND [Quantity] <= 2147483647 AND [Quantity] = FLOOR([Quantity])"));
        }

        builder.Entity<ProductUnit>().ToTable(t => t.HasCheckConstraint("CK_ProductUnits_ConversionFactor", "[ConversionFactor] > 0"));
        builder.Entity<Unit>().ToTable(t => t.HasCheckConstraint("CK_Units_UnitPackage", "[UnitPackage] > 0"));
        builder.Entity<PurchaseInvoiceItemBreakdown>().ToTable(t => t.HasCheckConstraint(
            "CK_PurchaseInvoiceItemBreakdowns_Packages",
            "[PackageQuantity] > 0 AND [UnitsPerPackage] > 0 AND [Quantity] = [PackageQuantity] * [UnitsPerPackage]"));
    }
}
