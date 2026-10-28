using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات المستأجرين (Tenants) وإدارة بيانات المنشآت والشركات.
/// </summary>
public partial class TenantRepository : Repository<Tenant>, ITenantRepository
{
    /// <summary>
    /// تهيئة مستودع المستأجرين وربطه بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public TenantRepository(AppDbContext context) : base(context)
    {
    }
}
