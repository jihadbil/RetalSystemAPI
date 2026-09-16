using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RetalSystemAPI.DataAccess.Specifications;

/// <summary>
/// واجهة نمط المواصفات (Specification Pattern) لتغليف شروط الاستعلامات، التضمينات (Eager Loading)، الترتيب، والتقسيم في كائنات قابلة لإعادة الاستخدام.
/// </summary>
/// <typeparam name="T">نوع الكيان المراد الاستعلام عنه</typeparam>
public interface ISpecification<T>
{
    /// <summary>تعبير الشرط المنطقي الأساسي للفلترة (Where Criteria)</summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>قائمة التضمينات المعبر عنها بـ Expression (Lambda Includes)</summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>قائمة التضمينات المعبر عنها بمسارات نصية (String Includes للكيانات المتداخلة)</summary>
    List<string> IncludeStrings { get; }

    /// <summary>تعبير الترتيب التصاعدي (OrderBy)</summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>تعبير الترتيب التنازلي (OrderByDescending)</summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>عدد السجلات المطلوب جلبها (Take)</summary>
    int? Take { get; }

    /// <summary>عدد السجلات المطلوب تخطيها (Skip)</summary>
    int? Skip { get; }

    /// <summary>هل تم تفعيل تقسيم النتائج إلى صفحات (Paging)</summary>
    bool IsPagingEnabled { get; }
}
