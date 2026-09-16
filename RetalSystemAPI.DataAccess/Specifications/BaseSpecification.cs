using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RetalSystemAPI.DataAccess.Specifications;

/// <summary>
/// فئة أساسية مجردة (Abstract Base Class) توفر الدوال والخصائص الجاهزة لبناء المواصفات المخصصة بسهولة.
/// </summary>
/// <typeparam name="T">نوع الكيان</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// بناء مواصفة عامة بدون شروط فلترة افتراضية.
    /// </summary>
    public BaseSpecification()
    {
    }

    /// <summary>
    /// بناء مواصفة مع تحديد شرط الفلترة الأساسي (Criteria).
    /// </summary>
    /// <param name="criteria">تعبير الشرط المنطقي للفلترة</param>
    public BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <inheritdoc />
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc />
    public List<Expression<Func<T, object>>> Includes { get; } = new();

    /// <inheritdoc />
    public List<string> IncludeStrings { get; } = new();

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc />
    public int? Take { get; private set; }

    /// <inheritdoc />
    public int? Skip { get; private set; }

    /// <inheritdoc />
    public bool IsPagingEnabled { get; private set; }

    /// <summary>
    /// إضافة تعبير لتضمين كيان مرتبط (Navigation Property) في الاستعلام عبر Expression.
    /// </summary>
    /// <param name="includeExpression">تعبير خاصية التنقل</param>
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// إضافة مسار نصي لتضمين كيانات متداخلة متعددة المستويات في الاستعلام (مثل "Items.Breakdowns").
    /// </summary>
    /// <param name="includeString">المسار النصي لخاصية التنقل</param>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// تطبيق ترتيب تصاعدي على نتائج الاستعلام.
    /// </summary>
    /// <param name="orderByExpression">تعبير الحقل المراد الترتيب بموجبه</param>
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// تطبيق ترتيب تنازلي على نتائج الاستعلام.
    /// </summary>
    /// <param name="orderByDescendingExpression">تعبير الحقل المراد الترتيب بموجبه تنازلياً</param>
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    /// <summary>
    /// تفعيل وتطبيق ترقيم الصفحات (Paging) على نتائج الاستعلام.
    /// </summary>
    /// <param name="skip">عدد السجلات المطلوب تجاوزها</param>
    /// <param name="take">عدد السجلات المطلوب جلبها في الصفحة</param>
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}
