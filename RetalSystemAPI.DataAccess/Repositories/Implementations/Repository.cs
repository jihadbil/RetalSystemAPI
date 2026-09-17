using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// التنفيذ العام المستند إلى Entity Framework Core لواجهة المستودع IRepository.
/// يوفر عمليات القراءة السريعة غير المتتبعة وعمليات الكتابة الآمنة مع معالجة الكيانات المحلية والمفصولة.
/// </summary>
/// <typeparam name="T">نوع الكيان الذي يرث من BaseEntity</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    /// <summary>سياق قاعدة البيانات الأساسي</summary>
    protected readonly AppDbContext _context;

    /// <summary>مجموعة الكيانات لـ DbSet التابعة لنوع الكيان الحالي</summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// بناء كائن المستودع وربطه بسياق قاعدة البيانات AppDbContext.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات</param>
    public Repository(AppDbContext context)
    {
        // حفظ مرجع سياق قاعدة البيانات
        _context = context;
        // استخراج مرجع DbSet المخصص للنوع T من السياق
        _dbSet = context.Set<T>();
    }

    // ── Queries ──────────────────────────────────────────────

    /// <summary>
    /// جلب كيان محدد بواسطة المعرف الأساسي (Primary Key).
    /// </summary>
    /// <param name="id">المعرف الفريد للكيان</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>الكيان المطلوب أو null إذا لم يوجد</returns>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // البحث عن الكيان في الذاكرة المحلية أولاً ثم في قاعدة البيانات عبر المفتاح الأساسي
        return await _dbSet.FindAsync(new object[] { id }, ct);
    }

    /// <summary>
    /// جلب جميع الكيانات غير المحذوفة كقائمة غير متتبعة للقراءة السريعة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>قائمة غير متتبعة للقراءة السريعة لكافة الكيانات</returns>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        // تعطيل التتبع AsNoTracking لتحسين الأداء وتقليل استهلاك الذاكرة ثم جلب القائمة
        return await _dbSet.AsNoTracking().ToListAsync(ct);
    }

    /// <summary>
    /// جلب جميع الكيانات مع تفعيل التتبع في ChangeTracker لإجراء التعديلات.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>قائمة الكيانات مع تتبع التغييرات للتعديل</returns>
    public async Task<IReadOnlyList<T>> GetAllTrackedAsync(CancellationToken ct = default)
    {
        // جلب جميع الكيانات مع إبقاء تتبع ChangeTracker مفعلاً للتعديل اللاحق
        return await _dbSet.ToListAsync(ct);
    }

    /// <summary>
    /// البحث عن الكيانات المطابقة لشرط الفلترة بدون تتبع.
    /// </summary>
    /// <param name="predicate">تعبير شرط الفلترة</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>قائمة الكيانات المطابقة للشرط</returns>
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        // تطبيق شرط الفلترة Where بدون تتبع وإرجاع النتائج في قائمة
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);
    }

    /// <summary>
    /// البحث عن الكيانات باستخدام مواصفة متقدمة (ISpecification) وتطبيق التضمينات والترتيب.
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>قائمة الكيانات المطابقة للمواصفة</returns>
    public async Task<IReadOnlyList<T>> FindAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        // بناء الاستعلام وتطبيق المواصفة ثم تعطيل التتبع وإرجاع النتائج
        return await ApplySpecification(spec).AsNoTracking().ToListAsync(ct);
    }

    /// <summary>
    /// جلب أول كيان يطابق الشرط أو null.
    /// </summary>
    /// <param name="predicate">تعبير شرط الفلترة</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>الكيان الأول المطابق أو null</returns>
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        // استرجاع السجل الأول المطابق للشرط بدون تتبع
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, ct);
    }

    /// <summary>
    /// جلب أول كيان يطابق المواصفة أو null.
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>الكيان الأول المطابق للمواصفة أو null</returns>
    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        // تطبيق المواصفة بدون تتبع وجلب أول سجل مطابق
        return await ApplySpecification(spec).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// جلب أول كيان يطابق المواصفة مع تفعيل التتبع في ChangeTracker لإجراء التعديلات وحفظها بأمان.
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>الكيان الأول المطابق مع التتبع</returns>
    public async Task<T?> FirstOrDefaultTrackedAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        // تطبيق المواصفة مع إبقاء التتبع مفعلاً وجلب السجل الأول للتعديل
        return await ApplySpecification(spec).FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// جلب أول كيان يطابق شرطاً مع تفعيل التتبع في ChangeTracker.
    /// </summary>
    /// <param name="predicate">تعبير شرط الفلترة</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>الكيان الأول المطابق مع التتبع</returns>
    public async Task<T?> FirstOrDefaultTrackedAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        // جلب أول سجل يطابق الشرط مع إبقاء تتبع ChangeTracker للتعديل
        return await _dbSet.FirstOrDefaultAsync(predicate, ct);
    }

    /// <summary>
    /// البحث عن الكيانات المطابقة لشرط الفلترة مع تفعيل التتبع، لجلب دفعة كيانات
    /// سيتم تعديلها في استعلام واحد بدلاً من استعلام لكل كيان.
    /// </summary>
    /// <param name="predicate">تعبير شرط الفلترة</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>قائمة الكيانات المتتبعة المطابقة للشرط</returns>
    public async Task<IReadOnlyList<T>> FindTrackedAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        // تصفية السجلات وجلبها بالكامل مع تفعيل التتبع للتعديل الجماعي
        return await _dbSet.Where(predicate).ToListAsync(ct);
    }

    /// <summary>
    /// تحميل أعمدة محددة فقط (Projection) دون الكيانات الكاملة لتفادي تحميل
    /// جداول كاملة في الذاكرة عند الحاجة لحقول قليلة.
    /// </summary>
    /// <typeparam name="TResult">نوع كائن الإسقاط</typeparam>
    /// <param name="selector">تعبير اختيار الحقول المراد إسقاطها</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>قائمة الكائنات الناتجة عن الإسقاط</returns>
    public async Task<IReadOnlyList<TResult>> SelectAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        CancellationToken ct = default)
    {
        // تطبيق الإسقاط Select مباشرة في استعلام SQL بدون تتبع لجلب الحقول المحددة فقط
        return await _dbSet.AsNoTracking().Select(selector).ToListAsync(ct);
    }

    /// <summary>
    /// التحقق من وجود أي سجل يطابق الشرط المنطقي.
    /// </summary>
    /// <param name="predicate">تعبير الشرط المنطقي</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>true إذا وجد أي سجل مطابق، وإلا false</returns>
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        // تنفيذ استعلام Any في قاعدة البيانات للتحقق السريع من الوجود
        return await _dbSet.AnyAsync(predicate, ct);
    }

    /// <summary>
    /// حساب عدد السجلات المطابقة لشرط اختياري.
    /// </summary>
    /// <param name="predicate">شرط الفلترة الاختياري</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>إجمالي عدد السجلات</returns>
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        // حساب إجمالي الجدول إذا كان الشرط فارغاً، أو حساب السجلات المطابقة للشرط
        return predicate is null
            ? await _dbSet.CountAsync(ct)
            : await _dbSet.CountAsync(predicate, ct);
    }

    /// <summary>
    /// حساب عدد السجلات المطابقة لمعايير المواصفة.
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>عدد السجلات المطابقة للمواصفة</returns>
    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        // تجهيز استعلام بدون تتبع
        IQueryable<T> query = _dbSet.AsNoTracking();
        // تطبيق شرط المواصفة إذا كان محدداً
        if (spec.Criteria is not null)
        {
            // تصفية الاستعلام بشرط المواصفة
            query = query.Where(spec.Criteria);
        }
        // تنفيذ عد السجلات في قاعدة البيانات
        return await query.CountAsync(ct);
    }

    // ── Pagination ────────────────────────────────────────────

    /// <summary>
    /// جلب صفحة بيانات مجزأة مع الفلترة والترتيب التلقائي.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة (1-indexed)</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="filter">شرط فلترة اختياري</param>
    /// <param name="orderBy">تعبير حقل الترتيب الاختياري</param>
    /// <param name="ascending">ترتيب تصاعدي (true) أم تنازلي (false)</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>عناصر الصفحة المحددة والعدد الكلي للسجلات</returns>
    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken ct = default)
    {
        // تجهيز استعلام بدون تتبع للقراءة السريعة
        IQueryable<T> query = _dbSet.AsNoTracking();

        // تطبيق الفلتر إذا تم تمريره
        if (filter is not null)
        {
            // إضافة شرط الفلترة
            query = query.Where(filter);
        }

        // حساب إجمالي عدد السجلات المطابقة قبل التقطيع
        int totalCount = await query.CountAsync(ct);

        // تطبيق الترتيب إذا تم تمريره
        if (orderBy is not null)
        {
            // الترتيب تصاعدياً أو تنازلياً حسب المعامل الممرر
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        // تخطي السجلات السابقة وجلب عدد عناصر الصفحة الحالية
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // إرجاع النتيجة كثنائية (العناصر، الإجمالي)
        return (items, totalCount);
    }

    /// <summary>
    /// جلب صفحة بيانات مجزأة بالاعتماد على مواصفة متقدمة (ISpecification).
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>عناصر الصفحة وإجمالي عدد السجلات المطابقة</returns>
    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        ISpecification<T> spec,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        // حساب إجمالي عدد السجلات المطابقة لمعايير المواصفة
        int totalCount = await CountAsync(spec, ct);

        // تطبيق المواصفة مع تقسيم الاستعلام لتفادي التضخم الناتج عن التضمينات المتعددة
        var query = ApplySpecification(spec).AsNoTracking().AsSplitQuery();
        // تقطيع وتحديد عناصر الصفحة المطلوبة
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // إرجاع قائمة العناصر والعدد الإجمالي
        return (items, totalCount);
    }

    /// <summary>
    /// تطبيق معايير وتضمينات المواصفة على استعلام IQueryable عبر SpecificationEvaluator.
    /// </summary>
    /// <param name="spec">كائن المواصفة</param>
    /// <returns>استعلام مهيأ بالمواصفة</returns>
    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        // تفويض تطبيق شروط المواصفة والـ Includes والترتيب لمقيم المواصفات
        return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
    }

    // ── Commands ──────────────────────────────────────────────

    /// <summary>
    /// إضافة كيان جديد إلى سياق قاعدة البيانات.
    /// </summary>
    /// <param name="entity">الكيان المراد إضافته</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        // إضافة الكيان إلى مجموعة DbSet بصورة غير متزامنة
        await _dbSet.AddAsync(entity, ct);
    }

    /// <summary>
    /// إضافة مجموعة من الكيانات الجديدة إلى سياق قاعدة البيانات.
    /// </summary>
    /// <param name="entities">قائمة الكيانات المراد إضافتها</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        // إضافة دفعة الكيانات إلى DbSet دفعة واحدة
        await _dbSet.AddRangeAsync(entities, ct);
    }

    /// <summary>
    /// تحديث بيانات الكيان مع معالجة الكيانات المتتبعة محلياً في الذاكرة لمنع أخطاء تضارب التتبع.
    /// </summary>
    /// <param name="entity">الكيان المراد تحديث بياناته</param>
    public void Update(T entity)
    {
        // فحص ما إذا كان الكيان متتبعاً بالفعل في الذاكرة المحلية لـ ChangeTracker بواسطة Id
        var local = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        // في حال وجود نسخة محلية مسبقة لنفس المعرف
        if (local != null)
        {
            // إذا كان الكيان الممرر ليس نفس المرجع المرجعي الدقيق للكائن المحلي
            if (!ReferenceEquals(local, entity))
            {
                // نسخ القيم الجديدة من الكيان الممرر إلى الكائن المتتبع محلياً
                _context.Entry(local).CurrentValues.SetValues(entity);
            }
            else
            {
                // إذا كان نفس المرجع، يتم وسم حالته صراحة بأنه معدل
                _context.Entry(local).State = EntityState.Modified;
            }
            // إنهاء الدالة بعد معالجة النسخة المحلية بنجاح
            return;
        }

        // في حال عدم وجود الكيان محلياً، يتم فحص حالة الإدخال
        var entry = _context.Entry(entity);
        // إذا كان الكيان منفصلاً Detached
        if (entry.State == EntityState.Detached)
        {
            // إرفاق الكيان بسياق البيانات
            _dbSet.Attach(entity);
        }
        // وسم حالة الكيان بأنه معدل ليتم توليد أمر UPDATE في قاعدة البيانات عند الحفظ
        entry.State = EntityState.Modified;
    }

    /// <summary>
    /// الحذف المنطقي للكيان بتعيين IsDeleted = true وتحديث حالته في سياق قاعدة البيانات.
    /// </summary>
    /// <param name="entity">الكيان المراد حذفه منطقياً</param>
    public void SoftDelete(T entity)
    {
        // وسم خاصية الحذف بالكيان لتصبح true
        entity.IsDeleted = true;
        // فحص وجود الكيان في الذاكرة المحلية
        var local = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        // في حال العثور عليه محلياً
        if (local != null)
        {
            // إذا كانت المراجع مختلفة
            if (!ReferenceEquals(local, entity))
            {
                // نسخ القيم المحدثة (ومنها IsDeleted = true) إلى النسخة المتتبعة محلياً
                _context.Entry(local).CurrentValues.SetValues(entity);
            }
            else
            {
                // وسم النسخة المحلية كمعدلة
                _context.Entry(local).State = EntityState.Modified;
            }
            // إنهاء الدالة
            return;
        }

        // فحص حالة الكيان إذا لم يكن موجوداً بالذاكرة المحلية
        var entry = _context.Entry(entity);
        // إرفاق الكيان إذا كان منفصلاً
        if (entry.State == EntityState.Detached)
        {
            // إرفاقه بمجموعة الكيانات
            _dbSet.Attach(entity);
        }
        // وسمه كمعدل ليتم حفظ حالة الحذف المنطقي
        entry.State = EntityState.Modified;
    }

    /// <summary>
    /// الحذف الفعلي والنهائي للكيان من جدول قاعدة البيانات.
    /// </summary>
    /// <param name="entity">الكيان المراد حذفه نهائياً</param>
    public void HardDelete(T entity)
    {
        // البحث عن الكيان في الذاكرة المحلية
        var local = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        // في حال وجوده محلياً
        if (local != null)
        {
            // حذفه فيزيائياً من مجموعة الكيانات
            _dbSet.Remove(local);
        }
        else
        {
            // فحص إدخال الكيان إذا لم يكن محلياً
            var entry = _context.Entry(entity);
            // إذا كان الكيان متتبعاً بالفعل
            if (entry.State != EntityState.Detached)
            {
                // إزالته مباشرة
                _dbSet.Remove(entity);
            }
            else
            {
                // إرفاقه أولاً بالسياق ثم حذفه
                _context.Attach(entity);
                // إزالة الكيان فيزيائياً
                _dbSet.Remove(entity);
            }
        }
    }
}
