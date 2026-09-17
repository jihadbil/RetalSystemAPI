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
            query = query.OrderBy(spec.OrderBy);
        }
        // أو فحص وتطبيق الترتيب التنازلي إذا تم تحديده
        else if (spec.OrderByDescending != null)
        {
            // تطبيق OrderByDescending
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // فحص ما إذا كان ترقيم الصفحات مفعلاً مع تحديد قيمتي Skip و Take
        if (spec.IsPagingEnabled && spec.Skip.HasValue && spec.Take.HasValue)
        {
            // تطبيق تخطي السجلات Skip وجلب عدد محدد Take
            query = query.Skip(spec.Skip.Value).Take(spec.Take.Value);
        }

        // إرجاع كائن الاستعلام النهائي بعد اكتمال تهيئته
        return query;
    }
}
