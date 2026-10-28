using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// محرك إدارة استمرارية الكيانات والتعامل مع متعقب التغييرات (Entity Persistence Engine).
/// يضمن التطبيق الصارم لقيود الأمان، عزل المستأجرين، حماية المفاتيح، والتحكم في تزامن البيانات والتحديث الانتقائي.
/// </summary>
internal static class EntityPersistence
{
    /// <summary>
    /// جلب كيان للقراءة فقط دون تتبع مع التحقق من الذاكرة المحلية أولاً لمنع قراءة بيانات ملغاة أو تنتمي لمستأجر آخر.
    /// </summary>
    /// <typeparam name="T">نوع الكيان.</typeparam>
    /// <param name="context">سياق قاعدة البيانات.</param>
    /// <param name="id">المعرف الفريد للكيان.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>الكيان كقراءة فقط أو null.</returns>
    public static async Task<T?> GetReadOnlyByIdAsync<T>(AppDbContext context, Guid id, CancellationToken ct)
        where T : BaseEntity
    {
        // التحقق من عدم طلب إلغاء العملية قبل الشروع في البحث
        ct.ThrowIfCancellationRequested();

        // البحث في الذاكرة المحلية لمتعقب التغييرات لمعرفة ما إذا كان السجل قيد المعالجة
        var local = context.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == id);

        // التحقق من عدم صلاحية الكيان المحلي إذا كان محدداً للحذف أو ينتمي لمستأجر آخر
        if (local is not null && (local.State == EntityState.Deleted || local.Entity.IsDeleted || (local.Entity is TenantBaseEntity owned && local.Entity is not Tenant && owned.TenantId != context.CurrentTenantId)))
            return null;

        // تنفيذ استعلام قاعدة البيانات دون تتبع لضمان سرعة الأداء وتوفير الموارد
        return await context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    /// <summary>
    /// جلب كيان متتبع للتعديل مع تفضيل الكائن المحمل مسبقاً في الذاكرة المحلية لضمان الاتساق الذري.
    /// </summary>
    /// <typeparam name="T">نوع الكيان.</typeparam>
    /// <param name="context">سياق قاعدة البيانات.</param>
    /// <param name="id">المعرف الفريد للكيان.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>الكيان المتتبع أو null.</returns>
    public static async Task<T?> GetByIdAsync<T>(AppDbContext context, Guid id, CancellationToken ct)
        where T : BaseEntity
    {
        // التحقق من عدم طلب إلغاء العملية
        ct.ThrowIfCancellationRequested();

        // فحص الذاكرة المحلية لمتعقب التغييرات لاسترجاع الكيان فوراً إن كان محملاً مسبقاً
        var local = context.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == id);
        if (local is not null)
        {
            // استخراج كائن الكيان المحلي
            var entity = local.Entity;

            // التحقق من أن الكيان المحلي نشط، غير محذوف، وينتمي للمستأجر الحالي
            return local.State != EntityState.Deleted && !entity.IsDeleted && (entity is not TenantBaseEntity owned || entity is Tenant || owned.TenantId == context.CurrentTenantId) ? entity : null;
        }

        // استرجاع الكيان من قاعدة البيانات وتتبعه في السياق في حال عدم وجوده بالذاكرة المحلية
        return await context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    /// <summary>
    /// تحديث الكيان في سياق قاعدة البيانات مع التحقق الصارم من ثبات المفاتيح وتوافق نسخة التزامن (RowVersion).
    /// </summary>
    /// <typeparam name="T">نوع الكيان.</typeparam>
    /// <param name="context">سياق قاعدة البيانات.</param>
    /// <param name="entity">الكيان المراد تحديث بياناته.</param>
    public static void Update<T>(AppDbContext context, T entity)
        where T : BaseEntity
    {
        // التأكد من أن السياق جاهز والمعاملة لم يتم التراجع عنها سابقاً
        context.EnsureTransactionUsable();

        // استخراج مدخل الكيان من متعقب التغييرات أو ربطه بالسياق
        var entry = context.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == entity.Id) ?? context.Entry(entity);

        // رفض تعديل أي سجل تم وضع علامة الحذف النهائي عليه مسبقاً
        if (entry.State == EntityState.Deleted)
            throw new InvalidOperationException("لا يمكن تعديل سجل محدد للحذف النهائي.");

        // ربط الكيان المنفصل بالسياق إذا لم يكن مربوطاً
        if (entry.State == EntityState.Detached)
            context.Set<T>().Attach(entity);

        // في حال كان الكيان الممرر كائناً مختلفاً في الذاكرة عن الكائن المتتبع
        if (!ReferenceEquals(entry.Entity, entity))
        {
            // استخراج القيم الواردة من الكائن الجديد
            var incoming = context.Entry(entity).CurrentValues;

            // التحقق الصارم من عدم محاولة تغيير المفاتيح الأساسية أو هوية المستأجر
            if (entry.Properties.Any(p => p.Metadata.IsKey() && !Equals(p.CurrentValue, incoming[p.Metadata.Name])))
                throw new UnauthorizedAccessException("لا يمكن تغيير ملكية السجل أو مفاتيحه.");

            // التحقق من توفر نسخة التزامن ومطابقتها للسجل غير المضاف حديثاً
            if (entry.State != EntityState.Added)
                entry.Property(e => e.RowVersion).OriginalValue = entity.RowVersion?.ToArray() ?? throw new DbUpdateConcurrencyException("نسخة التزامن مطلوبة لتعديل السجل.");

            // نسخ القيم إلى الخصائص القابلة للكتابة فقط
            foreach (var property in entry.Properties.Where(p => IsWritable(p.Metadata)))
                property.CurrentValue = incoming[property.Metadata.Name];
        }

        // وضع علامة التعديل على الخصائص القابلة للكتابة مع الحفاظ على حالة Added للسجلات الجديدة
        if (entry.State != EntityState.Added)
            foreach (var property in entry.Properties.Where(p => IsWritable(p.Metadata)))
                property.IsModified = true;
    }

    /// <summary>
    /// وضع علامة الحذف المنطقي على الكيان بتعيين IsDeleted إلى true وتحديثه في السياق.
    /// </summary>
    /// <typeparam name="T">نوع الكيان.</typeparam>
    /// <param name="context">سياق قاعدة البيانات.</param>
    /// <param name="entity">الكيان المطلوب حذفه منطقياً.</param>
    public static void SoftDelete<T>(AppDbContext context, T entity)
        where T : BaseEntity
    {
        // تفعيل خاصية الحذف المنطقي
        entity.IsDeleted = true;
        // إجراء تحديث الكيان في السياق ليتم حفظ التعديل
        Update(context, entity);
    }

    /// <summary>
    /// الحذف الفيزيائي النهائي للكيان من قاعدة البيانات مع تنظيف التتبع.
    /// </summary>
    /// <typeparam name="T">نوع الكيان.</typeparam>
    /// <param name="context">سياق قاعدة البيانات.</param>
    /// <param name="entity">الكيان المطلوب حذفه نهائياً.</param>
    public static void HardDelete<T>(AppDbContext context, T entity)
        where T : BaseEntity
    {
        // تحديث الكيان أولاً للتأكد من ربطه بالسياق
        Update(context, entity);
        // استخراج الكائن المتتبع من متعقب التغييرات
        var tracked = context.ChangeTracker.Entries<T>().Single(e => e.Entity.Id == entity.Id).Entity;
        // إزالة الكيان نهائياً من مجموعة البيانات
        context.Set<T>().Remove(tracked);
    }

    /// <summary>
    /// فحص هل الخاصية قابلة للتعديل والكتابة البرمجية مع استبعاد المفاتيح وحقول التتبع والتدقيق الأساسية.
    /// </summary>
    /// <param name="property">بيانات تعريف الخاصية في نموذج EF Core.</param>
    /// <returns>True إذا كانت الخاصية قابلة للتعديل، وإلا False.</returns>
    private static bool IsWritable(IProperty property) =>
        !property.IsKey() &&
        property.GetAfterSaveBehavior() == PropertySaveBehavior.Save &&
        property.Name is not nameof(BaseEntity.RowVersion)
                     and not nameof(BaseEntity.CreatedAt)
                     and not nameof(BaseEntity.CreatedByUserId)
                     and not nameof(TenantBaseEntity.TenantId);
}
