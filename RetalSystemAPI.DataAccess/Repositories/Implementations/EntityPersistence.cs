using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
internal static class EntityPersistence
{
    public static async Task<T?> GetReadOnlyByIdAsync<T>(AppDbContext context, Guid id, CancellationToken ct)
        where T : BaseEntity
    {
        ct.ThrowIfCancellationRequested();
        var local = context.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == id);
        if (local is not null && (local.State == EntityState.Deleted || local.Entity.IsDeleted || (local.Entity is TenantBaseEntity owned && local.Entity is not Tenant && owned.TenantId != context.CurrentTenantId)))
            return null;
        return await context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public static async Task<T?> GetByIdAsync<T>(AppDbContext context, Guid id, CancellationToken ct)
        where T : BaseEntity
    {
        ct.ThrowIfCancellationRequested();
        var local = context.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == id);
        if (local is not null)
        {
            var entity = local.Entity;
            return local.State != EntityState.Deleted && !entity.IsDeleted && (entity is not TenantBaseEntity owned || entity is Tenant || owned.TenantId == context.CurrentTenantId) ? entity : null;
        }

        return await context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public static void Update<T>(AppDbContext context, T entity)
        where T : BaseEntity
    {
        context.EnsureTransactionUsable();
        var entry = context.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == entity.Id) ?? context.Entry(entity);
        if (entry.State == EntityState.Deleted)
            throw new InvalidOperationException("لا يمكن تعديل سجل محدد للحذف النهائي.");
        if (entry.State == EntityState.Detached)
            context.Set<T>().Attach(entity);
        if (!ReferenceEquals(entry.Entity, entity))
        {
            var incoming = context.Entry(entity).CurrentValues;
            // Keys include tenant ownership. Never silently discard a change to a key.
            if (entry.Properties.Any(p => p.Metadata.IsKey() && !Equals(p.CurrentValue, incoming[p.Metadata.Name])))
                throw new UnauthorizedAccessException("لا يمكن تغيير ملكية السجل أو مفاتيحه.");
            if (entry.State != EntityState.Added)
                entry.Property(e => e.RowVersion).OriginalValue = entity.RowVersion?.ToArray() ?? throw new DbUpdateConcurrencyException("نسخة التزامن مطلوبة لتعديل السجل.");
            foreach (var property in entry.Properties.Where(p => IsWritable(p.Metadata)))
                property.CurrentValue = incoming[property.Metadata.Name];
        }

        // An unsaved entity must remain an INSERT, even when updated or soft deleted.
        if (entry.State != EntityState.Added)
            foreach (var property in entry.Properties.Where(p => IsWritable(p.Metadata)))
                property.IsModified = true;
    }

    public static void SoftDelete<T>(AppDbContext context, T entity)
        where T : BaseEntity
    {
        entity.IsDeleted = true;
        Update(context, entity);
    }

    public static void HardDelete<T>(AppDbContext context, T entity)
        where T : BaseEntity
    {
        Update(context, entity);
        var tracked = context.ChangeTracker.Entries<T>().Single(e => e.Entity.Id == entity.Id).Entity;
        context.Set<T>().Remove(tracked);
    }

    private static bool IsWritable(IProperty property) => !property.IsKey() && property.GetAfterSaveBehavior() == PropertySaveBehavior.Save && property.Name is not nameof(BaseEntity.RowVersion) and not nameof(BaseEntity.CreatedAt) and not nameof(BaseEntity.CreatedByUserId) and not nameof(TenantBaseEntity.TenantId);
}
