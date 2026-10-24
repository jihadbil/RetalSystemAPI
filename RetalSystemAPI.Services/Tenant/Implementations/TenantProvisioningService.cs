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

public sealed class TenantProvisioningService(ITenantExecutionScopeFactory scopes, IHttpContextAccessor http, IMapper mapper)
    : ITenantProvisioningService
{
    public async Task<ServiceResult<ProvisionTenantResponseDto>> ProvisionAsync(ProvisionTenantDto dto, CancellationToken ct = default)
    {
        if (http.HttpContext?.User is not { Identity.IsAuthenticated: true } caller || !caller.IsInRole("SuperAdmin"))
            return ServiceResult<ProvisionTenantResponseDto>.Failure("تهيئة المستأجر تتطلب مسؤول المنصة", ErrorCodes.Forbidden);
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<ProvisionTenantResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        var tenantId = Guid.NewGuid();
        await using var scope = scopes.CreateForProvisioning(tenantId);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            if (await uow.Tenants.NameExistsAsync(dto.Tenant.Name, ct))
                return ServiceResult<ProvisionTenantResponseDto>.Failure("اسم المستأجر مستخدم بالفعل", ErrorCodes.TenantNameExists);
            if (!await roles.RoleExistsAsync("Admin"))
                return ServiceResult<ProvisionTenantResponseDto>.Failure("دور مدير المستأجر غير مهيأ في النظام", ErrorCodes.ValidationError);

            var tenant = mapper.Map<TenantEntity>(dto.Tenant);
            tenant.Id = tenantId;
            var branch = new BranchEntity { Id = Guid.NewGuid(), TenantId = tenantId, Name = dto.BranchName, Address = dto.BranchAddress };
            db.AddRange(tenant, branch);
            await db.SaveChangesAsync(ct);

            var admin = new ApplicationUser
            {
                UserName = dto.AdminUserName, Email = dto.AdminEmail,
                TenantId = tenantId, BranchId = branch.Id, LockoutEnabled = true
            };
            var created = await users.CreateAsync(admin, dto.AdminPassword);
            if (!created.Succeeded) return IdentityFailure(created);
            var assigned = await users.AddToRoleAsync(admin, "Admin");
            if (!assigned.Succeeded) return IdentityFailure(assigned);

            var response = new ProvisionTenantResponseDto { TenantId = tenantId, BranchId = branch.Id, AdminUserId = admin.Id };
            await transaction.CommitAsync(ct);
            return ServiceResult<ProvisionTenantResponseDto>.Success(response);
        }
        catch (DbUpdateException ex) when (SqlConstraintErrors.IsUniqueViolation(ex, "IX_Tenants_Name"))
        {
            return ServiceResult<ProvisionTenantResponseDto>.Failure("اسم المستأجر مستخدم بالفعل", ErrorCodes.TenantNameExists);
        }
        // Disposing an uncommitted transaction rolls back tenant, branch, Identity user and roles together.
    }

    private static ServiceResult<ProvisionTenantResponseDto> IdentityFailure(IdentityResult result) =>
        ServiceResult<ProvisionTenantResponseDto>.Failure(string.Join("، ", result.Errors.Select(e => e.Description)),
            LoginIdentifierValidator.IsDuplicate(result) ? ErrorCodes.UserAlreadyExists : ErrorCodes.ValidationError);
}
