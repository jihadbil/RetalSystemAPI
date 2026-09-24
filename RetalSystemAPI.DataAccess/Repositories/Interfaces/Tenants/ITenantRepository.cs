using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات المستأجرين (Tenants) لإدارة الشركات والمنظمات المشتركة في النظام.
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
}
