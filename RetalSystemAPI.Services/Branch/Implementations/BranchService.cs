using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.DTOs.Branch;
using RetalSystemAPI.Services.Branch.Interfaces;
using RetalSystemAPI.Services.Branch.Specifications;
using RetalSystemAPI.Services.Common.Models;
using BranchEntity = RetalSystemAPI.Models.Branchs.Branch;

namespace RetalSystemAPI.Services.Branch.Implementations;

/// <summary>
/// تنفيذ خدمة الفروع لإدارة فروع المستأجر وتفاصيلها وأرقام هواتفها مع تطبيق قواعد العمل والتحقق من التكرار.
/// </summary>
public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة الفروع مع حقن وحدة العمل والمحول AutoMapper.
    /// </summary>
    public BranchService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BranchResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var branch = await _unitOfWork.Branches.FirstOrDefaultAsync(new BranchWithPhonesSpec(id), ct);
        if (branch is null)
        {
            return ServiceResult<BranchResponseDto>.Failure("الفرع غير موجود", ErrorCodes.BranchNotFound);
        }

        var result = _mapper.Map<BranchResponseDto>(branch);
        return ServiceResult<BranchResponseDto>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<BranchResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var branches = await _unitOfWork.Branches.FindAsync(new BranchWithPhonesSpec(), ct);
        var result = _mapper.Map<IReadOnlyList<BranchResponseDto>>(branches);
        return ServiceResult<IReadOnlyList<BranchResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<BranchResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var spec = new BranchWithPhonesSpec();
        var (items, totalCount) = await _unitOfWork.Branches.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<BranchResponseDto>>(items);
        var pagedResult = PagedResult<BranchResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<BranchResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BranchResponseDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default)
    {
        bool nameExists = await _unitOfWork.Branches.ExistsAsync(b => b.Name == dto.Name, ct);
        if (nameExists)
        {
            return ServiceResult<BranchResponseDto>.Failure("اسم الفرع موجود بالفعل", ErrorCodes.BranchNameExists);
        }

        var branch = _mapper.Map<BranchEntity>(dto);
        if (dto.Phones != null && dto.Phones.Any())
        {
            branch.BranchPhones = dto.Phones.Select(p => new BranchPhone
            {
                Name = string.IsNullOrWhiteSpace(p.Name) ? "الرئيسي" : p.Name,
                PhoneNumber = p.PhoneNumber
            }).ToList();
        }

        await _unitOfWork.Branches.AddAsync(branch, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // اعادة الجلب مع الهواتف للملاءمة
        var createdBranch = await _unitOfWork.Branches.FirstOrDefaultAsync(new BranchWithPhonesSpec(branch.Id), ct) ?? branch;
        var responseDto = _mapper.Map<BranchResponseDto>(createdBranch);

        return ServiceResult<BranchResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BranchResponseDto>> UpdateAsync(Guid id, UpdateBranchDto dto, CancellationToken ct = default)
    {
        var branch = await _unitOfWork.Branches.FirstOrDefaultAsync(new BranchWithPhonesSpec(id), ct);
        if (branch is null)
        {
            return ServiceResult<BranchResponseDto>.Failure("الفرع غير موجود", ErrorCodes.BranchNotFound);
        }

        bool nameExists = await _unitOfWork.Branches.ExistsAsync(b => b.Name == dto.Name && b.Id != id, ct);
        if (nameExists)
        {
            return ServiceResult<BranchResponseDto>.Failure("اسم الفرع موجود بالفعل لدى فرع آخر", ErrorCodes.BranchNameExists);
        }

        // تحديث البيانات الأساسية
        branch.Name = dto.Name;
        branch.Address = dto.Address;
        branch.IsActive = dto.IsActive;

        // تحديث هواتف الفرع (إعادة بناء الهواتف)
        foreach (var phone in branch.BranchPhones.ToList())
        {
            _unitOfWork.BranchPhones.HardDelete(phone);
        }

        if (dto.Phones.Count > 0)
        {
            var newPhones = dto.Phones.Select(p => new BranchPhone
            {
                BranchId = id,
                Name = p.Name,
                PhoneNumber = p.PhoneNumber
            }).ToList();

            await _unitOfWork.BranchPhones.AddRangeAsync(newPhones, ct);
        }

        _unitOfWork.Branches.Update(branch);
        await _unitOfWork.SaveChangesAsync(ct);

        var updatedBranch = await _unitOfWork.Branches.FirstOrDefaultAsync(new BranchWithPhonesSpec(id), ct) ?? branch;
        var responseDto = _mapper.Map<BranchResponseDto>(updatedBranch);

        return ServiceResult<BranchResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var branch = await _unitOfWork.Branches.FirstOrDefaultAsync(new BranchWithPhonesAndUsersSpec(id), ct);
        if (branch is null)
        {
            return ServiceResult.Failure("الفرع غير موجود", ErrorCodes.BranchNotFound);
        }

        if (branch.ApplicationUsers.Any())
        {
            return ServiceResult.Failure("لا يمكن حذف الفرع لوجود مستخدمين نشطين مرتبطين به", ErrorCodes.BranchHasUsers);
        }

        _unitOfWork.Branches.SoftDelete(branch);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id, ct);
        if (branch is null)
        {
            return ServiceResult.Failure("الفرع غير موجود", ErrorCodes.BranchNotFound);
        }

        branch.IsActive = !branch.IsActive;
        _unitOfWork.Branches.Update(branch);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
