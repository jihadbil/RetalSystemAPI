using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Users;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Users.Interfaces;

namespace RetalSystemAPI.Services.Users.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة المستخدمين والأدوار والتحقق من هوية المستأجر وإعادة تعيين كلمات المرور وإدارة الصلاحيات المخصصة.
/// </summary>
public class UserService : IUserService
{
    // مدير المستخدمين الخاص بنظام Identity
    private readonly UserManager<ApplicationUser> _userManager;
    // مدير الأدوار الخاص بنظام Identity
    private readonly RoleManager<IdentityRole> _roleManager;
    // سياق قاعدة البيانات الرئيسي
    private readonly AppDbContext _context;
    // خدمة توفير هوية المستأجر الحالي
    private readonly ICurrentTenantService _currentTenantService;

    /// <summary>
    /// تهيئة خدمة المستخدمين وحقن خدمات الهوية وقاعدة البيانات والمستأجر الحالي.
    /// </summary>
    /// <param name="userManager">مدير مستخدمي Identity</param>
    /// <param name="roleManager">مدير أدوار Identity</param>
    /// <param name="context">سياق قاعدة البيانات الرئيسي</param>
    /// <param name="currentTenantService">خدمة جلب المستأجر الحالي</param>
    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        ICurrentTenantService currentTenantService)
    {
        // تعيين مدير المستخدمين
        _userManager = userManager;
        // تعيين مدير الأدوار
        _roleManager = roleManager;
        // تعيين سياق قاعدة البيانات
        _context = context;
        // تعيين خدمة المستأجر الحالي
        _currentTenantService = currentTenantService;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<UserSummaryDto>>> GetPagedUsersAsync(
        int pageNumber,
        int pageSize,
        string? search,
        Guid? branchId,
        CancellationToken ct = default)
    {
        // جلب معرف المستأجر الحالي لتطبيق العزل المتعدد للمستأجرين
        var tenantId = _currentTenantService.TenantId;

        // بناء استعلام المستخدمين مع تضمين الفرع وتصفية المستأجر دون تتبع للأداء العالي
        var query = _context.Users
            .Include(u => u.Branch)
            .Where(u => u.TenantId == tenantId)
            .AsNoTracking();

        // تطبيق فلتر الفرع إن وُجد
        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            // تقييد الاستعلام بالفرع المحدد
            query = query.Where(u => u.BranchId == branchId.Value);
        }

        // تطبيق فلتر البحث بالاسم أو البريد أو رقم الهاتف
        if (!string.IsNullOrWhiteSpace(search))
        {
            // تحويل نص البحث إلى أحرف صغيرة للمقارنة غير الحساسة لحالة الأحرف
            var searchLower = search.Trim().ToLower();
            // تصفية السجلات المطابقة لأي من الحقول
            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(searchLower)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchLower)));
        }

        // حساب إجمالي عدد المستخدمين المطابقين
        int totalCount = await query.CountAsync(ct);

        // جلب مستخدمي الصفحة الحالية مرتبين حسب اسم المستخدم
        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // تهيئة قائمة تجميع النتائج المعروضة
        var dtoList = new List<UserSummaryDto>();

        // جلب أدوار صفحة المستخدمين في استعلامين موحدين بدل استعلام لكل مستخدم (تفادي N+1)
        var userIds = users.Select(u => u.Id).ToList();
        // تجميع الأدوار وربطها بمعرف المستخدم في قاموس سريع البحث
        var rolesById = (await _context.UserRoles
                .AsNoTracking()
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, RoleName = r.Name })
                .ToListAsync(ct))
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName ?? string.Empty).ToList());

        // تحويل كل مستخدم إلى كائن ملخص المستخدم DTO
        foreach (var u in users)
        {
            // استخراج أدوار المستخدم من القاموس المجمع
            var roles = rolesById.TryGetValue(u.Id, out var userRoles) ? userRoles : new List<string>();
            // فحص حالة نشاط الحساب (غير مقفل وتاريخ القفل في الماضي)
            bool isActive = !u.LockoutEnd.HasValue || u.LockoutEnd.Value <= DateTimeOffset.UtcNow;

            // إضافة كائن الملخص إلى القائمة
            dtoList.Add(new UserSummaryDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber,
                TenantId = u.TenantId,
                BranchId = u.BranchId,
                BranchName = u.Branch?.Name,
                Roles = roles,
                IsActive = isActive
            });
        }

        // بناء كائن النتائج المصفحة
        var pagedResult = PagedResult<UserSummaryDto>.Create(dtoList, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<UserSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UserDetailsDto>> GetUserByIdAsync(string id, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _currentTenantService.TenantId;
        // الاستعلام عن المستخدم بالمعرف وضمان عزل المستأجر وتضمين بيانات الفرع
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);

        // التحقق من وجود المستخدم
        if (user == null)
        {
            // إرجاع خطأ بعدم وجود المستخدم
            return ServiceResult<UserDetailsDto>.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        // جلب الأدوار المسندة للمستخدم
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        // التحقق من حالة نشاط الحساب
        bool isActive = !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;

        // جلب المطالبات المباشرة المسندة للمستخدم
        var userClaims = await _userManager.GetClaimsAsync(user);
        // تصفية المطالبات التي تمثل صلاحيات مباشرة
        var directPermissions = userClaims
            .Where(c => c.Type == Permissions.ClaimType)
            .Select(c => c.Value)
            .ToList();

        // تهيئة مجموعة الصلاحيات الفعالة بالصلاحيات المباشرة
        var effectivePermissions = new HashSet<string>(directPermissions, StringComparer.OrdinalIgnoreCase);
        // فحص ما إذا كان المستخدم يمتلك دور الإدارة السيادي
        bool isAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));
        // إذا كان مسؤولاً رئيسياً يتم منحه كافة صلاحيات النظام تلقائياً
        if (isAdmin)
        {
            // إضافة جميع الصلاحيات المعرفة في النظام
            foreach (var code in AppPermissions.GetAllPermissionCodes())
            {
                // إضافة كود الصلاحية
                effectivePermissions.Add(code);
            }
        }
        else
        {
            // استخراج الصلاحيات الموروثة من كل دور مسند للمستخدم
            foreach (var r in roles)
            {
                // جلب كائن الدور
                var roleObj = await _roleManager.FindByNameAsync(r);
                // التحقق من وجود الدور
                if (roleObj != null)
                {
                    // جلب مطالبات الدور
                    var rClaims = await _roleManager.GetClaimsAsync(roleObj);
                    // إضافة صلاحيات الدور إلى الصلاحيات الفعالة للمستخدم
                    foreach (var c in rClaims.Where(rc => rc.Type == Permissions.ClaimType))
                    {
                        // إضافة الصلاحية
                        effectivePermissions.Add(c.Value);
                    }
                }
            }
        }

        // بناء كائن التفاصيل الكامل للمستخدم
        var details = new UserDetailsDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            BranchName = user.Branch?.Name,
            Roles = roles,
            IsActive = isActive,
            DirectPermissions = directPermissions,
            EffectivePermissions = effectivePermissions.OrderBy(p => p).ToList()
        };

        // إرجاع النتيجة بنجاح
        return ServiceResult<UserDetailsDto>.Success(details);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UserSummaryDto>> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _currentTenantService.TenantId;

        // التحقق من عدم تكرار اسم المستخدم
        var existingUser = await _userManager.FindByNameAsync(dto.UserName);
        // في حال وجود الاسم مسبقاً
        if (existingUser != null)
        {
            // إرجاع خطأ بوجود اسم المستخدم
            return ServiceResult<UserSummaryDto>.Failure("اسم المستخدم موجود بالفعل", ErrorCodes.UserAlreadyExists);
        }

        // التحقق من عدم تكرار البريد الإلكتروني إن تم إدخاله
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            // الاستعلام بالبريد الإلكتروني
            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            // في حال وجود البريد مسبقاً
            if (existingEmail != null)
            {
                // إرجاع خطأ بوجود البريد
                return ServiceResult<UserSummaryDto>.Failure("البريد الإلكتروني مستخدم بالفعل", ErrorCodes.UserAlreadyExists);
            }
        }

        // تعيين معرف المستأجر المستهدف
        var targetTenantId = dto.TenantId != Guid.Empty ? dto.TenantId : tenantId;

        // إنشاء كائن المستخدم الجديد
        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            TenantId = targetTenantId,
            BranchId = dto.BranchId
        };

        // إنشاء المستخدم عبر UserManager مع تشفير كلمة المرور
        var result = await _userManager.CreateAsync(user, dto.Password);
        // التحقق من نجاح عملية الإنشاء
        if (!result.Succeeded)
        {
            // تجميع أخطاء التحقق
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            // إرجاع خطأ الإنشاء
            return ServiceResult<UserSummaryDto>.Failure($"فشل إنشاء المستخدم: {errors}", ErrorCodes.ValidationError);
        }

        // إسناد الدور المحدد للمستخدم إن تم تزويده
        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            // التحقق من وجود الدور أو إنشاؤه تلقائياً
            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                // إنشاء الدور إن لم يكن موجوداً
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));
            }
            // ربط المستخدم بالدور
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        // جلب قائمة أدوار المستخدم المسندة
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        // جلب اسم الفرع المرتبط
        var branch = await _context.Branches.FindAsync(new object[] { user.BranchId }, ct);

        // بناء كائن ملخص المستخدم المنشأ
        var summary = new UserSummaryDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            BranchName = branch?.Name,
            Roles = roles,
            IsActive = true
        };

        // إرجاع النتيجة الناجحة
        return ServiceResult<UserSummaryDto>.Success(summary);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UserSummaryDto>> UpdateUserAsync(string id, UpdateUserDto dto, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _currentTenantService.TenantId;
        // استعلام المستخدم بالمعرف مع عزله بالمستأجر وتضمين الفرع
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);

        // التحقق من وجود المستخدم
        if (user == null)
        {
            // إرجاع خطأ بعدم وجود المستخدم
            return ServiceResult<UserSummaryDto>.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        // تحديث البريد الإلكتروني
        user.Email = dto.Email;
        // تحديث رقم الهاتف
        user.PhoneNumber = dto.PhoneNumber;
        // تحديث الفرع المرتبط
        user.BranchId = dto.BranchId;

        // تعديل حالة قفل الحساب بحسب خاصية IsActive
        if (dto.IsActive)
        {
            // تفعيل الحساب وإلغاء القفل
            user.LockoutEnd = null;
        }
        else
        {
            // قفل الحساب إلى أقصى وقت ممكن لتعطيل الدخول
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }

        // حفظ تعديلات المستخدم في Identity
        var updateResult = await _userManager.UpdateAsync(user);
        // التحقق من نجاح التحديث
        if (!updateResult.Succeeded)
        {
            // تجميع رسائل الخطأ
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            // إرجاع رسالة الفشل
            return ServiceResult<UserSummaryDto>.Failure($"فشل تحديث بيانات المستخدم: {errors}", ErrorCodes.ValidationError);
        }

        // تحديث الدور إن تم تزويد دور جديد
        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            // جلب الأدوار الحالية للمستخدم
            var currentRoles = await _userManager.GetRolesAsync(user);
            // إذا كان الدور المطلوب مختلفاً عن الأدوار الحالية
            if (!currentRoles.Contains(dto.Role))
            {
                // التحقق من وجود الدور أو إنشاؤه
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    // إنشاء الدور الجديد
                    await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                }
                // إزالة الأدوار القديمة
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                // إسناد الدور الجديد
                await _userManager.AddToRoleAsync(user, dto.Role);
            }
        }

        // جلب أدوار المستخدم المحدثة
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        // جلب بيانات الفرع المحدث
        var branch = await _context.Branches.FindAsync(new object[] { user.BranchId }, ct);

        // بناء كائن ملخص المستخدم المحدث
        var summary = new UserSummaryDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            BranchName = branch?.Name,
            Roles = roles,
            IsActive = dto.IsActive
        };

        // إرجاع النتيجة الناجحة
        return ServiceResult<UserSummaryDto>.Success(summary);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteUserAsync(string id, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _currentTenantService.TenantId;
        // استعلام المستخدم المطلوب حذفه
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);

        // التحقق من وجود المستخدم
        if (user == null)
        {
            // إرجاع خطأ بعدم وجود المستخدم
            return ServiceResult.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        // حذف المستخدم عبر UserManager
        var deleteResult = await _userManager.DeleteAsync(user);
        // التحقق من نجاح الحذف
        if (!deleteResult.Succeeded)
        {
            // تجميع رسائل الخطأ
            var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            // إرجاع خطأ الحذف
            return ServiceResult.Failure($"فشل حذف المستخدم: {errors}", ErrorCodes.ValidationError);
        }

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ResetPasswordAsync(string id, ResetPasswordDto dto, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _currentTenantService.TenantId;
        // استعلام المستخدم المعني بإعادة التعيين
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);

        // التحقق من وجود المستخدم
        if (user == null)
        {
            // إرجاع خطأ بعدم وجود المستخدم
            return ServiceResult.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        // توليد رمز إعادة تعيين كلمة المرور آلياً
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // تطبيق كلمة المرور الجديدة بواسطة الرمز المولد
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        // التحقق من نجاح إعادة التعيين
        if (!result.Succeeded)
        {
            // تجميع أسباب الفشل
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            // إرجاع خطأ فشل تعيين كلمة المرور
            return ServiceResult.Failure($"فشل تعيين كلمة المرور: {errors}", ErrorCodes.ValidationError);
        }

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default)
    {
        // قائمة الأدوار الافتراضية للنظام
        var defaultRoles = new[] { "Admin", "Manager", "User", "Cashier" };
        // التحقق من وجود الأدوار الافتراضية وإنشاؤها إن لم تكن مسجلة
        foreach (var roleName in defaultRoles)
        {
            // إذا لم يكن الدور مسجلاً
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                // إنشاء الدور الافتراضي
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // جلب قائمة كافة الأدوار المسجلة في النظام
        var roles = await _roleManager.Roles.ToListAsync(ct);
        // استخراج معرفات الأدوار
        var roleIds = roles.Select(r => r.Id).ToList();

        // جلب مطالبات الصلاحيات لكل الأدوار في استعلام واحد بدل استعلام لكل دور (تفادي N+1)
        var permissionClaimsByRole = (await _context.RoleClaims
                .AsNoTracking()
                .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == Permissions.ClaimType)
                .Select(rc => new { rc.RoleId, rc.ClaimValue })
                .ToListAsync(ct))
            .GroupBy(x => x.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ClaimValue)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .OrderBy(p => p)
                .Select(p => p!)
                .ToList());

        // تهيئة قائمة تجميع الأدوار
        var roleDtos = new List<RoleDto>();

        // تحويل كل دور إلى DTO مع الصلاحيات التابعة له
        foreach (var r in roles)
        {
            // استخراج الصلاحيات المسندة للدور من القاموس المجمع
            var perms = permissionClaimsByRole.TryGetValue(r.Id, out var rolePerms)
                ? rolePerms
                : new List<string>();

            // إعطاء دور المدير والمسؤول الرئيسي كافة صلاحيات النظام
            if (r.Name != null && (r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                // جلب كافة صلاحيات النظام
                perms = AppPermissions.GetAllPermissionCodes();
            }

            // إضافة كائن الدور إلى القائمة
            roleDtos.Add(new RoleDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                Permissions = perms
            });
        }

        // إرجاع النتيجة الناجحة مع قائمة الأدوار
        return ServiceResult<List<RoleDto>>.Success(roleDtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<RoleDto>> CreateRoleAsync(string roleName, CancellationToken ct = default)
    {
        // التحقق من إدخال اسم الدور
        if (string.IsNullOrWhiteSpace(roleName))
        {
            // إرجاع خطأ اشتراط الاسم
            return ServiceResult<RoleDto>.Failure("اسم الدور مطلوب", ErrorCodes.ValidationError);
        }

        // تنظيف اسم الدور من الفراغات
        roleName = roleName.Trim();
        // التحقق من عدم وجود الدور مسبقاً
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            // إرجاع خطأ وجود الدور
            return ServiceResult<RoleDto>.Failure("هذا الدور موجود بالفعل", ErrorCodes.ValidationError);
        }

        // إنشاء كائن الدور الجديد
        var role = new IdentityRole(roleName);
        // حفظ الدور في قاعدة البيانات عبر مدير الأدوار
        var result = await _roleManager.CreateAsync(role);
        // التحقق من نجاح العملية
        if (!result.Succeeded)
        {
            // تجميع رسائل الخطأ
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            // إرجاع خطأ إنشاء الدور
            return ServiceResult<RoleDto>.Failure($"فشل إنشاء الدور: {errors}", ErrorCodes.ValidationError);
        }

        // إرجاع كائن الدور المنشأ بنجاح
        return ServiceResult<RoleDto>.Success(new RoleDto { Id = role.Id, Name = role.Name ?? string.Empty });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteRoleAsync(string roleId, CancellationToken ct = default)
    {
        // الاستعلام عن الدور بالمعرف
        var role = await _roleManager.FindByIdAsync(roleId);
        // التحقق من وجود الدور
        if (role == null)
        {
            // إرجاع خطأ بعدم وجود الدور
            return ServiceResult.Failure("الدور غير موجود", ErrorCodes.ValidationError);
        }

        // حظر حذف أدوار المسؤول الأساسية
        if (role.Name != null && (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
        {
            // إرجاع خطأ بمنع حذف دور المسؤول
            return ServiceResult.Failure("لا يمكن حذف دور المسؤول الرئيسي", ErrorCodes.ValidationError);
        }

        // حذف الدور عبر مدير الأدوار
        var result = await _roleManager.DeleteAsync(role);
        // التحقق من نجاح الحذف
        if (!result.Succeeded)
        {
            // تجميع رسائل الخطأ
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            // إرجاع خطأ الحذف
            return ServiceResult.Failure($"فشل حذف الدور: {errors}", ErrorCodes.ValidationError);
        }

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public Task<ServiceResult<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        // تحويل كافة الصلاحيات المعرفة في النظام إلى قائمة كائنات نقل البيانات
        var list = AppPermissions.All.Select(p => new PermissionDto
        {
            Code = p.Code,
            Name = p.Name,
            Category = p.Category,
            Description = p.Description,
            IsScreenAccess = p.IsScreenAccess
        }).ToList();

        // إرجاع النتيجة كمهمة مكتملة فوراً
        return Task.FromResult(ServiceResult<List<PermissionDto>>.Success(list));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<RolePermissionsDto>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default)
    {
        // الاستعلام عن الدور بالمعرف
        var role = await _roleManager.FindByIdAsync(roleId);
        // التحقق من وجود الدور
        if (role == null)
        {
            // إرجاع خطأ بعدم وجود الدور
            return ServiceResult<RolePermissionsDto>.Failure("الدور غير موجود", ErrorCodes.ValidationError);
        }

        // قائمة لتجميع صلاحيات الدور
        List<string> perms;
        // إذا كان الدور هو دور الإدارة الكاملة
        if (role.Name != null && (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
        {
            // منحه جميع أكواد الصلاحيات
            perms = AppPermissions.GetAllPermissionCodes();
        }
        else
        {
            // استعلام المطالبات المسجلة لهذا الدور
            var claims = await _roleManager.GetClaimsAsync(role);
            // تصفية مطالبات الصلاحيات وترتيبها
            perms = claims
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .OrderBy(p => p)
                .ToList();
        }

        // إرجاع النتيجة مع كائن صلاحيات الدور
        return ServiceResult<RolePermissionsDto>.Success(new RolePermissionsDto
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            Permissions = perms
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateRolePermissionsAsync(UpdateRolePermissionsDto dto, CancellationToken ct = default)
    {
        // الاستعلام عن الدور المطلوب تعديل صلاحياته
        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        // التحقق من وجود الدور
        if (role == null)
        {
            // إرجاع خطأ بعدم وجود الدور
            return ServiceResult.Failure("الدور غير موجود", ErrorCodes.ValidationError);
        }

        // حظر تقييد صلاحيات دور المسؤول الرئيسي
        if (role.Name != null && (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
        {
            // إرجاع خطأ الحظر
            return ServiceResult.Failure("دور المسؤول الرئيسي يمتلك كافة الصلاحيات دائماً ولا يمكن تقييده", ErrorCodes.ValidationError);
        }

        // جلب المطالبات السابقة المسجلة للدور
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        // حذف المطالبات السابقة التي تمثل صلاحيات
        foreach (var c in existingClaims.Where(c => c.Type == Permissions.ClaimType))
        {
            // إزالة المطالبة القديمة
            await _roleManager.RemoveClaimAsync(role, c);
        }

        // جلب مجموعة الأكواد الصالحة والمعتمدة في النظام
        var validPermissionCodes = AppPermissions.GetAllPermissionCodes().ToHashSet(StringComparer.OrdinalIgnoreCase);
        // إضافة الصلاحيات الجديدة المعتمدة للدور
        foreach (var p in dto.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            // التحقق من أن الكود معتمد في النظام
            if (validPermissionCodes.Contains(p))
            {
                // إضافة مطالبة الصلاحية الجديدة للدور
                await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, p));
            }
        }

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UserPermissionsDto>> GetUserPermissionsAsync(string userId, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _currentTenantService.TenantId;
        // استعلام المستخدم بالمعرف وتطبيق عزل المستأجر
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, ct);
        // التحقق من وجود المستخدم
        if (user == null)
        {
            // إرجاع خطأ بعدم وجود المستخدم
            return ServiceResult<UserPermissionsDto>.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        // جلب الأدوار المسندة للمستخدم
        var roles = await _userManager.GetRolesAsync(user);
        // جلب المطالبات المباشرة للمستخدم
        var directClaims = await _userManager.GetClaimsAsync(user);
        // فحص وجود شارة تخصيص الصلاحيات المباشرة
        bool isCustomized = directClaims.Any(c => c.Type == "Permissions.Customized" && c.Value == "true");

        // استخراج الصلاحيات المباشرة المسندة للمستخدم
        var directPerms = directClaims
            .Where(c => c.Type == Permissions.ClaimType)
            .Select(c => c.Value)
            .OrderBy(p => p)
            .ToList();

        // تجميع الصلاحيات المستمدة من أدوار المستخدم
        var rolePerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // المرور على أدوار المستخدم
        foreach (var roleName in roles)
        {
            // استعلام كائن الدور
            var role = await _roleManager.FindByNameAsync(roleName);
            // في حال وجود الدور
            if (role != null)
            {
                // جلب مطالبات الدور
                var rClaims = await _roleManager.GetClaimsAsync(role);
                // تجميع صلاحيات الدور
                foreach (var c in rClaims.Where(rc => rc.Type == Permissions.ClaimType))
                {
                    // التحقق من وجود قيمة الصلاحية وإضافتها
                    if (!string.IsNullOrWhiteSpace(c.Value))
                    {
                        // إضافة الصلاحية للمجموعة
                        rolePerms.Add(c.Value);
                    }
                }
            }
        }

        // تهيئة مجموعة الصلاحيات الفعالة
        var effectivePerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // فحص ما إذا كان المستخدم مسؤولاً رئيسياً
        bool isAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));

        // إذا كان مسؤولاً رئيسياً تمنح له كافة الصلاحيات
        if (isAdmin)
        {
            // إضافة كل أكواد الصلاحيات
            foreach (var code in AppPermissions.GetAllPermissionCodes())
            {
                // إضافة الصلاحية
                effectivePerms.Add(code);
            }
        }
        else if (isCustomized)
        {
            // المستخدم لديه صلاحيات مخصصة مستقلة عن الأدوار
            foreach (var p in directPerms)
            {
                // إضافة الصلاحية المباشرة
                effectivePerms.Add(p);
            }
        }
        else
        {
            // المستخدم يرث صلاحيات أدواره المسندة بالإضافة لأي صلاحية مباشرة
            foreach (var p in rolePerms)
            {
                // إضافة صلاحية الدور
                effectivePerms.Add(p);
            }
            foreach (var p in directPerms)
            {
                // إضافة الصلاحية المباشرة
                effectivePerms.Add(p);
            }
        }

        // بناء كائن مصفوفة صلاحيات المستخدم وإرجاع النتيجة
        return ServiceResult<UserPermissionsDto>.Success(new UserPermissionsDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Roles = roles.ToList(),
            IsCustomized = isCustomized,
            RolePermissions = rolePerms.OrderBy(p => p).ToList(),
            DirectPermissions = directPerms,
            EffectivePermissions = effectivePerms.OrderBy(p => p).ToList()
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateUserPermissionsAsync(UpdateUserPermissionsDto dto, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _currentTenantService.TenantId;
        // استعلام المستخدم المعني بتحديث صلاحياته
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId && u.TenantId == tenantId, ct);
        // التحقق من وجود المستخدم
        if (user == null)
        {
            // إرجاع خطأ بعدم وجود المستخدم
            return ServiceResult.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        // جلب المطالبات الحالية للمستخدم
        var existingClaims = await _userManager.GetClaimsAsync(user);
        // حذف المطالبات السابقة المتعلقة بالصلاحيات أو شارة التخصيص
        foreach (var c in existingClaims.Where(c => c.Type == Permissions.ClaimType || c.Type == "Permissions.Customized"))
        {
            // إزالة المطالبة
            await _userManager.RemoveClaimAsync(user, c);
        }

        // في حال تفعيل التخصيص المستقل لصلاحيات المستخدم
        if (dto.IsCustomized)
        {
            // تسجيل شارة أن المستخدم يمتلك صلاحيات مخصصة ومستقلة عن الأدوار
            await _userManager.AddClaimAsync(user, new Claim("Permissions.Customized", "true"));

            // جلب الأكواد الصالحة في النظام
            var validCodes = AppPermissions.GetAllPermissionCodes().ToHashSet(StringComparer.OrdinalIgnoreCase);
            // إضافة الصلاحيات المخصصة الجديدة للمستخدم
            foreach (var p in dto.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                // التأكد من صحة كود الصلاحية
                if (validCodes.Contains(p))
                {
                    // إضافة مطالبة الصلاحية للمستخدم
                    await _userManager.AddClaimAsync(user, new Claim(Permissions.ClaimType, p));
                }
            }
        }

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
