using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مساعد داخلي لتطبيق شروط المواصفات والتضمينات والترتيب على استعلامات Entity Framework Core.
/// </summary>
/// <typeparam name = "T">نوع الكيان الذي يرث من BaseEntity</typeparam>
internal static class SpecificationEvaluator<T>
    where T : BaseEntity
{
    /// <summary>
    /// بناء استعلام IQueryable النهائي بتطبيق معايير المواصفة المحددة على الاستعلام المصدري.
    /// </summary>
    /// <param name = "inputQuery">استعلام البداية (عادةً DbSet)</param>
    /// <param name = "spec">كائن المواصفة المراد تطبيقه</param>
    /// <returns>استعلام IQueryable مهيأ بجميع الشروط والتضمينات والترتيب</returns>
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
    {
        // تهيئة الاستعلام الأولي بالاستعلام الممرر
        var query = inputQuery;
        // تطبيق معيار الفلترة Where إذا كان محدداً في المواصفة
        if (spec.Criteria != null)
        {
            // إضافة شرط الفلترة للاستعلام
            query = query.Where(spec.Criteria);
        }

        // تطبيق تضمينات Lambda Includes عبر التجميع التكراري Aggregate
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        // تطبيق التضمينات النصية String Includes للمسارات المتداخلة
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));
        // فحص وتطبيق الترتيب التصاعدي إذا تم تحديده
        if (spec.OrderBy != null)
        {
            // تطبيق OrderBy
            query = query.OrderBy(spec.OrderBy).ThenBy(e => e.Id);
        }
        // أو فحص وتطبيق الترتيب التنازلي إذا تم تحديده
        else if (spec.OrderByDescending != null)
        {
            // تطبيق OrderByDescending
            query = query.OrderByDescending(spec.OrderByDescending).ThenBy(e => e.Id);
        }
        else
        {
            query = query.OrderBy(e => e.Id);
        }

        // إرجاع كائن الاستعلام النهائي بعد اكتمال تهيئته
        return query;
    }
}
