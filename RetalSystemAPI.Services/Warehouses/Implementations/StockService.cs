using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Warehouses.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة رصيد المخزون في المخازن وصالات العرض.
/// </summary>
public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StockService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // ── Storge Stock Implementation ──────────────────────────

    public async Task<ServiceResult<StorgeStockResponseDto>> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        var spec = new StorgeStockWithDetailsSpec(warehouseId, productBarcodeId);
        var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(spec, ct);

        if (stock is null)
        {
            return ServiceResult<StorgeStockResponseDto>.Failure("سجل مخزون المخزن غير موجود لهذا الباركود", ErrorCodes.StockNotFound);
        }

        var dto = _mapper.Map<StorgeStockResponseDto>(stock);
        return ServiceResult<StorgeStockResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetStorgeStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
    {
        // 1. استعلام كافة الباركودات/النكهات في النظام
        var allBarCodes = await _unitOfWork.ProductBarCodes.GetAllAsync(ct);
        var existingStocks = await _unitOfWork.StorgeStocks.FindAsync(s => s.WarehouseId == warehouseId, ct);
        var existingBarcodeIds = new HashSet<Guid>(existingStocks.Select(s => s.ProductBarcodeId));

        bool hasNew = false;
        foreach (var bc in allBarCodes)
        {
            if (!existingBarcodeIds.Contains(bc.Id))
            {
                var newStock = new StorgeStock
                {
                    TenantId = bc.TenantId,
                    WarehouseId = warehouseId,
                    ProductBarcodeId = bc.Id,
                    Quantity = 0,
                    MinStockLevel = 0
                };
                await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                hasNew = true;
            }
        }

        if (hasNew)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var spec = new StorgeStockWithDetailsSpec(warehouseId);
        var stocks = await _unitOfWork.StorgeStocks.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<StorgeStockResponseDto>>(stocks);

        return ServiceResult<IReadOnlyList<StorgeStockResponseDto>>.Success(dtos);
    }

    public async Task<ServiceResult<PagedResult<StorgeStockResponseDto>>> GetPagedStorgeStocksByWarehouseAsync(
        Guid warehouseId,
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var countSpec = new StorgeStockCountSpec(warehouseId, searchTerm);
        int totalCount = await _unitOfWork.StorgeStocks.CountAsync(countSpec, ct);

        var pagedSpec = new StorgeStockWithDetailsSpec(warehouseId, pageNumber, pageSize, searchTerm, isPaged: true);
        var stocks = await _unitOfWork.StorgeStocks.FindAsync(pagedSpec, ct);

        var dtos = _mapper.Map<IReadOnlyList<StorgeStockResponseDto>>(stocks);
        var pagedResult = PagedResult<StorgeStockResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<StorgeStockResponseDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<StorgeStockResponseDto>> SetStorgeStockAsync(SetStorgeStockDto dto, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null || warehouse.Type != WarehouseType.Storge)
        {
            return ServiceResult<StorgeStockResponseDto>.Failure("المخزن المحدد غير موجود أو ليس مخزن تخزين", ErrorCodes.WarehouseNotFound);
        }

        bool barcodeExists = await _unitOfWork.ProductBarCodes.ExistsAsync(b => b.Id == dto.ProductBarcodeId, ct);
        if (!barcodeExists)
        {
            return ServiceResult<StorgeStockResponseDto>.Failure("الباركود المحدد غير موجود", ErrorCodes.BarCodeNotFound);
        }

        var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
            s => s.WarehouseId == dto.WarehouseId && s.ProductBarcodeId == dto.ProductBarcodeId, ct);

        if (stock is null)
        {
            stock = new StorgeStock
            {
                TenantId = warehouse.TenantId,
                WarehouseId = dto.WarehouseId,
                ProductBarcodeId = dto.ProductBarcodeId,
                Quantity = (int)dto.Quantity,
                MinStockLevel = dto.MinStockLevel
            };
            await _unitOfWork.StorgeStocks.AddAsync(stock, ct);
        }
        else
        {
            stock.Quantity = (int)dto.Quantity;
            stock.MinStockLevel = dto.MinStockLevel;
            _unitOfWork.StorgeStocks.Update(stock);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var updatedStock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(new StorgeStockWithDetailsSpec(stock.WarehouseId, stock.ProductBarcodeId), ct) ?? stock;
        var responseDto = _mapper.Map<StorgeStockResponseDto>(updatedStock);

        return ServiceResult<StorgeStockResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetLowStorgeStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        var spec = new LowStorgeStockSpec(warehouseId);
        var stocks = await _unitOfWork.StorgeStocks.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<StorgeStockResponseDto>>(stocks);

        return ServiceResult<IReadOnlyList<StorgeStockResponseDto>>.Success(dtos);
    }

    // ── Showroom Stock Implementation ─────────────────────────

    public async Task<ServiceResult<ShowroomStockResponseDto>> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        var spec = new ShowroomStockWithDetailsSpec(warehouseId, productId);
        var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(spec, ct);

        if (stock is null)
        {
            return ServiceResult<ShowroomStockResponseDto>.Failure("سجل مخزون الصالة غير موجود لهذا المنتج", ErrorCodes.StockNotFound);
        }

        var dto = _mapper.Map<ShowroomStockResponseDto>(stock);
        return ServiceResult<ShowroomStockResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetShowroomStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
    {
        // 1. استعلام كافة المنتجات في النظام
        var allProducts = await _unitOfWork.Products.GetAllAsync(ct);
        var existingStocks = await _unitOfWork.ShowroomStocks.FindAsync(s => s.WarehouseId == warehouseId, ct);
        var existingProductIds = new HashSet<Guid>(existingStocks.Select(s => s.ProductId));

        bool hasNew = false;
        foreach (var p in allProducts)
        {
            if (!existingProductIds.Contains(p.Id))
            {
                var newStock = new ShowroomStock
                {
                    TenantId = p.TenantId,
                    WarehouseId = warehouseId,
                    ProductId = p.Id,
                    Quantity = 0,
                    MinStockLevel = 0
                };
                await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                hasNew = true;
            }
        }

        if (hasNew)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var spec = new ShowroomStockWithDetailsSpec(warehouseId);
        var stocks = await _unitOfWork.ShowroomStocks.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<ShowroomStockResponseDto>>(stocks);

        return ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>.Success(dtos);
    }

    public async Task<ServiceResult<PagedResult<ShowroomStockResponseDto>>> GetPagedShowroomStocksByWarehouseAsync(
        Guid warehouseId,
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var countSpec = new ShowroomStockCountSpec(warehouseId, searchTerm);
        int totalCount = await _unitOfWork.ShowroomStocks.CountAsync(countSpec, ct);

        var pagedSpec = new ShowroomStockWithDetailsSpec(warehouseId, pageNumber, pageSize, searchTerm, isPaged: true);
        var stocks = await _unitOfWork.ShowroomStocks.FindAsync(pagedSpec, ct);

        var dtos = _mapper.Map<IReadOnlyList<ShowroomStockResponseDto>>(stocks);
        var pagedResult = PagedResult<ShowroomStockResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<ShowroomStockResponseDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<ShowroomStockResponseDto>> SetShowroomStockAsync(SetShowroomStockDto dto, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null || warehouse.Type != WarehouseType.Show)
        {
            return ServiceResult<ShowroomStockResponseDto>.Failure("المخزن المحدد غير موجود أو ليس صالة عرض", ErrorCodes.WarehouseNotFound);
        }

        bool productExists = await _unitOfWork.Products.ExistsAsync(p => p.Id == dto.ProductId, ct);
        if (!productExists)
        {
            return ServiceResult<ShowroomStockResponseDto>.Failure("المنتج المحدد غير موجود", ErrorCodes.ProductNotFound);
        }

        var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
            s => s.WarehouseId == dto.WarehouseId && s.ProductId == dto.ProductId, ct);

        if (stock is null)
        {
            stock = new ShowroomStock
            {
                TenantId = warehouse.TenantId,
                WarehouseId = dto.WarehouseId,
                ProductId = dto.ProductId,
                Quantity = (int)dto.Quantity,
                MinStockLevel = dto.MinStockLevel
            };
            await _unitOfWork.ShowroomStocks.AddAsync(stock, ct);
        }
        else
        {
            stock.Quantity = (int)dto.Quantity;
            stock.MinStockLevel = dto.MinStockLevel;
            _unitOfWork.ShowroomStocks.Update(stock);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var updatedStock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(new ShowroomStockWithDetailsSpec(stock.WarehouseId, stock.ProductId), ct) ?? stock;
        var responseDto = _mapper.Map<ShowroomStockResponseDto>(updatedStock);

        return ServiceResult<ShowroomStockResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetLowShowroomStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        var spec = new LowShowroomStockSpec(warehouseId);
        var stocks = await _unitOfWork.ShowroomStocks.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<ShowroomStockResponseDto>>(stocks);

        return ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>.Success(dtos);
    }
}
