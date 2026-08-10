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

namespace RetalSystemAPI.Services.Auth.Implementations;

/// <summary>
/// تنفيذ خدمة المصادقة وتوليد رميز التوثيق JWT.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

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
        var authResponse = GenerateJwtToken(user);
        return ServiceResult<AuthResponseDto>.Success(authResponse);
    }

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

        var authResponse = GenerateJwtToken(user);
        return ServiceResult<AuthResponseDto>.Success(authResponse);
    }

    public Task<ServiceResult> LogoutAsync(string userId, CancellationToken ct = default)
    {
        return Task.FromResult(ServiceResult.Success());
    }

    private AuthResponseDto GenerateJwtToken(ApplicationUser user)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? "DefaultSuperSecretKeyForRetalSystemAPI123456789!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "RetalSystemAPI";
        var audience = _configuration["JwtSettings:Audience"] ?? "RetalSystemAPI-Client";
        var expiryHours = int.TryParse(_configuration["JwtSettings:ExpiryHours"], out var hours) ? hours : 8;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddHours(expiryHours);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("TenantId", user.TenantId.ToString()),
            new("BranchId", user.BranchId.ToString())
        };

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
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
            BranchId = user.BranchId
        };
    }
}
