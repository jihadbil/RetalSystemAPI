using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public sealed class UserLoginIdentifierConfiguration : IEntityTypeConfiguration<UserLoginIdentifier>
{
    public void Configure(EntityTypeBuilder<UserLoginIdentifier> builder)
    {
        builder.ToTable("UserLoginIdentifiers");
        builder.HasKey(x => x.NormalizedValue);
        builder.Property(x => x.NormalizedValue).HasMaxLength(256);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
