using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات تصنيفات ومجموعات الأصناف (Categories).
/// </summary>
public partial class CategoryRepository : Repository<Category>, ICategoryRepository
{
    /// <summary>
    /// تهيئة مستودع التصنيفات وربطه بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }
}
