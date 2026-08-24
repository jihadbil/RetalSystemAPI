using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Interceptors;

/// <summary>
/// Interceptor للتعبئة التلقائية لبيانات التتبع والتدقيق (CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId, TenantId, Id).
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenantService;

    public AuditInterceptor(
        ICurrentUserService currentUser,
        ICurrentTenantService tenantService)
    {
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var userId = _currentUser.UserId;
        var currentTenantId = _tenantService.TenantId;

        foreach (var entry in eventData.Context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.Id == Guid.Empty)
                {
                    entry.Entity.Id = Guid.NewGuid();
                }

                if (entry.Entity is TenantBaseEntity tenantEntity && tenantEntity.TenantId == Guid.Empty && currentTenantId != Guid.Empty)
                {
                    tenantEntity.TenantId = currentTenantId;
                }

                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                entry.Entity.CreatedByUserId = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedByUserId = userId;

                // منع التعديل على بيانات الإنشاء
                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Property(x => x.CreatedByUserId).IsModified = false;
                if (entry.Entity is TenantBaseEntity)
                {
                    entry.Property(nameof(TenantBaseEntity.TenantId)).IsModified = false;
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
