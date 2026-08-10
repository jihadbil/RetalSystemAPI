using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RetalSystemAPI.DataAccess.Specifications;

/// <summary>
/// واجهة تخصيص الاستعلامات (Specification Pattern) لتحديد شروط التجميع والتضمين والترتيب والصفحات.
/// </summary>
/// <typeparam name="T">نوع الكيان</typeparam>
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Take { get; }
    int? Skip { get; }
    bool IsPagingEnabled { get; }
}
