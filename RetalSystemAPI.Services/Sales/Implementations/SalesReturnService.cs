using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Sales.Interfaces;
using RetalSystemAPI.Services.Sales.Specifications;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Sales.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة مرتجعات المبيعات واسترجاع البضاعة للمخزون.
/// </summary>
public class SalesReturnService : ISalesReturnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalesReturnService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<SalesReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var salesReturn = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(id), ct);
        if (salesReturn is null)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        var dto = _mapper.Map<SalesReturnResponseDto>(salesReturn);
        return ServiceResult<SalesReturnResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<SalesReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        var salesReturn = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(returnNumber), ct);
        if (salesReturn is null)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        var dto = _mapper.Map<SalesReturnResponseDto>(salesReturn);
        return ServiceResult<SalesReturnResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new SalesReturnWithDetailsSpec(branchId, warehouseId, customerId, reason, fromDate, toDate, search);
        var returns = await _unitOfWork.SalesReturns.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<SalesReturnSummaryDto>>(returns);

        return ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>.Success(dtos);
    }

    public async Task<ServiceResult<PagedResult<SalesReturnSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new SalesReturnWithDetailsSpec(branchId, warehouseId, customerId, reason, fromDate, toDate, search);
        var (items, totalCount) = await _unitOfWork.SalesReturns.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<SalesReturnSummaryDto>>(items);
        var pagedResult = PagedResult<SalesReturnSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<SalesReturnSummaryDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<SalesReturnResponseDto>> CreateAsync(CreateSalesReturnDto dto, CancellationToken ct = default)
    {
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        if (dto.CustomerId.HasValue)
        {
            bool customerExists = await _unitOfWork.Customers.ExistsAsync(c => c.Id == dto.CustomerId.Value, ct);
            if (!customerExists)
            {
                return ServiceResult<SalesReturnResponseDto>.Failure("العميل المحدد غير موجود", ErrorCodes.CustomerNotFound);
            }
        }

        if (dto.OriginalInvoiceId.HasValue)
        {
            bool invoiceExists = await _unitOfWork.SalesInvoices.ExistsAsync(i => i.Id == dto.OriginalInvoiceId.Value, ct);
            if (!invoiceExists)
            {
                return ServiceResult<SalesReturnResponseDto>.Failure("الفاتورة الأصلية المحددة غير موجودة", ErrorCodes.SalesInvoiceNotFound);
            }
        }

        bool numExists = await _unitOfWork.SalesReturns.ExistsAsync(r => r.ReturnNumber == dto.ReturnNumber, ct);
        if (numExists)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("رقم المرتجع مستخدم بالفعل", ErrorCodes.SalesReturnNumberExists);
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب إضافة بند واحد على الأقل للمرتجع", ErrorCodes.ValidationError);
        }

        var salesReturn = _mapper.Map<SalesReturn>(dto);
        salesReturn.ReturnDate = dto.ReturnDate == default ? DateTime.UtcNow : dto.ReturnDate;

        salesReturn.Items = dto.Items.Select(item => new SalesReturnItem
        {
            ProductId = item.ProductId,
            ProductBarCodeId = item.ProductBarCodeId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.Quantity * item.UnitPrice,
            Notes = item.Notes
        }).ToList();

        salesReturn.TotalAmount = salesReturn.Items.Sum(i => i.LineTotal);

        // إعادة البضاعة إلى المخزون
        foreach (var item in salesReturn.Items)
        {
            if (warehouse.Type == WarehouseType.Show)
            {
                var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductId == item.ProductId, ct);
                if (stock != null)
                {
                    stock.Quantity += item.Quantity;
                    _unitOfWork.ShowroomStocks.Update(stock);
                }
                else
                {
                    var newStock = new ShowroomStock
                    {
                        TenantId = salesReturn.TenantId,
                        WarehouseId = warehouse.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                }
            }
            else if (warehouse.Type == WarehouseType.Storge && item.ProductBarCodeId.HasValue)
            {
                var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductBarcodeId == item.ProductBarCodeId.Value, ct);
                if (stock != null)
                {
                    stock.Quantity += item.Quantity;
                    _unitOfWork.StorgeStocks.Update(stock);
                }
                else
                {
                    var newStock = new StorgeStock
                    {
                        TenantId = salesReturn.TenantId,
                        WarehouseId = warehouse.Id,
                        ProductBarcodeId = item.ProductBarCodeId.Value,
                        Quantity = item.Quantity,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                }
            }
        }

        await _unitOfWork.SalesReturns.AddAsync(salesReturn, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var created = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(salesReturn.Id), ct) ?? salesReturn;
        var responseDto = _mapper.Map<SalesReturnResponseDto>(created);

        return ServiceResult<SalesReturnResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var salesReturn = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(id), ct);
        if (salesReturn is null)
        {
            return ServiceResult.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        _unitOfWork.SalesReturns.SoftDelete(salesReturn);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
