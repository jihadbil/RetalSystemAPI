using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Tenant.Interfaces;
using TenantEntity = RetalSystemAPI.Models.Tenant;

namespace RetalSystemAPI.Services.Tenant.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة المستأجرين على مستوى المنظومة وتدقيق الأسماء وحالات النشاط وإحصائيات المنشأة.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة المستأجرين مع حقن وحدة العمل وسياق البيانات والمحول.
    /// </summary>
    public TenantService(IUnitOfWork unitOfWork, AppDbContext dbContext, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TenantResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return ServiceResult<TenantResponseDto>.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        var result = _mapper.Map<TenantResponseDto>(tenant);
        await PopulateTenantStatsAsync(result, id, ct);

        return ServiceResult<TenantResponseDto>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<TenantResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var tenants = await _unitOfWork.Tenants.GetAllAsync(ct);
        var result = _mapper.Map<IReadOnlyList<TenantResponseDto>>(tenants);
        return ServiceResult<IReadOnlyList<TenantResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<TenantResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.Tenants.GetPagedAsync(
            pageNumber,
            pageSize,
            orderBy: t => t.CreatedAt,
            ascending: false,
            ct: ct);

        var dtos = _mapper.Map<IReadOnlyList<TenantResponseDto>>(items);
        var pagedResult = PagedResult<TenantResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<TenantResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TenantResponseDto>> CreateAsync(CreateTenantDto dto, CancellationToken ct = default)
    {
        bool nameExists = await _unitOfWork.Tenants.ExistsAsync(t => t.Name == dto.Name, ct);
        if (nameExists)
        {
            return ServiceResult<TenantResponseDto>.Failure("اسم المستأجر مستخدم بالفعل", ErrorCodes.TenantNameExists);
        }

        var tenant = _mapper.Map<TenantEntity>(dto);
        await _unitOfWork.Tenants.AddAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<TenantResponseDto>(tenant);
        return ServiceResult<TenantResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto dto, CancellationToken ct = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return ServiceResult<TenantResponseDto>.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        bool nameExists = await _unitOfWork.Tenants.ExistsAsync(t => t.Name == dto.Name && t.Id != id, ct);
        if (nameExists)
        {
            return ServiceResult<TenantResponseDto>.Failure("اسم المستأجر مستخدم بالفعل لدى مستأجر آخر", ErrorCodes.TenantNameExists);
        }

        _mapper.Map(dto, tenant);
        tenant.Id = id; // التأكد من ثبات الهوية

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<TenantResponseDto>(tenant);
        await PopulateTenantStatsAsync(responseDto, id, ct);

        return ServiceResult<TenantResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TenantResponseDto>> UpdateLogoAsync(Guid id, string logoUrl, CancellationToken ct = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return ServiceResult<TenantResponseDto>.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        tenant.LogoUrl = logoUrl;
        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<TenantResponseDto>(tenant);
        await PopulateTenantStatsAsync(responseDto, id, ct);

        return ServiceResult<TenantResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return ServiceResult.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        _unitOfWork.Tenants.SoftDelete(tenant);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return ServiceResult.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        tenant.IsActive = !tenant.IsActive;
        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    private async Task PopulateTenantStatsAsync(TenantResponseDto dto, Guid tenantId, CancellationToken ct)
    {
        dto.BranchesCount = await _dbContext.Branches.IgnoreQueryFilters().CountAsync(b => b.TenantId == tenantId && !b.IsDeleted, ct);
        dto.WarehousesCount = await _dbContext.Warehouses.IgnoreQueryFilters().CountAsync(w => w.TenantId == tenantId && !w.IsDeleted, ct);
        dto.UsersCount = await _dbContext.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == tenantId, ct);
    }
}
