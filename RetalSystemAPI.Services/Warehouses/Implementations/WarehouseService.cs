using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Warehouses.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة المخازن وصالات العرض.
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<WarehouseResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(new WarehouseWithDetailsSpec(id), ct);
        if (warehouse is null)
        {
            return ServiceResult<WarehouseResponseDto>.Failure("المخزن أو الصالة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        var dto = _mapper.Map<WarehouseResponseDto>(warehouse);
        return ServiceResult<WarehouseResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<IReadOnlyList<WarehouseSummaryDto>>> GetAllAsync(Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default)
    {
        var spec = new WarehouseWithDetailsSpec(branchId, type);
        var warehouses = await _unitOfWork.Warehouses.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<WarehouseSummaryDto>>(warehouses);

        return ServiceResult<IReadOnlyList<WarehouseSummaryDto>>.Success(dtos);
    }

    public async Task<ServiceResult<PagedResult<WarehouseSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default)
    {
        var spec = new WarehouseWithDetailsSpec(branchId, type);
        var (items, totalCount) = await _unitOfWork.Warehouses.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<WarehouseSummaryDto>>(items);
        var pagedResult = PagedResult<WarehouseSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<WarehouseSummaryDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<WarehouseResponseDto>> CreateAsync(CreateWarehouseDto dto, CancellationToken ct = default)
    {
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<WarehouseResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        bool nameExists = await _unitOfWork.Warehouses.ExistsAsync(w => w.Name == dto.Name && w.BranchId == dto.BranchId, ct);
        if (nameExists)
        {
            return ServiceResult<WarehouseResponseDto>.Failure("اسم المخزن/الصالة مستخدم بالفعل في هذا الفرع", ErrorCodes.WarehouseNameExists);
        }

        var warehouse = _mapper.Map<Warehouse>(dto);
        warehouse.IsActive = true;

        await _unitOfWork.Warehouses.AddAsync(warehouse, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // توليد أسطر المخزون تلقائياً للمخزن الجديد
        if (warehouse.Type == WarehouseType.Storge)
        {
            var allBarCodes = await _unitOfWork.ProductBarCodes.GetAllAsync(ct);
            foreach (var bc in allBarCodes)
            {
                var storgeStock = new StorgeStock
                {
                    TenantId = warehouse.TenantId,
                    WarehouseId = warehouse.Id,
                    ProductBarcodeId = bc.Id,
                    Quantity = 0,
                    MinStockLevel = 0
                };
                await _unitOfWork.StorgeStocks.AddAsync(storgeStock, ct);
            }
            await _unitOfWork.SaveChangesAsync(ct);
        }
        else if (warehouse.Type == WarehouseType.Show)
        {
            var allProducts = await _unitOfWork.Products.GetAllAsync(ct);
            foreach (var p in allProducts)
            {
                var showStock = new ShowroomStock
                {
                    TenantId = warehouse.TenantId,
                    WarehouseId = warehouse.Id,
                    ProductId = p.Id,
                    Quantity = 0,
                    MinStockLevel = 0
                };
                await _unitOfWork.ShowroomStocks.AddAsync(showStock, ct);
            }
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var created = await _unitOfWork.Warehouses.FirstOrDefaultAsync(new WarehouseWithDetailsSpec(warehouse.Id), ct) ?? warehouse;
        var responseDto = _mapper.Map<WarehouseResponseDto>(created);

        return ServiceResult<WarehouseResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<WarehouseResponseDto>> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(new WarehouseWithDetailsSpec(id), ct);
        if (warehouse is null)
        {
            return ServiceResult<WarehouseResponseDto>.Failure("المخزن أو الصالة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        bool nameExists = await _unitOfWork.Warehouses.ExistsAsync(w => w.Name == dto.Name && w.BranchId == warehouse.BranchId && w.Id != id, ct);
        if (nameExists)
        {
            return ServiceResult<WarehouseResponseDto>.Failure("اسم المخزن/الصالة مستخدم بالفعل في هذا الفرع", ErrorCodes.WarehouseNameExists);
        }

        _mapper.Map(dto, warehouse);
        warehouse.Id = id;

        _unitOfWork.Warehouses.Update(warehouse);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.Warehouses.FirstOrDefaultAsync(new WarehouseWithDetailsSpec(id), ct) ?? warehouse;
        var responseDto = _mapper.Map<WarehouseResponseDto>(updated);

        return ServiceResult<WarehouseResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, ct);
        if (warehouse is null)
        {
            return ServiceResult.Failure("المخزن أو الصالة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        bool hasStorgeStock = await _unitOfWork.StorgeStocks.ExistsAsync(s => s.WarehouseId == id && s.Quantity > 0, ct);
        bool hasShowroomStock = await _unitOfWork.ShowroomStocks.ExistsAsync(s => s.WarehouseId == id && s.Quantity > 0, ct);

        if (hasStorgeStock || hasShowroomStock)
        {
            return ServiceResult.Failure("لا يمكن حذف المخزن/الصالة لوجود كميات مخزونية مسجلة بها", ErrorCodes.WarehouseHasStock);
        }

        _unitOfWork.Warehouses.SoftDelete(warehouse);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, ct);
        if (warehouse is null)
        {
            return ServiceResult.Failure("المخزن أو الصالة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        warehouse.IsActive = !warehouse.IsActive;
        _unitOfWork.Warehouses.Update(warehouse);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
