using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع عامة توفر العمليات الأساسية لقراءة وكتابة البيانات في قاعدة البيانات بنمط المستودعات (Repository Pattern).
/// </summary>
/// <typeparam name="T">نوع الكيان المراد التعامل معه، ويجب أن يرث من BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    // ── Queries ──────────────────────────────────────────────
    
    /// <summary>
    /// جلب كيان محدد بواسطة معرفه الفريد (Id) بدون تتبع للتعديلات (AsNoTracking).
    /// </summary>
    /// <param name="id">المعرف الفريد للكيان</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>الكيان المطلوب أو null إذا لم يتم العثور عليه</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب جميع الكيانات غير المحذوفة بدون تتبع للتعديلات (AsNoTracking).
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بجميع الكيانات</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب جميع الكيانات مع تفعيل تتبع التغييرات (Tracked) لإجراء عمليات تعديل جماعية.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بالكيانات مع التتبع</returns>
    Task<IReadOnlyList<T>> GetAllTrackedAsync(CancellationToken ct = default);

    /// <summary>
    /// البحث عن كيانات مطابقة لشرط منطقي محدد.
    /// </summary>
    /// <param name="predicate">تعبير الشرط المنطقي للفلترة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة الكيانات المطابقة للشرط</returns>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// البحث عن كيانات باستخدام مواصفة متقدمة (Specification Pattern) تشمل الفلترة والتضمينات والترتيب.
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة الكيانات المطابقة للمواصفة</returns>
    Task<IReadOnlyList<T>> FindAsync(ISpecification<T> spec, CancellationToken ct = default);

    /// <summary>
    /// جلب أول كيان يطابق شرطاً محدداً، أو القيمة الافتراضية null.
    /// </summary>
    /// <param name="predicate">تعبير الشرط المنطقي</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>الكيان الأول المطابق أو null</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// جلب أول كيان يطابق مواصفة محددة، أو القيمة الافتراضية null.
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>الكيان الأول المطابق أو null</returns>
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default);

    /// <summary>
    /// جلب أول كيان يطابق مواصفة محددة مع تفعيل التتبع (Tracked).
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>الكيان الأول المطابق مع التتبع أو null</returns>
    Task<T?> FirstOrDefaultTrackedAsync(ISpecification<T> spec, CancellationToken ct = default);

    /// <summary>
    /// جلب أول كيان يطابق شرطاً محدداً مع تفعيل التتبع (Tracked).
    /// </summary>
    /// <param name="predicate">تعبير الشرط المنطقي</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>الكيان الأول المطابق مع التتبع أو null</returns>
    Task<T?> FirstOrDefaultTrackedAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// البحث عن كيانات مطابقة لشرط منطقي مع تفعيل التتبع (Tracked)، لجلب دفعة واحدة من الكيانات
    /// المراد تعديلها لاحقاً بدلاً من استعلام منفصل لكل كيان (معالجة نمط N+1).
    /// </summary>
    /// <param name="predicate">تعبير الشرط المنطقي للفلترة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة الكيانات المطابقة مع التتبع</returns>
    Task<IReadOnlyList<T>> FindTrackedAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// تحميل أعمدة محددة فقط (Projection) دون الكيانات الكاملة، لتفادي تحميل جداول
    /// كاملة في الذاكرة عندما تُستهلك حقول قليلة فقط (مثل المفاتيح والأسماء).
    /// </summary>
    /// <typeparam name="TResult">نوع النتيجة النحيفة (شكل مُسقط أو نوع بسيط)</typeparam>
    /// <param name="selector">تعبير تحديد الأعمدة المطلوبة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة النتائج النحيفة</returns>
    Task<IReadOnlyList<TResult>> SelectAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        CancellationToken ct = default);

    /// <summary>
    /// التحقق من وجود أي كيان يطابق شرطاً محدداً.
    /// </summary>
    /// <param name="predicate">تعبير الشرط المنطقي</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>true إذا وجد أي سجل مطابق، وإلا false</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// حساب العدد الإجمالي للكيانات المطابقة لشرط اختياري.
    /// </summary>
    /// <param name="predicate">شرط الفلترة الاختياري</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>عدد السجلات المطابقة</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>
    /// حساب العدد الإجمالي للكيانات المطابقة لمواصفة محددة.
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>عدد السجلات المطابقة للمواصفة</returns>
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);

    // ── Pagination ────────────────────────────────────────────

    /// <summary>
    /// جلب صفحة بيانات مجزأة (Pagination) مع دعم الفلترة والترتيب التلقائي.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوب جلبها (1-indexed)</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="filter">شرط فلترة اختياري</param>
    /// <param name="orderBy">تعبير حقل الترتيب</param>
    /// <param name="ascending">ترتيب تصاعدي (true) أم تنازلي (false)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة عناصر الصفحة مع إجمالي عدد السجلات الكلي</returns>
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة (Pagination) باستخدام مواصفة متقدمة (Specification).
    /// </summary>
    /// <param name="spec">كائن المواصفة المتضمن لشروط الفلترة والتضمينات</param>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>عناصر الصفحة وإجمالي عدد السجلات الكلي المطابق للمواصفة</returns>
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        ISpecification<T> spec,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    // ── Commands ──────────────────────────────────────────────

    /// <summary>
    /// إضافة كيان جديد إلى سياق قاعدة البيانات بصورة غير متزامنة.
    /// </summary>
    /// <param name="entity">الكيان المراد إضافته</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة كيانات جديدة دفعة واحدة إلى سياق قاعدة البيانات.
    /// </summary>
    /// <param name="entities">قائمة الكيانات المراد إضافتها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>
    /// تعليم كيان موجود كـ "معدل" لتحديث بياناته عند الحفظ.
    /// </summary>
    /// <param name="entity">الكيان المراد تحديثه</param>
    void Update(T entity);

    /// <summary>
    /// الحذف المنطقي للكيان (Soft Delete) بتعيين IsDeleted = true دون مسحه فعلياً من قاعدة البيانات.
    /// </summary>
    /// <param name="entity">الكيان المراد حذفه منطقياً</param>
    void SoftDelete(T entity);

    /// <summary>
    /// الحذف النهائي للكيان (Hard Delete) وإزالته فيزيائياً من جدول قاعدة البيانات.
    /// </summary>
    /// <param name="entity">الكيان المراد حذفه نهائياً</param>
    void HardDelete(T entity);
}
