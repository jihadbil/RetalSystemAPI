using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة المستودع العام للبيانات (Generic Repository Interface).
/// تُعرّف العمليات الأساسية لمعالجة الكيانات (CRUD) مع تطبيق عزل المستأجر التلقائي والحذف المنطقي.
/// </summary>
/// <typeparam name="T">نوع الكيان المشتق من BaseEntity.</typeparam>
public interface IRepository<T>
    where T : BaseEntity
{
    /// <summary>
    /// جلب كيان محدد بواسطة معرفه الفريد كقراءة فقط بدون تتبع (AsNoTracking).
    /// يحترم قيود عزل المستأجر الحالي ويتجاهل السجلات المحذوفة منطقياً.
    /// </summary>
    /// <param name="id">المعرف الفريد للكيان المطلوب.</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة.</param>
    /// <returns>نسخة الكيان كقراءة فقط، أو null في حال عدم العثور عليه.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تحميل نسخة متتبعة بالكامل من الكيان (Tracked Entity) للتعديل ضمن سياق وحدة العمل الحالية.
    /// يفحص الذاكرة المحلية (ChangeTracker) أولاً قبل استعلام قاعدة البيانات.
    /// </summary>
    /// <param name="id">المعرف الفريد للكيان المطلوب للتعديل.</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة.</param>
    /// <returns>الكيان المتتبع الجاهز للتعديل، أو null إن لم يكن موجوداً.</returns>
    Task<T?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// التحقق السريع من وجود كيان في قاعدة البيانات بواسطة معرفه الفريد.
    /// </summary>
    /// <param name="id">المعرف الفريد للكيان.</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة.</param>
    /// <returns>True إذا كان الكيان موجوداً وفعالاً، وإلا False.</returns>
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// إضافة كيان جديد إلى سياق قاعدة البيانات وتجهيزه للحفظ.
    /// </summary>
    /// <param name="entity">الكيان المراد إضافته.</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة.</param>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة من الكيانات دفعة واحدة إلى سياق قاعدة البيانات.
    /// </summary>
    /// <param name="entities">قائمة الكيانات المراد إضافتها.</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة.</param>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>
    /// إدراج الكيان في متعقب التغييرات ووضع علامة عليه كمعدل مع حماية المفاتيح ونسخة التزامن.
    /// </summary>
    /// <param name="entity">الكيان المراد تحديث بياناته.</param>
    void Update(T entity);

    /// <summary>
    /// وضع علامة الحذف المنطقي (Soft Delete) على الكيان بتعيين IsDeleted = true دون حذفه فيزيائياً من قاعدة البيانات.
    /// </summary>
    /// <param name="entity">الكيان المراد حذفه منطقياً.</param>
    void SoftDelete(T entity);
}
