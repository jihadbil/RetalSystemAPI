using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// تنفيذ خدمة وحدات القياس والتحقق من عدم تكرار الأسماء والتحقق من عدم وجود ارتباطات سابقة عند الحذف.
/// </summary>
public class UnitService : IUnitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة وحدات القياس مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    public UnitService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UnitResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var unit = await _unitOfWork.Units.GetByIdAsync(id, ct);
        if (unit is null)
        {
            return ServiceResult<UnitResponseDto>.Failure("الوحدة غير موجودة", ErrorCodes.UnitNotFound);
        }

        var result = _mapper.Map<UnitResponseDto>(unit);
        return ServiceResult<UnitResponseDto>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<UnitResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var units = await _unitOfWork.Units.GetAllAsync(ct);
        var result = _mapper.Map<IReadOnlyList<UnitResponseDto>>(units);
        return ServiceResult<IReadOnlyList<UnitResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UnitResponseDto>> CreateAsync(CreateUnitDto dto, CancellationToken ct = default)
    {
        bool nameExists = await _unitOfWork.Units.ExistsAsync(u => u.Name == dto.Name, ct);
        if (nameExists)
        {
            return ServiceResult<UnitResponseDto>.Failure("اسم الوحدة موجود بالفعل", ErrorCodes.UnitNameExists);
        }

        var unit = _mapper.Map<Unit>(dto);
        await _unitOfWork.Units.AddAsync(unit, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<UnitResponseDto>(unit);
        return ServiceResult<UnitResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UnitResponseDto>> UpdateAsync(Guid id, UpdateUnitDto dto, CancellationToken ct = default)
    {
        var unit = await _unitOfWork.Units.GetByIdAsync(id, ct);
        if (unit is null)
        {
            return ServiceResult<UnitResponseDto>.Failure("الوحدة غير موجودة", ErrorCodes.UnitNotFound);
        }

        bool nameExists = await _unitOfWork.Units.ExistsAsync(u => u.Name == dto.Name && u.Id != id, ct);
        if (nameExists)
        {
            return ServiceResult<UnitResponseDto>.Failure("اسم الوحدة موجود بالفعل لدى وحدة أخرى", ErrorCodes.UnitNameExists);
        }

        _mapper.Map(dto, unit);
        unit.Id = id;

        _unitOfWork.Units.Update(unit);
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<UnitResponseDto>(unit);
        return ServiceResult<UnitResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var unit = await _unitOfWork.Units.GetByIdAsync(id, ct);
        if (unit is null)
        {
            return ServiceResult.Failure("الوحدة غير موجودة", ErrorCodes.UnitNotFound);
        }

        bool inUse = await _unitOfWork.ProductUnits.ExistsAsync(pu => pu.UnitId == id, ct);
        if (inUse)
        {
            return ServiceResult.Failure("لا يمكن حذف الوحدة لأنها مرتبطة بمنتجات قائمة", ErrorCodes.UnitInUse);
        }

        _unitOfWork.Units.SoftDelete(unit);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
