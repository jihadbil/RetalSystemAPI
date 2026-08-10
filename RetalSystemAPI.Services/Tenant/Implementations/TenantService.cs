using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Tenant.Interfaces;
using TenantEntity = RetalSystemAPI.Models.Tenant;

namespace RetalSystemAPI.Services.Tenant.Implementations;

/// <summary>
/// تنفيذ خدمة المستأجرين لإدارة بيانات المستأجرين على مستوى النظام.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TenantService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<TenantResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return ServiceResult<TenantResponseDto>.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        var result = _mapper.Map<TenantResponseDto>(tenant);
        return ServiceResult<TenantResponseDto>.Success(result);
    }

    public async Task<ServiceResult<IReadOnlyList<TenantResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var tenants = await _unitOfWork.Tenants.GetAllAsync(ct);
        var result = _mapper.Map<IReadOnlyList<TenantResponseDto>>(tenants);
        return ServiceResult<IReadOnlyList<TenantResponseDto>>.Success(result);
    }

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
        return ServiceResult<TenantResponseDto>.Success(responseDto);
    }

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
}
