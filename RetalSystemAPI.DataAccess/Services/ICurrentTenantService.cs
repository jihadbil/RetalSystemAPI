using System;

namespace RetalSystemAPI.DataAccess.Services;

/// <summary>
/// واجهة توفر معرف المستأجر الحالي المستخرج من سياق الطلب (HTTP Context).
/// </summary>
public interface ICurrentTenantService
{
    Guid TenantId { get; }
}
