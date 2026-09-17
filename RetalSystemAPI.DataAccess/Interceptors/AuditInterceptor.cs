using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Interceptors;

/// <summary>
/// معترض حفظ التغييرات (SaveChangesInterceptor) للتعبئة التلقائية لبيانات التدقيق والتتبع للمستأجرين:
/// - تعيين المعرف التلقائي (Id = Guid.NewGuid())
/// - تعيين معرف المستأجر (TenantId)
/// - توثيق تاريخ ومستخدم الإنشاء (CreatedAt, CreatedByUserId)
/// - توثيق تاريخ ومستخدم التعديل (UpdatedAt, UpdatedByUserId) مع حماية حقول الإنشاء من التغيير.
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenantService;

    /// <summary>
    /// تهيئة المعترض مع حقن خدمات المستخدم والمستأجر الحاليين.
    /// </summary>
    /// <param name="currentUser">خدمة استخراج المستخدم الحالي</param>
    /// <param name="tenantService">خدمة استخراج المستأجر الحالي</param>
    public AuditInterceptor(
        ICurrentUserService currentUser,
        ICurrentTenantService tenantService)
    {
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    /// <summary>
    /// يتم استدعاؤها تلقائياً قبل تنفيذ SaveChangesAsync لفحص الكيانات المضافة والمعدلة وحقن بيانات التدقيق.
    /// </summary>
    /// <param name="eventData">بيانات الحدث وسياق قاعدة البيانات المرتبط بالعملية</param>
    /// <param name="result">نتيجة الاعتراض الأولية</param>
    /// <param name="cancellationToken">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>نتيجة المعالجة بعد إتمام التدقيق وحقن البيانات</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // التحقق من وجود سياق قاعدة البيانات؛ وفي حال عدم وجوده يتم تمرير التنفيذ للمحرك الأساسي فوراً
        if (eventData.Context is null)
        {
            // تمرير التنفيذ دون إجراء أي تعديلات
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // استخراج معرف المستخدم الحالي من خدمة الجلسة
        var userId = _currentUser.UserId;
        // استخراج معرف المستأجر الحالي من خدمة التوكن
        var currentTenantId = _tenantService.TenantId;

        // فحص وتكرار جميع الكيانات المتأثرة بعملية الحفظ والتي ترث من BaseEntity
        foreach (var entry in eventData.Context.ChangeTracker.Entries<BaseEntity>())
        {
            // معالجة حالة إضافة كيان جديد (Insert)
            if (entry.State == EntityState.Added)
            {
                // إذا لم يتم توليد معرف فريد Id للكيان مسبقاً، يتم توليد Guid جديد تلقائياً
                if (entry.Entity.Id == Guid.Empty)
                {
                    // تعيين المعرف الجديد للكيان
                    entry.Entity.Id = Guid.NewGuid();
                }

                // إذا كان الكيان كياناً متعدد المستأجرين وبلا معرف مستأجر محدد، يتم ربطه بالمستأجر الحالي
                if (entry.Entity is TenantBaseEntity tenantEntity && tenantEntity.TenantId == Guid.Empty && currentTenantId != Guid.Empty)
                {
                    // تعيين معرف المستأجر
                    tenantEntity.TenantId = currentTenantId;
                }

                // تعيين تاريخ الإنشاء للتوقيت الحالي بتوقيت جرينتش إذا لم يكن محدداً
                if (entry.Entity.CreatedAt == default)
                {
                    // حفظ تاريخ الإنشاء الحالي
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                // تسجيل معرف المستخدم الذي أنشأ هذا السجل
                entry.Entity.CreatedByUserId = userId;
            }
            // معالجة حالة تعديل كيان موجود مسبقاً (Update)
            else if (entry.State == EntityState.Modified)
            {
                // تحديث تاريخ آخر تعديل للوقت الحالي
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                // تسجيل معرف المستخدم الذي أجرى التعديل
                entry.Entity.UpdatedByUserId = userId;

                // حماية تاريخ الإنشاء الأصلي من أي تعديل عبر وسم الخاصية بأنها لم تتغير
                entry.Property(x => x.CreatedAt).IsModified = false;
                // حماية مستخدم الإنشاء الأصلي من أي تعديل
                entry.Property(x => x.CreatedByUserId).IsModified = false;
                // في حال كان الكيان يتبع لمستأجر، يتم منع تغيير المستأجر نهائياً لمنع تسريب البيانات
                if (entry.Entity is TenantBaseEntity)
                {
                    // تجميد معرف المستأجر ومنع تحديثه
                    entry.Property(nameof(TenantBaseEntity.TenantId)).IsModified = false;
                }
            }
        }

        // إرجاع النتيجة ومتابعة عملية الحفظ في قاعدة البيانات
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
