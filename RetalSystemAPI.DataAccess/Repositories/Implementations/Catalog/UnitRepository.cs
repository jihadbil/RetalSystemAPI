using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات الوحدات الأساسية للقياس والتعبئة (Units).
/// </summary>
public partial class UnitRepository : Repository<Unit>, IUnitRepository
{
    /// <summary>
    /// تهيئة مستودع الوحدات وربطه بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public UnitRepository(AppDbContext context) : base(context)
    {
    }
}
