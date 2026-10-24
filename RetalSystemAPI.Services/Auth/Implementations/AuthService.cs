using RetalSystemAPI.Models.DTOs.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Auth;
using RetalSystemAPI.Services.Auth.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Auth.Implementations;
/// <summary>
/// تنفيذ خدمة المصادقة وتوليد رموز التوثيق JWT والتحقق من حسابات المستخدمين والمستأجرين وتدقيق الصلاحيات.
/// </summary>
public class AuthService : IAuthService
{
    // مدير هوية المستخدمين لإنشاء الحسابات والتحقق من كلمات المرور
    private readonly UserManager<ApplicationUser> _userManager;
    // مدير أدوار النظام للبحث عن الأدوار واستخراج مطالباتها
    private readonly RoleManager<IdentityRole> _roleManager;
    // موفر إعدادات التكوين لقراءة إعدادات JWT ومفاتيح التشفير
    private readonly IConfiguration _configuration;
    // وحدة العمل للوصول إلى مستودعات البيانات والتحقق من وجود المستأجر
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;
    /// <summary>
    /// تهيئة خدمة المصادقة مع حقن مدير المستخدمين ومدير الأدوار وإعدادات التكوين ووحدة العمل.
    /// </summary>
    /// <param name = "userManager">مدير مستخدمي الهوية</param>
    /// <param name = "roleManager">مدير أدوار الهوية</param>
    /// <param name = "configuration">إعدادات التطبيق لقراءة تكوينات التوكن</param>
    /// <param name = "unitOfWork">وحدة العمل للوصول للمستودعات</param>
    public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IUnitOfWork unitOfWork, AppDbContext dbContext, ICurrentTenantService currentTenantService)
    {
        // تعيين مرجع مدير المستخدمين
        _userManager = userManager;
        // تعيين مرجع مدير الأدوار
        _roleManager = roleManager;
        // تعيين مرجع التكوينات
        _configuration = configuration;
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<AuthResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        var currentTenantId = _currentTenantService.TenantId;
        if (currentTenantId == Guid.Empty)
        {
            return ServiceResult<AuthResponseDto>.Failure("المستخدم غير مصرح له", ErrorCodes.Unauthorized);
        }

        // Tenant membership is determined by the authenticated caller, never by
        // a client supplied identifier.
        if (dto.TenantId != Guid.Empty && dto.TenantId != currentTenantId)
        {
            return ServiceResult<AuthResponseDto>.Failure("لا يمكن إنشاء مستخدم لدى مستأجر آخر", ErrorCodes.Forbidden);
        }

        // 1. التحقق من وجود المستأجر في قاعدة البيانات
        var tenant = await _unitOfWork.Tenants.GetByIdForUpdateAsync(currentTenantId, ct);
        // إذا لم يتم العثور على المستأجر، يتم إرجاع نتيجة فشل
        if (tenant is null)
        {
            // إرجاع خطأ عدم وجود المستأجر
            return ServiceResult<AuthResponseDto>.Failure("المستأجر المحدد غير موجود", ErrorCodes.TenantNotFound);
        }

        if (!tenant.IsActive)
        {
            return ServiceResult<AuthResponseDto>.Failure("حساب المستأجر غير نشط", ErrorCodes.Unauthorized);
        }

        var branchExists = await _unitOfWork.Branches.IsActiveForTenantAsync(dto.BranchId, currentTenantId, ct);
        if (!branchExists)
        {
            return ServiceResult<AuthResponseDto>.Failure("الفرع المحدد غير موجود أو غير نشط", ErrorCodes.BranchNotFound);
        }

        // 2. التحقق من عدم تكرار اسم المستخدم
        var existingUser = await _userManager.FindByNameAsync(dto.UserName);
        // التحقق مما إذا كان هناك مستخدم بنفس الاسم
        if (existingUser is not null)
        {
            // إرجاع خطأ وجود المستخدم مسبقاً
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم موجود بالفعل", ErrorCodes.UserAlreadyExists);
        }

        // التحقق من عدم تكرار البريد الإلكتروني
        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        // إذا كان البريد الإلكتروني مسجلاً مسبقاً
        if (existingEmail is not null)
        {
            // إرجاع خطأ استخدام البريد مسبقاً
            return ServiceResult<AuthResponseDto>.Failure("البريد الإلكتروني مستخدم بالفعل", ErrorCodes.UserAlreadyExists);
        }

        // 3. إنشاء كائن المستخدم وربطه بالمستأجر والفرع المحددين
        var user = new ApplicationUser
        {
            // تعيين اسم المستخدم
            UserName = dto.UserName,
            // تعيين البريد الإلكتروني
            Email = dto.Email,
            // تعيين معرف المستأجر
            TenantId = currentTenantId,
            // تعيين معرف الفرع
            BranchId = dto.BranchId,
            LockoutEnabled = true
        };
        // حفظ وإنشاء المستخدم مع تشفير كلمة المرور عبر Identity
        var createResult = await _userManager.CreateAsync(user, dto.Password);
        // التحقق من نجاح عملية الإنشاء
        if (!createResult.Succeeded)
        {
            // تجميع رسائل أخطاء التحقق وقواعد كلمة المرور
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            // إرجاع نتيجة فشل التحقق مع التفاصيل
            return ServiceResult<AuthResponseDto>.Failure($"فشل إنشاء الحساب: {errors}",
                LoginIdentifierValidator.IsDuplicate(createResult) ? ErrorCodes.UserAlreadyExists : ErrorCodes.ValidationError);
        }

        // 4. توليد رمز JWT وتجهيز بيانات الاستجابة للمستخدم الجديد
        var authResponse = await GenerateJwtTokenAsync(user);
        // إرجاع النتيجة الناجحة متضمنة بيانات الجلسة والتوكن
        return ServiceResult<AuthResponseDto>.Success(authResponse);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<AuthResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // البحث عن المستخدم بواسطة اسم المستخدم أو البريد الإلكتروني
        var user = await _userManager.FindByNameAsync(dto.UserName) ?? await _userManager.FindByEmailAsync(dto.UserName);
        // التحقق من وجود المستخدم
        if (user is null)
        {
            // إرجاع خطأ في بيانات الاعتماد عند عدم العثور على الحساب
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", ErrorCodes.InvalidCredentials);
        }

        // Disabled and locked accounts must never receive a new token. This
        // check is independent of LockoutEnabled so administratively disabled
        // legacy accounts remain blocked as well.
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", ErrorCodes.InvalidCredentials);
        }

        var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == user.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
        {
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", ErrorCodes.InvalidCredentials);
        }

        var branchIsActive = await _dbContext.Branches.IgnoreQueryFilters().AsNoTracking().AnyAsync(b => b.Id == user.BranchId && b.TenantId == user.TenantId && b.IsActive && !b.IsDeleted, ct);
        if (!branchIsActive)
        {
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", ErrorCodes.InvalidCredentials);
        }

        // التحقق من صحة كلمة المرور المدخلة ومطابقتها للتجزئة المخزنة
        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        // إذا كانت كلمة المرور غير مطابقة
        if (!isPasswordValid)
        {
            if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
                await _userManager.UpdateAsync(user);
            }

            await _userManager.AccessFailedAsync(user);
            // إرجاع خطأ في بيانات الاعتماد
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", ErrorCodes.InvalidCredentials);
        }

        if (user.AccessFailedCount > 0)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        // توليد رمز JWT وتجميع صلاحيات وأدوار المستخدم
        var authResponse = await GenerateJwtTokenAsync(user);
        // إرجاع نتيجة نجاح المصادقة مع التوكن
        return ServiceResult<AuthResponseDto>.Success(authResponse);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> LogoutAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult.Failure("المستخدم غير موجود", ErrorCodes.UserNotFound);
        }

        // تدوير SecurityStamp يبطل فوراً جميع رموز JWT الصادرة سابقاً لهذا المستخدم.
        var result = await _userManager.UpdateSecurityStampAsync(user);
        if (!result.Succeeded)
        {
            return ServiceResult.Failure("تعذر إنهاء جلسة المستخدم", ErrorCodes.Unauthorized);
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// توليد توكن مصادقة JWT مشفر وموقع يحمل بيانات ومعرفات المستخدم والمستأجر والفرع والأدوار والصلاحيات.
    /// </summary>
    /// <param name = "user">كائن المستخدم المستهدف</param>
    /// <returns>استجابة المصادقة متضمنة التوكن وتاريخ انتهاء الصلاحية وقائمة الصلاحيات</returns>
    private async Task<AuthResponseDto> GenerateJwtTokenAsync(ApplicationUser user)
    {
        // قراءة المفتاح السري لتوقيع التوكن من ملف الإعدادات أو استخدام القيمة الافتراضية
        var secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
        // قراءة مصدر التوكن (Issuer)
        var issuer = _configuration["JwtSettings:Issuer"] ?? "RetalSystemAPI";
        // قراءة الجمهور المستهدف (Audience)
        var audience = _configuration["JwtSettings:Audience"] ?? "RetalSystemAPI-Client";
        // قراءة مدة صلاحية التوكن بالساعات مع تعيين 8 ساعات كقيمة افتراضية
        var expiryHours = int.TryParse(_configuration["JwtSettings:ExpiryHours"], out var hours) ? hours : 8;
        // تجهيز المفتاح المتماثل من بايتات المفتاح السري
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        // إعداد بيانات التوقيع الرقمي باستخدام خوارزمية HMAC-SHA256
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        // حساب وقت وتاريخ انتهاء صلاحية التوكن بالتوقيت العالمي UTC
        var expiresAt = DateTime.UtcNow.AddHours(expiryHours);
        // جلب قائمة أسماء الأدوار المسندة للمستخدم من Identity
        var resolvedPermissions = await UserPermissionResolver.ResolveAsync(user, _userManager, _roleManager);
        var roles = resolvedPermissions.Roles;
        // بناء قائمة المطالبات (Claims) الأساسية لبيانات الجلسة
        var claims = new List<Claim>
        {
            // إضافة معرف المستخدم
            new(ClaimTypes.NameIdentifier, user.Id),
            // إضافة اسم المستخدم
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            // إضافة معرف المستأجر
            new("TenantId", user.TenantId.ToString()),
            // إضافة معرف الفرع
            new("BranchId", user.BranchId.ToString()),
            // نسخة الجلسة المستخدمة لإبطال الرموز عند الخروج أو تغيير الصلاحيات
            new("SecurityStamp", user.SecurityStamp ?? string.Empty)
        };
        // إضافة كل دور من أدوار المستخدم كمطالبة منفصلة
        foreach (var role in roles)
        {
            // إضافة مطالبة الدور
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // إضافة البريد الإلكتروني كمطالبة في حال توفره
        if (!string.IsNullOrEmpty(user.Email))
        {
            // إضافة مطالبة البريد الإلكتروني
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var permissions = resolvedPermissions.EffectivePermissions;

        // تضمين كافة الصلاحيات المجمعة داخل مطالبات التوكن JWT
        foreach (var perm in permissions)
        {
            // إضافة مطالبة الصلاحية إلى قائمة المطالبات
            claims.Add(new Claim(Permissions.ClaimType, perm));
        }

        // إعداد واصف رمز التوثيق متضمناً الهوية والمصدر والجمهور ومدة الانتهاء وبيانات التوقيع
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // تعيين موضوع التوكن وهوية المطالبات
            Subject = new ClaimsIdentity(claims),
            // تعيين تاريخ ووقت انتهاء الصلاحية
            Expires = expiresAt,
            // تعيين جهة الإصدار
            Issuer = issuer,
            // تعيين الجمهور
            Audience = audience,
            // تعيين بيانات التوقيع والمفتاح السري
            SigningCredentials = credentials
        };
        // إنشاء معالج رموز JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        // إنشاء كائن رمز التوثيق
        var token = tokenHandler.CreateToken(tokenDescriptor);
        // تحويل الرمز إلى صيغة نصية مشفرة (String Token)
        var tokenString = tokenHandler.WriteToken(token);
        // بناء كائن الاستجابة بكافة البيانات المطلوبة للعميل
        return new AuthResponseDto
        {
            // نص التوكن المشفر
            Token = tokenString,
            // رمز التحديث
            RefreshToken = string.Empty,
            // تاريخ انتهاء الصلاحية
            ExpiresAt = expiresAt,
            // معرف المستخدم
            UserId = user.Id,
            // اسم المستخدم
            UserName = user.UserName ?? string.Empty,
            // معرف المستأجر
            TenantId = user.TenantId,
            // معرف الفرع
            BranchId = user.BranchId,
            // قائمة الأدوار
            Roles = roles.ToList(),
            // قائمة الصلاحيات مرتبة أبجدياً
            Permissions = permissions.OrderBy(p => p).ToList()
        };
    }
}
