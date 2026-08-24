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
/// تنفيذ خدمة إدارة فواتير المبيعات وحركات البيع وخصم المخزون.
/// </summary>
public class SalesInvoiceService : ISalesInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalesInvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<SalesInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        var dto = _mapper.Map<SalesInvoiceResponseDto>(invoice);
        return ServiceResult<SalesInvoiceResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<SalesInvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(invoiceNumber), ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        var dto = _mapper.Map<SalesInvoiceResponseDto>(invoice);
        return ServiceResult<SalesInvoiceResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<IReadOnlyList<SalesInvoiceSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new SalesInvoiceWithDetailsSpec(branchId, warehouseId, customerId, status, paymentMethod, fromDate, toDate, search);
        var invoices = await _unitOfWork.SalesInvoices.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<SalesInvoiceSummaryDto>>(invoices);

        return ServiceResult<IReadOnlyList<SalesInvoiceSummaryDto>>.Success(dtos);
    }

    public async Task<ServiceResult<PagedResult<SalesInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new SalesInvoiceWithDetailsSpec(branchId, warehouseId, customerId, status, paymentMethod, fromDate, toDate, search);
        var (items, totalCount) = await _unitOfWork.SalesInvoices.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<SalesInvoiceSummaryDto>>(items);
        var pagedResult = PagedResult<SalesInvoiceSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<SalesInvoiceSummaryDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<SalesInvoiceResponseDto>> CreateAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default)
    {
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        if (dto.CustomerId.HasValue)
        {
            bool customerExists = await _unitOfWork.Customers.ExistsAsync(c => c.Id == dto.CustomerId.Value, ct);
            if (!customerExists)
            {
                return ServiceResult<SalesInvoiceResponseDto>.Failure("العميل المحدد غير موجود", ErrorCodes.CustomerNotFound);
            }
        }

        bool numExists = await _unitOfWork.SalesInvoices.ExistsAsync(s => s.InvoiceNumber == dto.InvoiceNumber, ct);
        if (numExists)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("رقم الفاتورة مستخدم بالفعل", ErrorCodes.SalesInvoiceNumberExists);
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب إضافة بند واحد على الأقل للفاتورة", ErrorCodes.ValidationError);
        }

        // التحقق من كفاية المخزون وتجهيز البنود
        foreach (var item in dto.Items)
        {
            if (warehouse.Type == WarehouseType.Show)
            {
                var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductId == item.ProductId, ct);
                if (stock is null || stock.Quantity < item.Quantity)
                {
                    return ServiceResult<SalesInvoiceResponseDto>.Failure($"الكمية المطلوبة غير متوفرة في صالة العرض للصنف المحدد", ErrorCodes.InsufficientShowroomStock);
                }
            }
            else if (warehouse.Type == WarehouseType.Storge && item.ProductBarCodeId.HasValue)
            {
                var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductBarcodeId == item.ProductBarCodeId.Value, ct);
                if (stock is null || stock.Quantity < item.Quantity)
                {
                    return ServiceResult<SalesInvoiceResponseDto>.Failure($"الكمية المطلوبة غير متوفرة في المخزن للصنف المحدد", ErrorCodes.InsufficientStock);
                }
            }
        }

        var invoice = _mapper.Map<SalesInvoice>(dto);
        invoice.InvoiceDate = dto.InvoiceDate == default ? DateTime.UtcNow : dto.InvoiceDate;
        invoice.RemainingAmount = dto.TotalAmount - dto.PaidAmount;

        invoice.Items = dto.Items.Select(item => new SalesInvoiceItem
        {
            ProductId = item.ProductId,
            ProductBarCodeId = item.ProductBarCodeId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            UnitCost = item.UnitCost,
            DiscountAmount = item.DiscountAmount,
            LineTotal = (item.Quantity * item.UnitPrice) - item.DiscountAmount
        }).ToList();

        // خصم الكميات من المخزون
        foreach (var item in invoice.Items)
        {
            if (warehouse.Type == WarehouseType.Show)
            {
                var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductId == item.ProductId, ct);
                if (stock != null)
                {
                    stock.Quantity -= item.Quantity;
                    _unitOfWork.ShowroomStocks.Update(stock);
                }
            }
            else if (warehouse.Type == WarehouseType.Storge && item.ProductBarCodeId.HasValue)
            {
                var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductBarcodeId == item.ProductBarCodeId.Value, ct);
                if (stock != null)
                {
                    stock.Quantity -= item.Quantity;
                    _unitOfWork.StorgeStocks.Update(stock);
                }
            }
        }

        await _unitOfWork.SalesInvoices.AddAsync(invoice, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var created = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(invoice.Id), ct) ?? invoice;
        var responseDto = _mapper.Map<SalesInvoiceResponseDto>(created);

        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<SalesInvoiceResponseDto>> UpdateAsync(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        invoice.Status = dto.Status;
        invoice.PaymentMethod = dto.PaymentMethod;
        invoice.PaidAmount = dto.PaidAmount;
        invoice.RemainingAmount = invoice.TotalAmount - dto.PaidAmount;
        invoice.Notes = dto.Notes;

        _unitOfWork.SalesInvoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(id), ct) ?? invoice;
        var responseDto = _mapper.Map<SalesInvoiceResponseDto>(updated);

        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<SalesInvoiceResponseDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        bool isCancelling = (status == InvoiceStatus.Cancelled || status == InvoiceStatus.Voided) &&
                            (invoice.Status != InvoiceStatus.Cancelled && invoice.Status != InvoiceStatus.Voided);

        invoice.Status = status;
        _unitOfWork.SalesInvoices.Update(invoice);

        // استعادة المخزون في حال إلغاء الفاتورة
        if (isCancelling && invoice.Items != null && invoice.Items.Any())
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(invoice.WarehouseId, ct);
            if (warehouse != null)
            {
                foreach (var item in invoice.Items)
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
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(id), ct) ?? invoice;
        var responseDto = _mapper.Map<SalesInvoiceResponseDto>(updated);

        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        _unitOfWork.SalesInvoices.SoftDelete(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
