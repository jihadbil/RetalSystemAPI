using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Purchase.Interfaces;
using RetalSystemAPI.Services.Purchase.Specifications;

namespace RetalSystemAPI.Services.Purchase.Implementations;

public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PurchaseInvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<PagedResult<PurchaseInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? supplierId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        var spec = new PurchaseInvoiceFilterSpec(
            supplierId, branchId, warehouseId, status, paymentMethod, fromDate, toDate, searchTerm);

        var (items, totalCount) = await _unitOfWork.PurchaseInvoices.GetPagedAsync(spec, pageNumber, pageSize, ct);
        var dtos = _mapper.Map<IReadOnlyList<PurchaseInvoiceSummaryDto>>(items);
        var pagedResult = PagedResult<PurchaseInvoiceSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<PurchaseInvoiceSummaryDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        var dto = _mapper.Map<PurchaseInvoiceResponseDto>(invoice);
        return ServiceResult<PurchaseInvoiceResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        bool supplierExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Id == dto.SupplierId, ct);
        if (!supplierExists)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
        }

        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        bool numExists = await _unitOfWork.PurchaseInvoices.ExistsAsync(p => p.InvoiceNumber == dto.InvoiceNumber, ct);
        if (numExists)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("رقم فاتورة المشتريات مستخدم بالفعل", ErrorCodes.PurchaseOrderNumberExists);
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("يجب إضافة بند واحد على الأقل لفاتورة المشتريات", ErrorCodes.ValidationError);
        }

        var invoice = _mapper.Map<PurchaseInvoice>(dto);
        invoice.Items = dto.Items.Select(item => new PurchaseInvoiceItem
        {
            ProductId = item.ProductId,
            ProductBarCodeId = item.ProductBarCodeId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            DiscountAmount = item.DiscountAmount,
            LineTotal = Math.Max(0, (item.Quantity * item.UnitPrice) - item.DiscountAmount)
        }).ToList();

        invoice.SubTotal = invoice.Items.Sum(i => i.LineTotal);
        invoice.TotalAmount = Math.Max(0, invoice.SubTotal - invoice.DiscountAmount + invoice.TaxAmount);

        if (invoice.Status == InvoiceStatus.Paid)
        {
            invoice.PaidAmount = invoice.TotalAmount;
            invoice.RemainingAmount = 0;
        }
        else
        {
            invoice.RemainingAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        }

        await _unitOfWork.PurchaseInvoices.AddAsync(invoice, ct);

        // ── زيادة المخزون تلقائياً في المستودع المحدد ───────────────
        if (warehouse.Type == WarehouseType.Storge)
        {
            foreach (var item in invoice.Items)
            {
                if (!item.ProductBarCodeId.HasValue || item.ProductBarCodeId.Value == Guid.Empty)
                {
                    // محاولة جلب الباركود الافتراضي للصنف
                    var bc = await _unitOfWork.ProductBarCodes.FirstOrDefaultAsync(b => b.ProductId == item.ProductId, ct);
                    if (bc != null)
                    {
                        item.ProductBarCodeId = bc.Id;
                    }
                }

                if (!item.ProductBarCodeId.HasValue || item.ProductBarCodeId.Value == Guid.Empty) continue;

                var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductBarcodeId == item.ProductBarCodeId.Value, ct);

                if (stock != null)
                {
                    var stockToUpdate = await _unitOfWork.StorgeStocks.GetByIdAsync(stock.Id, ct);
                    if (stockToUpdate != null)
                    {
                        stockToUpdate.Quantity += (int)item.Quantity;
                        _unitOfWork.StorgeStocks.Update(stockToUpdate);
                    }
                }
                else
                {
                    var newStock = new StorgeStock
                    {
                        WarehouseId = warehouse.Id,
                        ProductBarcodeId = item.ProductBarCodeId.Value,
                        Quantity = (int)item.Quantity,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                }
            }
        }
        else if (warehouse.Type == WarehouseType.Show)
        {
            foreach (var item in invoice.Items)
            {
                var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductId == item.ProductId, ct);

                if (stock != null)
                {
                    var stockToUpdate = await _unitOfWork.ShowroomStocks.GetByIdAsync(stock.Id, ct);
                    if (stockToUpdate != null)
                    {
                        stockToUpdate.Quantity += (int)item.Quantity;
                        _unitOfWork.ShowroomStocks.Update(stockToUpdate);
                    }
                }
                else
                {
                    var newStock = new ShowroomStock
                    {
                        WarehouseId = warehouse.Id,
                        ProductId = item.ProductId,
                        Quantity = (int)item.Quantity,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var createdInvoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(invoice.Id), ct) ?? invoice;
        var responseDto = _mapper.Map<PurchaseInvoiceResponseDto>(createdInvoice);

        return ServiceResult<PurchaseInvoiceResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> UpdateAsync(Guid id, UpdatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id, ct);
        if (invoice is null)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        invoice.InvoiceNumber = dto.InvoiceNumber;
        invoice.InvoiceDate = dto.InvoiceDate;
        invoice.SupplierId = dto.SupplierId;
        invoice.BranchId = dto.BranchId;
        invoice.WarehouseId = dto.WarehouseId;
        invoice.Status = dto.Status;
        invoice.PaymentMethod = dto.PaymentMethod;
        invoice.DiscountAmount = dto.DiscountAmount;
        invoice.TaxAmount = dto.TaxAmount;
        invoice.PaidAmount = dto.PaidAmount;
        invoice.Notes = dto.Notes;
        invoice.TotalAmount = Math.Max(0, invoice.SubTotal - invoice.DiscountAmount + invoice.TaxAmount);
        invoice.RemainingAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);

        _unitOfWork.PurchaseInvoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        var updatedInvoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct) ?? invoice;
        var responseDto = _mapper.Map<PurchaseInvoiceResponseDto>(updatedInvoice);

        return ServiceResult<PurchaseInvoiceResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id, ct);
        if (invoice is null)
        {
            return ServiceResult.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return ServiceResult.Failure("الفاتورة ملغاة بالفعل", ErrorCodes.PurchaseOrderInvalidStatus);
        }

        invoice.Status = InvoiceStatus.Cancelled;
        _unitOfWork.PurchaseInvoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id, ct);
        if (invoice is null)
        {
            return ServiceResult.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        _unitOfWork.PurchaseInvoices.SoftDelete(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
