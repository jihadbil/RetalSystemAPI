using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
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

    /// <summary>
    /// تهيئة خدمة المصادقة مع حقن مدير المستخدمين ومدير الأدوار وإعدادات التكوين ووحدة العمل.
    /// </summary>
    /// <param name="userManager">مدير مستخدمي الهوية</param>
    /// <param name="roleManager">مدير أدوار الهوية</param>
    /// <param name="configuration">إعدادات التطبيق لقراءة تكوينات التوكن</param>
    /// <param name="unitOfWork">وحدة العمل للوصول للمستودعات</param>
    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        // تعيين مرجع مدير المستخدمين
        _userManager = userManager;

        // تعيين مرجع مدير الأدوار
        _roleManager = roleManager;

        // تعيين مرجع التكوينات
        _configuration = configuration;

        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        // 1. التحقق من وجود المستأجر في قاعدة البيانات
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(dto.TenantId, ct);
        // إذا لم يتم العثور على المستأجر، يتم إرجاع نتيجة فشل
        if (tenant is null)
        {
            // إرجاع خطأ عدم وجود المستأجر
            return ServiceResult<AuthResponseDto>.Failure("المستأجر المحدد غير موجود", ErrorCodes.TenantNotFound);
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
            TenantId = dto.TenantId,
            // تعيين معرف الفرع
            BranchId = dto.BranchId
        };

        // حفظ وإنشاء المستخدم مع تشفير كلمة المرور عبر Identity
        var createResult = await _userManager.CreateAsync(user, dto.Password);
        // التحقق من نجاح عملية الإنشاء
        if (!createResult.Succeeded)
        {
            // تجميع رسائل أخطاء التحقق وقواعد كلمة المرور
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            // إرجاع نتيجة فشل التحقق مع التفاصيل
            return ServiceResult<AuthResponseDto>.Failure($"فشل إنشاء الحساب: {errors}", ErrorCodes.ValidationError);
        }

        // 4. توليد رمز JWT وتجهيز بيانات الاستجابة للمستخدم الجديد
        var authResponse = await GenerateJwtTokenAsync(user);

        // إرجاع النتيجة الناجحة متضمنة بيانات الجلسة والتوكن
        return ServiceResult<AuthResponseDto>.Success(authResponse);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        // البحث عن المستخدم بواسطة اسم المستخدم أو البريد الإلكتروني
        var user = await _userManager.FindByNameAsync(dto.UserName)
                   ?? await _userManager.FindByEmailAsync(dto.UserName);

        // التحقق من وجود المستخدم
        if (user is null)
        {
            // إرجاع خطأ في بيانات الاعتماد عند عدم العثور على الحساب
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", ErrorCodes.InvalidCredentials);
        }

        // التحقق من صحة كلمة المرور المدخلة ومطابقتها للتجزئة المخزنة
        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        // إذا كانت كلمة المرور غير مطابقة
        if (!isPasswordValid)
        {
            // إرجاع خطأ في بيانات الاعتماد
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", ErrorCodes.InvalidCredentials);
        }

        // توليد رمز JWT وتجميع صلاحيات وأدوار المستخدم
        var authResponse = await GenerateJwtTokenAsync(user);

        // إرجاع نتيجة نجاح المصادقة مع التوكن
        return ServiceResult<AuthResponseDto>.Success(authResponse);
    }

    /// <inheritdoc />
    public Task<ServiceResult> LogoutAsync(string userId, CancellationToken ct = default)
    {
        // نظراً لأن توكن JWT غير متصل بحالة على الخادم (Stateless)، تكتمل العملية بنجاح تمهيداً لحذف التوكن لدى العميل
        return Task.FromResult(ServiceResult.Success());
    }

    /// <summary>
    /// توليد توكن مصادقة JWT مشفر وموقع يحمل بيانات ومعرفات المستخدم والمستأجر والفرع والأدوار والصلاحيات.
    /// </summary>
    /// <param name="user">كائن المستخدم المستهدف</param>
    /// <returns>استجابة المصادقة متضمنة التوكن وتاريخ انتهاء الصلاحية وقائمة الصلاحيات</returns>
    private async Task<AuthResponseDto> GenerateJwtTokenAsync(ApplicationUser user)
    {
        // قراءة المفتاح السري لتوقيع التوكن من ملف الإعدادات أو استخدام القيمة الافتراضية
        var secretKey = _configuration["JwtSettings:Secret"] ?? "DefaultSuperSecretKeyForRetalSystemAPI123456789!";

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
        var roles = await _userManager.GetRolesAsync(user);

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
            new("BranchId", user.BranchId.ToString())
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

        // استخراج وتجميع كافة الصلاحيات الممنوحة للمستخدم بدون تكرار
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // التحقق مما إذا كان المستخدم يملك دور مسؤول عام أو مسؤول نظام
        bool isAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));

        // فحص حالة المسؤول لمنحه كافة الصلاحيات مباشرة
        if (isAdmin)
        {
            // إضافة كافة أكواد الصلاحيات المعرفة في النظام تلقائياً
            foreach (var perm in AppPermissions.GetAllPermissionCodes())
            {
                // إدراج الصلاحية في المجموعة
                permissions.Add(perm);
            }
        }
        else
        {
            // جلب مطالبات المستخدم المباشرة المخزنة في قاعدة البيانات
            var userClaims = await _userManager.GetClaimsAsync(user);

            // التحقق مما إذا كانت صلاحيات المستخدم مخصصة بشكل يدوي
            bool isCustomized = userClaims.Any(c => c.Type == "Permissions.Customized" && c.Value == "true");

            // فحص حالة التخصيص
            if (isCustomized)
            {
                // في حالة التخصيص، يتم اعتماد صلاحياته المباشرة المخصصة له فقط
                foreach (var c in userClaims.Where(uc => uc.Type == Permissions.ClaimType))
                {
                    // التحقق من صلاحية القيمة
                    if (!string.IsNullOrWhiteSpace(c.Value))
                    {
                        // إضافة الصلاحية المخصصة
                        permissions.Add(c.Value);
                    }
                }
            }
            else
            {
                // في حالة عدم التخصيص، يرث المستخدم صلاحيات كافة أدواره المسندة
                foreach (var roleName in roles)
                {
                    // جلب كائن الدور بالاسم
                    var role = await _roleManager.FindByNameAsync(roleName);
                    // التحقق من وجود الدور
                    if (role != null)
                    {
                        // جلب مطالبات وصلاحيات هذا الدور
                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        // المرور على مطالبات الصلاحيات الخاصة بالدور
                        foreach (var c in roleClaims.Where(rc => rc.Type == Permissions.ClaimType))
                        {
                            // التحقق من وجود قيمة للصلاحية
                            if (!string.IsNullOrWhiteSpace(c.Value))
                            {
                                // إضافة الصلاحية المستمدة من الدور
                                permissions.Add(c.Value);
                            }
                        }
                    }
                }

                // إضافة أي صلاحيات مباشرة فردية إن وجدت للمستخدم
                foreach (var c in userClaims.Where(uc => uc.Type == Permissions.ClaimType))
                {
                    // التحقق من صحة القيمة
                    if (!string.IsNullOrWhiteSpace(c.Value))
                    {
                        // إدراج الصلاحية المباشرة
                        permissions.Add(c.Value);
                    }
                }
            }
        }

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
