using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using BranchEntity = RetalSystemAPI.Models.Branchs.Branch;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Tenant.Interfaces;
using TenantEntity = RetalSystemAPI.Models.Tenant;

namespace RetalSystemAPI.Services.Tenant.Implementations;

/// <summary>
/// تنفيذ خدمة تهيئة المستأجر وتأسيس قاعدة بياناته وعزله المنطقي وفرعه الأساسي وحسابه الإداري.
/// </summary>
/// <param name="scopes">مصنع نطاقات التنفيذ لإنشاء سياق عمل معزول خاص بالمستأجر الجديد</param>
/// <param name="http">الوصول لسياق طلب HTTP للتحقق من هوية وصلاحيات المستخدم الطالب</param>
/// <param name="mapper">خدمة تحويل النماذج بين الكيانات وDTOs</param>
public sealed class TenantProvisioningService(
    ITenantExecutionScopeFactory scopes,
    IHttpContextAccessor http,
    IMapper mapper) : ITenantProvisioningService
{
    /// <inheritdoc />
    /// <summary>
    /// تنفيذ خطوات تهيئة المستأجر الجديد بالكامل ضمن معاملة قاعدة بيانات ذرية واحدة.
    /// </summary>
    public async Task<ServiceResult<ProvisionTenantResponseDto>> ProvisionAsync(
        ProvisionTenantDto dto,
        CancellationToken ct = default)
    {
        // التحقق من هوية المستخدم المتصل والتأكد من انتمائه لدور المسؤول العام للمنصة SuperAdmin
        if (http.HttpContext?.User is not { Identity.IsAuthenticated: true } caller || !caller.IsInRole("SuperAdmin"))
        {
            // إرجاع خطأ عدم امتلاك الصلاحية الكافية لتهيئة المستأجرين
            return ServiceResult<ProvisionTenantResponseDto>.Failure(
                "تهيئة المستأجر تتطلب مسؤول المنصة",
                ErrorCodes.Forbidden);
        }

        // التحقق من صحة بيانات النموذج المدخل
        if (DtoValidation.GetError(dto) is { } validationError)
        {
            // إرجاع خطأ التحقق من صحة البيانات
            return ServiceResult<ProvisionTenantResponseDto>.Failure(
                validationError,
                ErrorCodes.ValidationError);
        }

        // توليد معرف فريد جديد للمستأجر الجديد
        var tenantId = Guid.NewGuid();

        // إنشاء نطاق تنفيذ معزول مخصص لبيئة المستأجر الجديد
        await using var scope = scopes.CreateForProvisioning(tenantId);

        // جلب سياق قاعدة البيانات المخصص من نطاق التنفيذ
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // جلب وحدة العمل المرتبطة بالنطاق
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // جلب خدمة إدارة المستخدمين التابعة للهوية Identity
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // جلب خدمة إدارة الأدوار للتحقق من الأدوار المطلوبة
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // بدء معاملة قاعدة بيانات ذرية تضمن تراجع كافة العمليات في حال حدوث أي فشل
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            // فحص مسبق لعدم تكرار اسم المستأجر في النظام
            if (await uow.Tenants.NameExistsAsync(dto.Tenant.Name, ct))
            {
                // إرجاع خطأ بوجود اسم المستأجر مسبقاً
                return ServiceResult<ProvisionTenantResponseDto>.Failure(
                    "اسم المستأجر مستخدم بالفعل",
                    ErrorCodes.TenantNameExists);
            }

            // التحقق من أن دور مدير المستأجر Admin موجود في النظام
            if (!await roles.RoleExistsAsync("Admin"))
            {
                // إرجاع خطأ بعدم تهيئة دور المدير في النظام
                return ServiceResult<ProvisionTenantResponseDto>.Failure(
                    "دور مدير المستأجر غير مهيأ في النظام",
                    ErrorCodes.ValidationError);
            }

            // تحويل نموذج DTO إلى كيان المستأجر
            var tenant = mapper.Map<TenantEntity>(dto.Tenant);

            // تعيين المعرف المولد للمستأجر
            tenant.Id = tenantId;

            // إنشاء الفرع الرئيسي الأول التابع للمستأجر الجديد
            var branch = new BranchEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = dto.BranchName,
                Address = dto.BranchAddress
            };

            // إضافة كيان المستأجر والفرع الجديدين إلى سياق قاعدة البيانات
            db.AddRange(tenant, branch);

            // حفظ بيانات المستأجر والفرع في قاعدة البيانات
            await db.SaveChangesAsync(ct);

            // تهيئة بيانات حساب المستخدم المسؤول الأول للمستأجر
            var admin = new ApplicationUser
            {
                UserName = dto.AdminUserName,
                Email = dto.AdminEmail,
                TenantId = tenantId,
                BranchId = branch.Id,
                LockoutEnabled = true
            };

            // إنشاء حساب المسؤول في نظام الهوية وتعيين كلمة المرور
            var created = await users.CreateAsync(admin, dto.AdminPassword);

            // التحقق من نجاح عملية إنشاء المستخدم المسؤول
            if (!created.Succeeded)
            {
                // في حال الفشل إرجاع تفاصيل أخطاء الهوية
                return IdentityFailure(created);
            }

            // إسناد دور مدير المستأجر Admin لحساب المسؤول الجديد
            var assigned = await users.AddToRoleAsync(admin, "Admin");

            // التحقق من نجاح إسناد الدور
            if (!assigned.Succeeded)
            {
                // في حال الفشل إرجاع تفاصيل خطأ إسناد الدور
                return IdentityFailure(assigned);
            }

            // تجهيز كائن الاستجابة بالمعرفات المنشأة
            var response = new ProvisionTenantResponseDto
            {
                TenantId = tenantId,
                BranchId = branch.Id,
                AdminUserId = admin.Id
            };

            // اعتماد وتأكيد المعاملة في قاعدة البيانات
            await transaction.CommitAsync(ct);

            // إرجاع نتيجة النجاح مع كائن الاستجابة
            return ServiceResult<ProvisionTenantResponseDto>.Success(response);
        }
        catch (DbUpdateException ex) when (SqlConstraintErrors.IsUniqueViolation(ex, "IX_Tenants_Name"))
        {
            // في حال وجود تعارض فريد في اسم المستأجر أثناء الحفظ
            return ServiceResult<ProvisionTenantResponseDto>.Failure(
                "اسم المستأجر مستخدم بالفعل",
                ErrorCodes.TenantNameExists);
        }
        // التخلص من المعاملة غير المعتمدة يتولى التراجع التلقائي (Rollback) عن المستأجر والفرع والمستخدم والأدوار معاً
    }

    /// <summary>
    /// معالجة أخطاء نظام الهوية Identity وتحويلها إلى رسالة ورمز خطأ مناسب في ServiceResult.
    /// </summary>
    /// <param name="result">نتيجة عملية الهوية المحتوية على تفاصيل الأخطاء</param>
    /// <returns>نتيجة خدمة فاشلة تحتوي على رسائل الخطأ وكود الخطأ المرجعي</returns>
    private static ServiceResult<ProvisionTenantResponseDto> IdentityFailure(IdentityResult result) =>
        ServiceResult<ProvisionTenantResponseDto>.Failure(
            // تجميع رسائل أخطاء الهوية مفصولة بفاصلة
            string.Join("، ", result.Errors.Select(e => e.Description)),
            // تحديد كود الخطأ المناسب بناءً على وجود تكرار لاسم المستخدم أو البريد الإلكتروني
            LoginIdentifierValidator.IsDuplicate(result)
                ? ErrorCodes.UserAlreadyExists
                : ErrorCodes.ValidationError);
}
