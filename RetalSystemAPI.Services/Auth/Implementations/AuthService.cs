using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.DTOs.Auth;
using RetalSystemAPI.Services.Auth.Interfaces;
using RetalSystemAPI.Services.Common.Models;

using RetalSystemAPI.Models.Constants;

namespace RetalSystemAPI.Services.Auth.Implementations;

/// <summary>
/// تنفيذ خدمة المصادقة وتوليد رموز التوثيق JWT والتحقق من حسابات المستخدمين والمستأجرين.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// تهيئة خدمة المصادقة مع حقن مدير المستخدمين ومدير الأدوار وإعدادات التكوين ووحدة العمل.
    /// </summary>
    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        // 1. التحقق من وجود المستأجر
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(dto.TenantId, ct);
        if (tenant is null)
        {
            return ServiceResult<AuthResponseDto>.Failure("المستأجر المحدد غير موجود", ErrorCodes.TenantNotFound);
        }

        // 2. التحقق من تكرار المستخدم
        var existingUser = await _userManager.FindByNameAsync(dto.UserName);
        if (existingUser is not null)
        {
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم موجود بالفعل", ErrorCodes.UserAlreadyExists);
        }

        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingEmail is not null)
        {
            return ServiceResult<AuthResponseDto>.Failure("البريد الإلكتروني مستخدم بالفعل", ErrorCodes.UserAlreadyExists);
        }

        // 3. إنشاء حساب المستخدم
        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            TenantId = dto.TenantId,
            BranchId = dto.BranchId
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors);
            return ServiceResult<AuthResponseDto>.Failure($"فشل إنشاء الحساب: {errors}", ErrorCodes.ValidationError);
        }

        // 4. توليد الرمز والرد
        var authResponse = await GenerateJwtTokenAsync(user);
        return ServiceResult<AuthResponseDto>.Success(authResponse);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName)
                   ?? await _userManager.FindByEmailAsync(dto.UserName);

        if (user is null)
        {
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", ErrorCodes.InvalidCredentials);
        }

        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            return ServiceResult<AuthResponseDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", ErrorCodes.InvalidCredentials);
        }

        var authResponse = await GenerateJwtTokenAsync(user);
        return ServiceResult<AuthResponseDto>.Success(authResponse);
    }

    /// <inheritdoc />
    public Task<ServiceResult> LogoutAsync(string userId, CancellationToken ct = default)
    {
        return Task.FromResult(ServiceResult.Success());
    }

    /// <summary>
    /// توليد توكن مصادقة JWT مشفر وموقع يحمل بيانات ومعرفات المستخدم والمستأجر والفرع والأدوار.
    /// </summary>
    /// <param name="user">كائن المستخدم</param>
    /// <returns>استجابة المصادقة متضمنة التوكن وتاريخ انتهاء الصلاحية</returns>
    private async Task<AuthResponseDto> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? "DefaultSuperSecretKeyForRetalSystemAPI123456789!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "RetalSystemAPI";
        var audience = _configuration["JwtSettings:Audience"] ?? "RetalSystemAPI-Client";
        var expiryHours = int.TryParse(_configuration["JwtSettings:ExpiryHours"], out var hours) ? hours : 8;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddHours(expiryHours);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("TenantId", user.TenantId.ToString()),
            new("BranchId", user.BranchId.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        // استخراج وتجميع كافة الصلاحيات الممنوحة للمستخدم
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool isAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));
        
        if (isAdmin)
        {
            // المسؤول يمتلك كافة صلاحيات النظام بدون قيد
            foreach (var perm in AppPermissions.GetAllPermissionCodes())
            {
                permissions.Add(perm);
            }
        }
        else
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            bool isCustomized = userClaims.Any(c => c.Type == "Permissions.Customized" && c.Value == "true");

            if (isCustomized)
            {
                // إذا تم تخصيص صلاحيات مستقلة للمستخدم، تُعتمد صلاحياته المباشرة المخصصة له فقط دون فرض باقي صلاحيات الدور
                foreach (var c in userClaims.Where(uc => uc.Type == Permissions.ClaimType))
                {
                    if (!string.IsNullOrWhiteSpace(c.Value))
                    {
                        permissions.Add(c.Value);
                    }
                }
            }
            else
            {
                // إذا لم يتم التخصيص، يرث المستخدم صلاحيات أدواره المسندة تلقائياً
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        foreach (var c in roleClaims.Where(rc => rc.Type == Permissions.ClaimType))
                        {
                            if (!string.IsNullOrWhiteSpace(c.Value))
                            {
                                permissions.Add(c.Value);
                            }
                        }
                    }
                }

                // إضافة أي صلاحيات إضافية مباشرة إن وجدت
                foreach (var c in userClaims.Where(uc => uc.Type == Permissions.ClaimType))
                {
                    if (!string.IsNullOrWhiteSpace(c.Value))
                    {
                        permissions.Add(c.Value);
                    }
                }
            }
        }

        foreach (var perm in permissions)
        {
            claims.Add(new Claim(Permissions.ClaimType, perm));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new AuthResponseDto
        {
            Token = tokenString,
            RefreshToken = string.Empty,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            Roles = roles.ToList(),
            Permissions = permissions.OrderBy(p => p).ToList()
        };
    }
}
