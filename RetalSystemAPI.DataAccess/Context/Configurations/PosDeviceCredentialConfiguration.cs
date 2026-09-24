using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Accounting;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class PosDeviceCredentialConfiguration : IEntityTypeConfiguration<PosDeviceCredential>
{
    public void Configure(EntityTypeBuilder<PosDeviceCredential> builder)
    {
        builder.ToTable("PosDeviceCredentials");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(64);
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasOne(x => x.PosTerminal).WithMany(x => x.DeviceCredentials).HasForeignKey(x => x.PosTerminalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.PosTerminalId, x.RevokedAt });
    }
}
