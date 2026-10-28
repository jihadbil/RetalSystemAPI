using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// الفئة الأساسية لتنفيذ المستودع العام للبيانات (Generic Repository Implementation).
/// تقدم العمليات الشائعة لجميع الكيانات بالاعتماد على سياق EF Core ومحرك استمرارية الكيانات.
/// </summary>
/// <typeparam name="T">نوع الكيان المشتق من BaseEntity.</typeparam>
public abstract class Repository<T> : IRepository<T> where T : BaseEntity
{
    /// <summary>سياق قاعدة البيانات المشترك</summary>
    protected readonly AppDbContext _context;

    /// <summary>مجموعة الكيانات المخصصة للنوع T في سياق EF Core</summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// تهيئة المستودع وربطه بسياق قاعدة البيانات ومجموعة الكيانات المقابلة.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات الحالي.</param>
    protected Repository(AppDbContext context)
    {
        // حفظ المرجع لسياق قاعدة البيانات
        _context = context;
        // استخراج مجموعة الكيانات المقابلة للنوع T
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// جلب كيان محدد كقراءة فقط بدون تتبع مع التحقق من قيود المستأجر والحذف المنطقي.
    /// </summary>
    /// <param name="id">المعرف الفريد للكيان.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>الكيان كقراءة فقط أو null.</returns>
    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // تفويض القراءة غير المتتبعة إلى محرك استمرارية الكيانات الموحد
        return EntityPersistence.GetReadOnlyByIdAsync<T>(_context, id, ct);
    }

    /// <summary>
    /// جلب الكيان متتبعاً للتعديل مع فحصه في الذاكرة المحلية أولاً.
    /// </summary>
    /// <param name="id">المعرف الفريد للكيان.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>الكيان المتتبع أو null.</returns>
    public Task<T?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع الكيان متتبعاً للذاكرة للبدء في إجراء التعديلات عليه
        return EntityPersistence.GetByIdAsync<T>(_context, id, ct);
    }

    /// <summary>
    /// فحص وجود الكيان في قاعدة البيانات بالاعتماد على المعرف الفريد.
    /// </summary>
    /// <param name="id">المعرف الفريد للكيان.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد السجل، وإلا False.</returns>
    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        // فحص وجود أي سجل يطابق المعرف المعطى بشكل خفيف وسريع
        return _dbSet.AnyAsync(e => e.Id == id, ct);
    }

    /// <summary>
    /// إضافة كيان جديد إلى مجموعة الكيانات.
    /// </summary>
    /// <param name="entity">الكيان المراد حفظه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        // إضافة الكيان إلى متعقب التغييرات بوضع Added
        await _dbSet.AddAsync(entity, ct);
    }

    /// <summary>
    /// إضافة مجموعة من الكيانات دفعة واحدة.
    /// </summary>
    /// <param name="entities">قائمة الكيانات المراد حفظها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        // إضافة كافة عناصر القائمة دفعة واحدة إلى متعقب التغييرات
        return _dbSet.AddRangeAsync(entities, ct);
    }

    /// <summary>
    /// تحديث بيانات الكيان مع تطبيق فحص الأمان لنسخة التزامن ومفاتيح الملكية.
    /// </summary>
    /// <param name="entity">الكيان المحدث.</param>
    public void Update(T entity)
    {
        // إرسال الكيان لمحرك الاستمرارية للتحقق من سلامة المفاتيح وتحديث الخصائص المسموحة
        EntityPersistence.Update(_context, entity);
    }

    /// <summary>
    /// تطبيق الحذف المنطقي على الكيان.
    /// </summary>
    /// <param name="entity">الكيان المراد حذفه منطقياً.</param>
    public void SoftDelete(T entity)
    {
        // تعيين علامة الحذف المنطقي وتحديث السجل
        EntityPersistence.SoftDelete(_context, entity);
    }
}
