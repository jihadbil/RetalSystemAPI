using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Suppliers.Interfaces;
using RetalSystemAPI.Services.Suppliers.Specifications;

namespace RetalSystemAPI.Services.Suppliers.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة الموردين، حساباتهم، وجهات الاتصال وأرقام الهواتف التابعة لهم.
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة الموردين مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SupplierResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(id), ct);
        if (supplier is null)
        {
            return ServiceResult<SupplierResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        var dto = _mapper.Map<SupplierResponseDto>(supplier);
        return ServiceResult<SupplierResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<SupplierSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var suppliers = await _unitOfWork.Suppliers.FindAsync(new SupplierWithDetailsSpec(), ct);
        var dtos = _mapper.Map<IReadOnlyList<SupplierSummaryDto>>(suppliers);
        return ServiceResult<IReadOnlyList<SupplierSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<SupplierSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, string? search = null, CancellationToken ct = default)
    {
        var spec = new SupplierWithDetailsSpec(search);
        var (items, totalCount) = await _unitOfWork.Suppliers.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<SupplierSummaryDto>>(items);
        var pagedResult = PagedResult<SupplierSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<SupplierSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SupplierResponseDto>> CreateAsync(CreateSupplierDto dto, CancellationToken ct = default)
    {
        bool exists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Name == dto.Name, ct);
        if (exists)
        {
            return ServiceResult<SupplierResponseDto>.Failure("اسم المورد موجود بالفعل", ErrorCodes.SupplierNameExists);
        }

        var supplier = _mapper.Map<Supplier>(dto);
        if (dto.Phones != null && dto.Phones.Any())
        {
            supplier.SupplierPhones = dto.Phones.Select(p => new SupplierPhone
            {
                PhoneNumber = p.PhoneNumber,
                Name = p.Name
            }).ToList();
        }

        await _unitOfWork.Suppliers.AddAsync(supplier, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var created = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(supplier.Id), ct) ?? supplier;
        var responseDto = _mapper.Map<SupplierResponseDto>(created);

        return ServiceResult<SupplierResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SupplierResponseDto>> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken ct = default)
    {
        var supplier = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(id), ct);
        if (supplier is null)
        {
            return ServiceResult<SupplierResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        bool nameExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Name == dto.Name && s.Id != id, ct);
        if (nameExists)
        {
            return ServiceResult<SupplierResponseDto>.Failure("اسم المورد مستخدم بالفعل لمورد آخر", ErrorCodes.SupplierNameExists);
        }

        _mapper.Map(dto, supplier);
        supplier.Id = id;

        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(id), ct) ?? supplier;
        var responseDto = _mapper.Map<SupplierResponseDto>(updated);

        return ServiceResult<SupplierResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, ct);
        if (supplier is null)
        {
            return ServiceResult.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        _unitOfWork.Suppliers.SoftDelete(supplier);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SupplierResponseDto>> AddPhoneAsync(Guid supplierId, SupplierPhoneDto dto, CancellationToken ct = default)
    {
        var supplier = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(supplierId), ct);
        if (supplier is null)
        {
            return ServiceResult<SupplierResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        var phone = new SupplierPhone
        {
            SupplierId = supplierId,
            PhoneNumber = dto.PhoneNumber,
            Name = dto.Name
        };

        await _unitOfWork.SupplierPhones.AddAsync(phone, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(supplierId), ct) ?? supplier;
        var responseDto = _mapper.Map<SupplierResponseDto>(updated);

        return ServiceResult<SupplierResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeletePhoneAsync(Guid supplierId, Guid phoneId, CancellationToken ct = default)
    {
        var phone = await _unitOfWork.SupplierPhones.GetByIdAsync(phoneId, ct);
        if (phone is null || phone.SupplierId != supplierId)
        {
            return ServiceResult.Failure("رقم الهاتف غير موجود أو لا ينتمي لهذا المورد", ErrorCodes.NotFound);
        }

        _unitOfWork.SupplierPhones.SoftDelete(phone);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
