using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات المخازن والمستودعات والصالات (Warehouses).
/// </summary>
public partial class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    /// <summary>
    /// تهيئة نسخة جديدة من مستودع بيانات المستودعات.
    /// </summary>
    /// <param name="context">سياق قاعدة بيانات النظام للوصول إلى الجداول وحفظ الحركات.</param>
    public WarehouseRepository(AppDbContext context) : base(context)
    {
        // تمرير سياق البيانات إلى الفئة الأساسية Repository
    }
}
