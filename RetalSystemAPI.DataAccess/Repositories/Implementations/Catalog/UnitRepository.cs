using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات الوحدات الأساسية للقياس.
/// </summary>
public partial class UnitRepository : Repository<Unit>, IUnitRepository
{
    public UnitRepository(AppDbContext context) : base(context)
    {
    }
}
