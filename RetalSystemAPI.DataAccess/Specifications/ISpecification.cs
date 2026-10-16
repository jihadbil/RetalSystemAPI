using System.Linq.Expressions;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفات داخلية لتغليف شروط الاستعلامات والتضمينات والترتيب. تُطبّق الصفحات داخل المستودع المتخصص.
/// </summary>
/// <typeparam name = "T">نوع الكيان المراد الاستعلام عنه</typeparam>
internal interface ISpecification<T>
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
}
