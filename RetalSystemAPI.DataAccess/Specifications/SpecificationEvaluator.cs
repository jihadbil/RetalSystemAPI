using System.Linq;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Specifications;

/// <summary>
/// فئة مساعدة واستاتيكية لتطبيق مواصفات ISpecification (الفلترة، الـ Includes، الترتيب، والتقسيم) على استعلامات IQueryable في Entity Framework Core.
/// </summary>
/// <typeparam name="T">نوع الكيان الذي يرث من BaseEntity</typeparam>
public static class SpecificationEvaluator<T> where T : BaseEntity
{
    /// <summary>
    /// بناء استعلام IQueryable النهائي بتطبيق معايير المواصفة المحددة على الاستعلام المصدري.
    /// </summary>
    /// <param name="inputQuery">استعلام البداية (عادةً DbSet)</param>
    /// <param name="spec">كائن المواصفة المراد تطبيقه</param>
    /// <returns>استعلام IQueryable مهيأ بجميع الشروط والتضمينات والترتيب</returns>
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
    {
        var query = inputQuery;

        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        // تطبيق Lambda Includes
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        // تطبيق String Includes
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        if (spec.IsPagingEnabled && spec.Skip.HasValue && spec.Take.HasValue)
        {
            query = query.Skip(spec.Skip.Value).Take(spec.Take.Value);
        }

        return query;
    }
}
