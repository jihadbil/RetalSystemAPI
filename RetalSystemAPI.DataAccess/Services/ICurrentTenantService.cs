using System;

namespace RetalSystemAPI.DataAccess.Services;

/// <summary>
/// واجهة توفر معرف المستأجر الحالي (TenantId) المستخرج من سياق الطلب (HTTP Context) لتطبيق الفلترة متعددة المستأجرين (Multi-Tenancy).
/// </summary>
public interface ICurrentTenantService
{
    /// <summary>
    /// المعرف الفريد للمستأجر الحالي صاحب الجلسة، أو Guid.Empty إذا كان الطلب عاماً أو غير مصادق عليه.
    /// </summary>
    Guid TenantId { get; }
}
